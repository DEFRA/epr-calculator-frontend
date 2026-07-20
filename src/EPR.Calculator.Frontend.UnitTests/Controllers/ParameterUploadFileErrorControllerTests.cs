using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class ParameterUploadFileErrorControllerTests
{
    private const string TestUserName = "test.user@paycal";

    private ParameterUploadFileErrorController controller = null!;
    private Dictionary<string, byte[]> sessionStorage = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        sessionStorage = new Dictionary<string, byte[]>();
        controller = CreateController(CreateHttpContext(TestUserName, sessionStorage));
    }

    [TestMethod]
    public void Index_WhenSessionErrorsMissing_RedirectsToStandardError()
    {
        // Arrange

        // Act
        var result = controller.Index() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public void Index_WhenSessionErrorsEmpty_RedirectsToStandardError()
    {
        // Arrange
        controller.HttpContext.Session.SetString(UploadFileErrorIds.DefaultParameterUploadErrors, string.Empty);

        // Act
        var result = controller.Index() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public void Index_WhenValidationErrorsExist_ReturnsErrorViewWithValidationErrors()
    {
        // Arrange
        var validationErrors = new List<ValidationErrorDto>
        {
            new() { ErrorMessage = "Parameter Unique reference is incorrect", Exception = string.Empty },
            new() { ErrorMessage = "Parameter value is incorrect", Exception = string.Empty }
        };
        SetUploadErrors(validationErrors);

        // Act
        var result = controller.Index() as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNull(viewModel.ParamterErrors);
        Assert.IsNotNull(viewModel.ValidationErrors);
        Assert.AreEqual(2, viewModel.ValidationErrors.Count);
        CollectionAssert.AreEqual(
            validationErrors.Select(error => error.ErrorMessage).ToList(),
            viewModel.ValidationErrors.Select(error => error.ErrorMessage).ToList());
    }

    [TestMethod]
    public void Index_WhenParameterErrorsExist_ReturnsErrorViewWithParameterErrorsAndSummary()
    {
        // Arrange
        var parameterErrors = new List<CreateDefaultParameterSettingErrorDto>
        {
            new() { Message = "Parameter Unique reference is incorrect", Description = string.Empty },
            new() { Message = "Parameter value is incorrect", Description = string.Empty }
        };
        SetUploadErrors(parameterErrors);

        // Act
        var result = controller.Index() as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.ParamterErrors);
        Assert.AreEqual(2, viewModel.ParamterErrors.Count);
        Assert.AreEqual(1, viewModel.ValidationErrors?.Count);
        Assert.AreEqual("The file contained 2 errors.", viewModel.ValidationErrors?[0].ErrorMessage);
        CollectionAssert.AreEqual(
            parameterErrors.Select(error => error.Message).ToList(),
            viewModel.ParamterErrors.Select(error => error.Message).ToList());
    }

    [TestMethod]
    public void IndexPost_WhenErrorsProvided_PersistsErrorsAndReturnsOk()
    {
        // Arrange
        var errors = "[{\"ErrorMessage\":\"File processing failed\"}]";

        // Act
        var result = controller.Index(errors) as OkResult;
        var storedErrors = controller.HttpContext.Session.GetString(UploadFileErrorIds.DefaultParameterUploadErrors);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(errors, storedErrors);
    }

    [TestMethod]
    public async Task Upload_WhenFileExtensionIsNotCsv_ReturnsErrorView()
    {
        // Arrange
        var fileUpload = CreateFormFile("scheme-parameters.xlsx", BuildParameterCsvContent(("COMC-AL", "2210.45")));

        // Act
        var result = await controller.Upload(fileUpload) as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Errors);
        Assert.AreEqual(ErrorMessages.FileMustBeCSV, viewModel.Errors.ErrorMessage);
    }

    [TestMethod]
    public async Task Upload_WhenCsvIsValid_ReturnsRefreshViewWithParsedRows()
    {
        // Arrange
        var fileUpload = CreateFormFile(
            "scheme-parameters.csv",
            BuildParameterCsvContent(("COMC-AL", "2210.45"), ("COMC-FC", "2210.00")));

        // Act
        var result = await controller.Upload(fileUpload) as ViewResult;
        var viewModel = result?.Model as ParameterRefreshViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileRefresh, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("scheme-parameters.csv", viewModel.FileName);
        Assert.AreEqual(2, viewModel.ParameterTemplateValues.Count);
        Assert.AreEqual("COMC-AL", viewModel.ParameterTemplateValues[0].ParameterUniqueReferenceId);
        Assert.AreEqual("2210.45", viewModel.ParameterTemplateValues[0].ParameterValue);
        Assert.AreEqual("COMC-FC", viewModel.ParameterTemplateValues[1].ParameterUniqueReferenceId);
        Assert.AreEqual("2210.00", viewModel.ParameterTemplateValues[1].ParameterValue);
    }

    private void SetUploadErrors(object errors)
    {
        controller.HttpContext.Session.SetString(
            UploadFileErrorIds.DefaultParameterUploadErrors,
            JsonConvert.SerializeObject(errors));
    }

    private static ParameterUploadFileErrorController CreateController(HttpContext httpContext)
    {
        return new ParameterUploadFileErrorController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static DefaultHttpContext CreateHttpContext(string? userName, Dictionary<string, byte[]> sessionStorage)
    {
        var identity = string.IsNullOrWhiteSpace(userName)
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], "TestAuth");

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity),
            Session = BuildSession(sessionStorage)
        };
    }

    private static ISession BuildSession(Dictionary<string, byte[]> storage)
    {
        var session = new Mock<ISession>();
        session.SetupGet(s => s.Id).Returns("parameter-upload-error-session");
        session.SetupGet(s => s.IsAvailable).Returns(true);
        session.SetupGet(s => s.Keys).Returns(() => storage.Keys);
        session.Setup(s => s.Clear()).Callback(storage.Clear);
        session.Setup(s => s.Remove(It.IsAny<string>())).Callback<string>(key => storage.Remove(key));
        session.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => storage[key] = value);
        session.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
            .Returns((string key, out byte[]? value) =>
            {
                var found = storage.TryGetValue(key, out var bytes);
                value = bytes;
                return found;
            });
        session.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        session.Setup(s => s.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        return session.Object;
    }

    private static FormFile CreateFormFile(string fileName, string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(contentBytes);
        return new FormFile(stream, 0, contentBytes.Length, "fileUpload", fileName);
    }

    private static string BuildParameterCsvContent(params (string ParameterId, string ParameterValue)[] rows)
    {
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Parameter Unique Ref,Parameter Type,Parameter Category,Valid Range From,Valid Range To,Parameter Value");

        foreach (var row in rows)
            csvBuilder.AppendLine(
                $"{row.ParameterId},Communication costs by material,Aluminium,0,999999,{row.ParameterValue}");

        return csvBuilder.ToString();
    }
}
