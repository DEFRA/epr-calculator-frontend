using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DashboardControllerTests
    {
        [TestMethod]
        public async Task DashboardController_Success_View_Test()
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(MockData.GetCalculationRuns()))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new DashboardController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);

            var result = controller.Index() as ViewResult;
            Assert.IsNotNull(result);
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

            var controller = new DashboardController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);

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
            var controller = new DashboardController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);

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
            var config = ConfigurationItems.GetConfigurationValues();
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

            var controller = new DashboardController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);

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
                new CalculationRun { CalculatorRunClassificationId = 1 },
                new CalculationRun { CalculatorRunClassificationId = 2 },
                new CalculationRun { CalculatorRunClassificationId = 9 } // This will use the default value
            };

            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<DashboardViewModel>();

            // Act
            if (calculationRuns.Count > 0)
            {
                foreach (var calculationRun in calculationRuns)
                {
                    var classification_val = runClassifications.FirstOrDefault(c => (int)c == calculationRun.CalculatorRunClassificationId);
                    var member = typeof(RunClassification).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == classification_val.ToString());

                    var attribute = member?.GetCustomAttribute<EnumMemberAttribute>(false);

                    calculationRun.Status = attribute?.Value ?? string.Empty; // Use a default value if attribute or value is null

                    dashboardRunData.Add(new DashboardViewModel(calculationRun));
                }
            }

            // Assert
            Assert.AreEqual(3, dashboardRunData.Count);
            Assert.AreEqual("IN THE QUEUE", dashboardRunData[0].Status);
            Assert.AreEqual("RUNNING", dashboardRunData[1].Status);
            Assert.AreEqual(string.Empty, dashboardRunData[2].Status); // Default value
        }
    }
}
