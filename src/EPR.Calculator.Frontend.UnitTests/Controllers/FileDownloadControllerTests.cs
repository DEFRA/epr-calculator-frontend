using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class FileDownloadControllerTests
{
    private const int RunId = 123;

    private Mock<IEprCalculatorApiService> apiService = null!;
    private FileDownloadController controller = null!;
    private Mock<IFileDownloadService> fileDownloadService = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        fileDownloadService = new Mock<IFileDownloadService>();

        controller = new FileDownloadController(
            apiService.Object,
            fileDownloadService.Object,
            new TelemetryClient(TelemetryConfiguration.CreateDefault()))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public async Task DownloadResultFile_WhenDownloadSucceeds_ReturnsFileResult()
    {
        // Arrange
        var expectedResult = new FileContentResult([ 1, 2, 3 ], "application/octet-stream")
        {
            FileDownloadName = "result.csv"
        };

        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun());

        fileDownloadService
            .Setup(service => service.DownloadResultFile(RunId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await controller.DownloadResultFile(RunId);

        // Assert
        fileDownloadService.Verify(
            service => service.DownloadResultFile(RunId),
            Times.Once);

        Assert.AreSame(expectedResult, result);
    }

    [TestMethod]
    public async Task DownloadResultFile_WhenDownloadThrows_RedirectsToDownloadFileError()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun());

        fileDownloadService
            .Setup(service => service.DownloadResultFile(RunId))
            .ThrowsAsync(new InvalidOperationException("Download failed"));

        // Act
        var result = await controller.DownloadResultFile(RunId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual(nameof(FileDownloadController.DownloadError), redirectResult.ActionName);
    }

    [TestMethod]
    public async Task DownloadBillingFile_WhenRunDoesNotExist_RedirectsToStandardError()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync((CalculatorRunDto?)null);

        // Act
        var result = await controller.DownloadBillingFile(RunId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("StandardError", redirectResult.ControllerName);
        fileDownloadService.Verify(
            service => service.DownloadBillingFile(It.IsAny<int>(), It.IsAny<bool>()),
            Times.Never);
    }

    [TestMethod]
    public async Task DownloadBillingFile_WhenBillingFileIsNotLatest_RedirectsToRunOverview()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun());

        // Act
        var result = await controller.DownloadBillingFile(RunId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual(ActionNames.Index, redirectResult.ActionName);
        Assert.AreEqual(ControllerNames.CalculationRunOverview, redirectResult.ControllerName);
        Assert.AreEqual(RunId, redirectResult.RouteValues?["runId"]);
        fileDownloadService.Verify(
            service => service.DownloadBillingFile(It.IsAny<int>(), It.IsAny<bool>()),
            Times.Never);
    }

    [DataRow(true)]
    [DataRow(false)]
    [TestMethod]
    public async Task DownloadBillingFile_WhenRunAndBillingFileAreValid_ReturnsFileResult(bool hasBeenSentToFss)
    {
        // Arrange
        var expectedResult = new FileContentResult([ 9, 8, 7 ], "text/csv")
        {
            FileDownloadName = "billing.csv"
        };

        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun(true, hasBeenSentToFss));

        fileDownloadService
            .Setup(service => service.DownloadBillingFile(RunId, hasBeenSentToFss))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await controller.DownloadBillingFile(RunId);

        // Assert
        Assert.AreSame(expectedResult, result);
        fileDownloadService.Verify(
            service => service.DownloadBillingFile(RunId, hasBeenSentToFss),
            Times.Once);
    }

    [TestMethod]
    public async Task DownloadBillingFile_WhenFileDownloadThrows_RedirectsToDownloadFileError()
    {
        // Arrange
        apiService.Setup(service => service.GetCalculatorRun(RunId)).ReturnsAsync(BuildRun(true));
        fileDownloadService
            .Setup(service => service.DownloadBillingFile(It.IsAny<int>(), It.IsAny<bool>()))
            .ThrowsAsync(new InvalidOperationException("Billing file download failed"));

        // Act
        var result = await controller.DownloadBillingFile(RunId);

        // Assert
        var redirectResult = result as RedirectToActionResult;
        Assert.IsNotNull(redirectResult);
        Assert.AreEqual(nameof(FileDownloadController.DownloadError), redirectResult.ActionName);
    }

    private static CalculatorRunDto BuildRun(bool isBillingFileLatest = false, bool hasBeenSentToFss = false)
    {
        return new CalculatorRunDto
        {
            RunId = RunId,
            RunName = $"Run {RunId}",
            RunClassification = RunClassification.INITIAL_RUN,
            RelativeYear = new RelativeYear(2025),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "unit-test",
            BillingRunStatus = BillingRunStatus.None,
            BillingFile = new CalculatorRunDto.BillingFileDto
            {
                Id = 1,
                IsLatest = isBillingFileLatest,
                HasBeenSentToFss = hasBeenSentToFss,
                CsvFileName = "billing.csv",
                JsonFileName = "billing.json",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "unit-test"
            }
        };
    }
}
