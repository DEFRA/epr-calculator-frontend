using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class ParameterUploadFileControllerTests
{
    private const string TestUserName = "test.user@paycal";
    private ParameterUploadFileController controller = null!;
    private DefaultHttpContext httpContext = null!;
    private TempDataDictionary tempData = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        httpContext = CreateHttpContext(TestUserName);
        tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        controller = CreateController(httpContext, tempData);
    }

    [TestMethod]
    public void Index_ReturnsExpectedViewAndViewModel()
    {
        // Arrange

        // Act
        var result = controller.Index() as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
    }

    [TestMethod]
    public async Task UploadGet_FilePathMissing_RedirectsToStandardError()
    {
        // Arrange

        // Act
        var result = await controller.Upload() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task UploadGet_FilePathInvalid_RedirectsToStandardError()
    {
        // Arrange
        controller.TempData["FilePath"] = @"C:\does-not-exist\missing-parameter-file.csv";

        // Act
        var result = await controller.Upload() as RedirectToActionResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    [TestMethod]
    public async Task UploadGet_ValidCsvPath_ReturnsRefreshViewWithParsedValues()
    {
        // Arrange
        var tempFilePath = CreateTemporaryFile(".csv", BuildParameterCsvContent(("COMC-AL", "2210.45"), ("COMC-FC", "2210.00")));
        controller.TempData["FilePath"] = tempFilePath;

        try
        {
            // Act
            var result = await controller.Upload() as ViewResult;
            var viewModel = result?.Model as ParameterRefreshViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.ParameterUploadFileRefresh, result.ViewName);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(Path.GetFileName(tempFilePath), viewModel.FileName);
            Assert.AreEqual(2, viewModel.ParameterTemplateValues.Count);
            Assert.AreEqual("COMC-AL", viewModel.ParameterTemplateValues[0].ParameterUniqueReferenceId);
            Assert.AreEqual("2210.45", viewModel.ParameterTemplateValues[0].ParameterValue);
            Assert.AreEqual("COMC-FC", viewModel.ParameterTemplateValues[1].ParameterUniqueReferenceId);
            Assert.AreEqual("2210.00", viewModel.ParameterTemplateValues[1].ParameterValue);
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }

    [TestMethod]
    public async Task UploadPost_FileIsNull_ReturnsIndexViewWithValidationError()
    {
        // Arrange

        // Act
        var result = await controller.Upload(null!) as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Errors);
        Assert.AreEqual(ErrorMessages.FileNotSelected, viewModel.Errors.ErrorMessage);
        Assert.AreEqual(ViewControlNames.FileUpload, viewModel.Errors.DOMElementId);
    }

    [TestMethod]
    public async Task UploadPost_FileIsNotCsv_ReturnsIndexViewWithValidationError()
    {
        // Arrange
        var file = CreateFormFile("scheme-parameters.txt", BuildParameterCsvContent(("COMC-AL", "2210.45")));

        // Act
        var result = await controller.Upload(file) as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Errors);
        Assert.AreEqual(ErrorMessages.FileMustBeCSV, viewModel.Errors.ErrorMessage);
        Assert.AreEqual(ViewControlNames.FileUpload, viewModel.Errors.DOMElementId);
    }

    [TestMethod]
    public async Task UploadPost_FileExceedsSizeLimit_ReturnsIndexViewWithValidationError()
    {
        // Arrange
        var oversizedFileContent = new string('x', (int)StaticHelpers.MaxFileSize + 1);
        var file = CreateFormFile("scheme-parameters.csv", oversizedFileContent);

        // Act
        var result = await controller.Upload(file) as ViewResult;
        var viewModel = result?.Model as ParameterUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Errors);
        Assert.AreEqual(ErrorMessages.FileNotExceed50KB, viewModel.Errors.ErrorMessage);
        Assert.AreEqual(ViewControlNames.FileUpload, viewModel.Errors.DOMElementId);
    }

    [TestMethod]
    public async Task UploadPost_ValidCsv_ReturnsRefreshViewWithParsedValues()
    {
        // Arrange
        var file = CreateFormFile(
            "scheme-parameters.csv",
            BuildParameterCsvContent(("COMC-AL", "2210.45"), ("COMC-WD", "2210.00")));

        // Act
        var result = await controller.Upload(file) as ViewResult;
        var viewModel = result?.Model as ParameterRefreshViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.ParameterUploadFileRefresh, result.ViewName);
        Assert.IsNotNull(viewModel);
        Assert.AreEqual("scheme-parameters.csv", viewModel.FileName);
        Assert.AreEqual(2, viewModel.ParameterTemplateValues.Count);
        Assert.AreEqual("COMC-AL", viewModel.ParameterTemplateValues[0].ParameterUniqueReferenceId);
        Assert.AreEqual("2210.45", viewModel.ParameterTemplateValues[0].ParameterValue);
        Assert.AreEqual("COMC-WD", viewModel.ParameterTemplateValues[1].ParameterUniqueReferenceId);
        Assert.AreEqual("2210.00", viewModel.ParameterTemplateValues[1].ParameterValue);
    }

    [TestMethod]
    public void ParameterUploadFileController_DownloadCSVTemplate_Test()
    {
        var result = controller.DownloadCsvTemplate() as PhysicalFileResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("DefaultParameterTemplate.xlsx", result.FileDownloadName);
        Assert.AreEqual(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            result.ContentType);
    }

    private static ParameterUploadFileController CreateController(HttpContext httpContext, ITempDataDictionary tempData)
    {
        var environment = new Mock<IWebHostEnvironment>();
        environment
            .Setup(e => e.WebRootPath)
            .Returns(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

        return new ParameterUploadFileController(environment.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = tempData
        };
    }

    private static DefaultHttpContext CreateHttpContext(string? userName)
    {
        var identity = string.IsNullOrWhiteSpace(userName)
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)]);

        return new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
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
            csvBuilder.AppendLine($"{row.ParameterId},Communication costs by material,Aluminium,0,999999,{row.ParameterValue}");

        return csvBuilder.ToString();
    }

    private static string CreateTemporaryFile(string extension, string content)
    {
        var temporaryFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        File.WriteAllText(temporaryFilePath, content);
        return temporaryFilePath;
    }
}
