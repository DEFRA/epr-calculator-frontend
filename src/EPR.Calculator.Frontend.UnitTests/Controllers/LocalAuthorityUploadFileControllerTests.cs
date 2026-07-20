using System.Security.Claims;
using System.Text;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class LocalAuthorityUploadFileControllerTests
{
    private const string TestUserName = "test.user@paycal";
    private const string LapcapFilePathTempDataKey = "LapcapFilePath";

    private LocalAuthorityUploadFileController controller = null!;
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
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        Assert.IsNotNull(viewModel);
    }

    [TestMethod]
    public async Task UploadGet_FilePathMissing_RedirectsToStandardError()
    {
        // Arrange

        // Act
        var result = await controller.Upload() as RedirectToActionResult;

        // Assert
        AssertStandardErrorRedirect(result);
    }

    [TestMethod]
    public async Task UploadGet_FilePathInvalid_RedirectsToStandardError()
    {
        // Arrange
        controller.TempData[LapcapFilePathTempDataKey] = @"C:\does-not-exist\missing-local-authority-file.csv";

        // Act
        var result = await controller.Upload() as RedirectToActionResult;

        // Assert
        AssertStandardErrorRedirect(result);
    }

    [TestMethod]
    public async Task UploadGet_FileIsNotCsv_ReturnsIndexViewWithValidationError()
    {
        // Arrange
        var temporaryFilePath = CreateTemporaryFile(".txt", BuildLapcapCsvContent(("England", "Paper", "123.45")));
        controller.TempData[LapcapFilePathTempDataKey] = temporaryFilePath;

        try
        {
            // Act
            var result = await controller.Upload() as ViewResult;
            var viewModel = result?.Model as LapcapUploadViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
            AssertSingleUploadError(viewModel, ErrorMessages.FileMustBeCSV);
        }
        finally
        {
            File.Delete(temporaryFilePath);
        }
    }

    [TestMethod]
    public async Task UploadGet_ValidCsvPath_ReturnsRefreshViewWithParsedValues()
    {
        // Arrange
        var temporaryFilePath = CreateTemporaryFile(
            ".csv",
            BuildLapcapCsvContent(("England", "Paper", "123.45"), ("Scotland", "Plastic", "456.78")));
        controller.TempData[LapcapFilePathTempDataKey] = temporaryFilePath;

        try
        {
            // Act
            var result = await controller.Upload() as ViewResult;
            var viewModel = result?.Model as LapcapRefreshViewModel;
            var parsedRows = viewModel?.LapcapTemplateValue.ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.LocalAuthorityUploadFileRefresh, result.ViewName);
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(Path.GetFileName(temporaryFilePath), viewModel.FileName);
            Assert.IsNotNull(parsedRows);
            Assert.AreEqual(2, parsedRows.Count);
            Assert.AreEqual("England", parsedRows[0].CountryName);
            Assert.AreEqual("Paper", parsedRows[0].Material);
            Assert.AreEqual("123.45", parsedRows[0].TotalCost);
            Assert.AreEqual("Scotland", parsedRows[1].CountryName);
            Assert.AreEqual("Plastic", parsedRows[1].Material);
            Assert.AreEqual("456.78", parsedRows[1].TotalCost);
        }
        finally
        {
            File.Delete(temporaryFilePath);
        }
    }

    [TestMethod]
    public async Task UploadPost_FileIsNull_ReturnsIndexViewWithValidationError()
    {
        // Arrange

        // Act
        var result = await controller.Upload(null!) as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        AssertSingleUploadError(viewModel, ErrorMessages.FileNotSelected);
    }

    [TestMethod]
    public async Task UploadPost_FileIsNotCsv_ReturnsIndexViewWithValidationError()
    {
        // Arrange
        var file = CreateFormFile("local-authority.txt", BuildLapcapCsvContent(("England", "Paper", "123.45")));

        // Act
        var result = await controller.Upload(file) as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        AssertSingleUploadError(viewModel, ErrorMessages.FileMustBeCSV);
    }

    [TestMethod]
    public async Task UploadPost_FileExceedsSizeLimit_ReturnsIndexViewWithValidationError()
    {
        // Arrange
        var oversizedContent = new string('x', (int)StaticHelpers.MaxFileSize + 1);
        var file = CreateFormFile("local-authority.csv", oversizedContent);

        // Act
        var result = await controller.Upload(file) as ViewResult;
        var viewModel = result?.Model as LapcapUploadViewModel;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(ViewNames.LocalAuthorityUploadFileIndex, result.ViewName);
        AssertSingleUploadError(viewModel, ErrorMessages.FileNotExceed50KB);
    }

    [TestMethod]
    public async Task UploadPost_ValidCsv_ReturnsRefreshViewWithParsedValues()
    {
        // Arrange
        var file = CreateFormFile(
            "local-authority.csv",
            BuildLapcapCsvContent(("England", "Paper", "123.45"), ("Scotland", "Plastic", "456.78")));

        // Act
        var result = await controller.Upload(file) as ViewResult;
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

    private static LocalAuthorityUploadFileController CreateController(HttpContext httpContext, ITempDataDictionary tempData)
    {
        return new LocalAuthorityUploadFileController
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext },
            TempData = tempData
        };
    }

    private static DefaultHttpContext CreateHttpContext(string? userName)
    {
        var identity = string.IsNullOrWhiteSpace(userName)
            ? new ClaimsIdentity()
            : new ClaimsIdentity([new Claim(ClaimTypes.Name, userName)], "TestAuth");

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

    private static string BuildLapcapCsvContent(params (string Country, string Material, string TotalCost)[] rows)
    {
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Country,Material,Total Cost");

        foreach (var row in rows)
            csvBuilder.AppendLine($"{row.Country},{row.Material},{row.TotalCost}");

        return csvBuilder.ToString();
    }

    private static string CreateTemporaryFile(string extension, string content)
    {
        var temporaryFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
        File.WriteAllText(temporaryFilePath, content);
        return temporaryFilePath;
    }

    private static void AssertStandardErrorRedirect(RedirectToActionResult? result)
    {
        Assert.IsNotNull(result);
        Assert.AreEqual("Index", result.ActionName);
        Assert.AreEqual("StandardError", result.ControllerName);
    }

    private static void AssertSingleUploadError(LapcapUploadViewModel? viewModel, string expectedErrorMessage)
    {
        Assert.IsNotNull(viewModel);
        Assert.IsNotNull(viewModel.Errors);
        Assert.AreEqual(1, viewModel.Errors.Count);
        Assert.AreEqual(expectedErrorMessage, viewModel.Errors[0].ErrorMessage);
        Assert.AreEqual(ViewControlNames.FileUpload, viewModel.Errors[0].DOMElementId);
    }
}
