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
public class LocalAuthorityUploadFileErrorControllerTests
{
    private const string TestUserName = "test.user@paycal";

    private LocalAuthorityUploadFileErrorController controller = null!;
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
        controller.HttpContext.Session.SetString(UploadFileErrorIds.LocalAuthorityUploadErrors, string.Empty);

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
        var validationErrors = new ValidationProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc9110",
            Errors = new Dictionary<string, string[]>
            {
                { "SomeProperty", [ "Country is missing", "Material is invalid" ] }
            }
        };
        SetUploadErrors(validationErrors);

        // Act
        var result = controller.Index() as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNull(viewModel.LapcapErrors);
        Assert.IsNotNull(viewModel.ValidationErrors);
        Assert.AreEqual(2, viewModel.ValidationErrors.Count);
    }

    [TestMethod]
    public void Index_WhenLapcapErrorsExist_ReturnsErrorViewWithLapcapErrorsAndSummary()
    {
        // Arrange
        var lapcapErrors = new List<CreateLapcapDataErrorDto>
        {
            new() { Message = "Total cost must be numeric", Description = string.Empty, Country = "England", Material = "Plastic", UniqueReference = "ENG-PLA" },
            new() { Message = "Country is required", Description = string.Empty, Country = string.Empty, Material = "Glass", UniqueReference = "UNK-GLA" }
        };
        SetUploadErrors(lapcapErrors);

        // Act
        var result = controller.Index() as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.LapcapErrors);
        Assert.AreEqual(2, viewModel.LapcapErrors.Count);
        Assert.IsNotNull(viewModel.ValidationErrors);
        Assert.AreEqual(1, viewModel.ValidationErrors.Count);
        Assert.AreEqual("The file contained 2 errors.", viewModel.ValidationErrors[0].ErrorMessage);
        CollectionAssert.AreEqual(
            lapcapErrors.Select(error => error.Message).ToList(),
            viewModel.LapcapErrors.Select(error => error.Message).ToList());
    }

    [TestMethod]
    public void Index_WhenSingleLapcapErrorExists_ReturnsSingularSummaryMessage()
    {
        // Arrange
        var lapcapErrors = new List<CreateLapcapDataErrorDto>
        {
            new() { Message = "Country is required", Description = string.Empty, Country = string.Empty, Material = "Paper", UniqueReference = "UNK-PAP" }
        };
        SetUploadErrors(lapcapErrors);

        // Act
        var result = controller.Index() as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.ValidationErrors);
        Assert.AreEqual("The file contained 1 error.", viewModel.ValidationErrors[0].ErrorMessage);
    }

    [TestMethod]
    public void IndexPost_WhenErrorsProvided_PersistsErrorsAndReturnsOk()
    {
        // Arrange
        var errors = "[{\"ErrorMessage\":\"File processing failed\"}]";

        // Act
        var result = controller.Index(errors) as OkResult;
        var storedErrors = controller.HttpContext.Session.GetString(UploadFileErrorIds.LocalAuthorityUploadErrors);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(errors, storedErrors);
    }

    [TestMethod]
    public async Task Upload_WhenFileExtensionIsNotCsv_ReturnsErrorView()
    {
        // Arrange
        var fileUpload = CreateFormFile("local-authority.xlsx", BuildLapcapCsvContent(("England", "Paper", "123.45")));

        // Act
        var result = await controller.Upload(fileUpload) as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileErrorIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Errors);
        Assert.AreEqual(1, viewModel.Errors.Count);
        Assert.AreEqual(ErrorMessages.FileMustBeCSV, viewModel.Errors[0].ErrorMessage);
    }

    [TestMethod]
    public async Task Upload_WhenCsvIsValid_ReturnsRefreshViewWithParsedRows()
    {
        // Arrange
        var fileUpload = CreateFormFile(
            "local-authority.csv",
            BuildLapcapCsvContent(("England", "Paper", "123.45"), ("Scotland", "Plastic", "456.78")));

        // Act
        var result = await controller.Upload(fileUpload) as ViewResult;
        var viewModel = result?.Model as LapcapRefreshViewModel;
        var parsedRows = viewModel?.LapcapTemplateValue.ToList();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileRefresh, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("local-authority.csv", viewModel.FileName);
        Assert.IsNotNull(parsedRows);
        Assert.AreEqual(2, parsedRows.Count);
        Assert.AreEqual("England", parsedRows[0].CountryName);
        Assert.AreEqual("Paper", parsedRows[0].Material);
        Assert.AreEqual("123.45", parsedRows[0].TotalCost);
        Assert.AreEqual("Scotland", parsedRows[1].CountryName);
        Assert.AreEqual("Plastic", parsedRows[1].Material);
        Assert.AreEqual("456.78", parsedRows[1].TotalCost);
    }

    private void SetUploadErrors(object errors)
    {
        controller.HttpContext.Session.SetString(
            UploadFileErrorIds.LocalAuthorityUploadErrors,
            JsonConvert.SerializeObject(errors));
    }

    private static LocalAuthorityUploadFileErrorController CreateController(HttpContext httpContext)
    {
        return new LocalAuthorityUploadFileErrorController
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
        session.SetupGet(s => s.Id).Returns("local-authority-upload-error-session");
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

    private static string BuildLapcapCsvContent(params (string Country, string Material, string TotalCost)[] rows)
    {
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Country,Material,Total Cost");

        foreach (var row in rows)
            csvBuilder.AppendLine($"{row.Country},{row.Material},{row.TotalCost}");

        return csvBuilder.ToString();
    }
}
