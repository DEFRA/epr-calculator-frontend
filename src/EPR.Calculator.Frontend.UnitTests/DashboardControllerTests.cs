using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DashboardControllerTests
    {
        private readonly IConfiguration configuration = ConfigurationItems.GetConfigurationValues();

        private Fixture Fixture { get; } = new Fixture();

        [TestMethod]
        public async Task DashboardController_Success_View_Test()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculationRuns()))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object);
            controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            var resultModel = result.Model as DashboardViewModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(3, resultModel.Calculations.Count());
            Assert.AreEqual(0, resultModel.Calculations.Count(x => x.Id == 12));
        }

        [TestMethod]
        public async Task DashboardController_Success_No_Data_View_Test()
        {
            var content = "No data available for the specified year.Please check the year and try again.";
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(content)
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object);

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DashboardController_Failure_View_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Test content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object);

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task DashboardController_Failure_WhenNullConfiguration_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Test content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var config = configuration;
            config.GetSection(ConfigSection.DashboardCalculatorRun).Value = string.Empty;
            var controller = new DashboardController(config, mockHttpClientFactory.Object);

            var result = controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void Index_RedirectsToStandardError_WhenExceptionIsThrown()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Test content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Throws(new Exception()); // Ensure exception is thrown when CreateClient is called

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object);

            // Act
            var result = controller.Index() as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void Should_Classify_CalculationRuns_And_Handle_DefaultValue()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 1, CalculatorRunClassificationId = 1, Name = "Default cettings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.InTheQueue, Financial_Year = "2024-25" },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = 2, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Running, Financial_Year = "2024-25" },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = 3, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Unclassified, Financial_Year = "2024-25" },
                new CalculationRun { Id = 5, CalculatorRunClassificationId = 5, Name = "Test 5", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Error, Financial_Year = "2024-25" },
            };

            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<DashboardViewModel.CalculationRunViewModel>();

            // Act
            if (calculationRuns.Count > 0)
            {
                foreach (var calculationRun in calculationRuns)
                {
                    var classification_val = runClassifications.FirstOrDefault(c => (int)c == calculationRun.CalculatorRunClassificationId);
                    var member = typeof(RunClassification).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == classification_val.ToString());

                    var attribute = member?.GetCustomAttribute<EnumMemberAttribute>(false);

                    calculationRun.Status = attribute?.Value ?? string.Empty; // Use a default value if attribute or value is null

                    dashboardRunData.Add(new DashboardViewModel.CalculationRunViewModel(calculationRun));
                }
            }

            // Assert
            Assert.AreEqual(4, dashboardRunData.Count);
            Assert.AreEqual(CalculationRunStatus.InTheQueue, dashboardRunData.First().Status);
            Assert.AreEqual(CalculationRunStatus.Running, dashboardRunData[1].Status);
            Assert.AreEqual(CalculationRunStatus.Unclassified, dashboardRunData[2].Status);
            Assert.AreEqual(CalculationRunStatus.Error, dashboardRunData.Last().Status); // Default value
        }

        [TestMethod]
        public async Task Index_ShowsErrorLink_WhenStatusIsError()
        {
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 5, CalculatorRunClassificationId = 5, Name = "Test Run", CreatedAt = DateTime.Parse("30/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.Error, Financial_Year = "2024-25" },
                new CalculationRun { Id = 10, CalculatorRunClassificationId = 1, Name = "Test 5", CreatedAt = DateTime.Parse("30/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Status = CalculationRunStatus.InTheQueue, Financial_Year = "2024-25" },
            };

            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                   .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(calculationRuns))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            // Act
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object);
            var result = controller.Index() as ViewResult;
            var model = result?.Model as DashboardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(2, model.Calculations.Count());
            Assert.AreEqual(CalculationRunStatus.Error, model.Calculations.First().Status);
            Assert.IsTrue(model.Calculations.First().ShowErrorLink);
        }
    }
}
