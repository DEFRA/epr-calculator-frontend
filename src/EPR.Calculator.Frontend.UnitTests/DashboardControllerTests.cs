using System.Globalization;
using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DashboardControllerTests
    {
        private readonly IConfiguration configuration = ConfigurationItems.GetConfigurationValues();

        public DashboardControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            this.MockHttpContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public async Task DashboardController_Success_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, MockData.GetCalculationRuns());

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);

            var resultModel = result.Model as DashboardViewModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(3, resultModel.Calculations.Count());
            Assert.AreEqual(0, resultModel.Calculations.Count(x => x.Id == 12));
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Success_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, MockData.GetCalculationRuns());

            // Act
            var result = await controller.GetCalculations("2024-25") as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);

            var resultModel = result.Model as IEnumerable<CalculationRunViewModel>;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(3, resultModel.Count());
            Assert.AreEqual(0, resultModel.Count(x => x.Id == 12));
        }

        [TestMethod]
        public async Task DashboardController_Success_No_Data_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.NotFound);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Success_No_Data_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(this.Fixture, HttpStatusCode.NotFound);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DashboardController_Failure_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(
                this.Fixture,
                HttpStatusCode.BadRequest,
                // Pass an empty configuration to cause a failure when trying to read values from it.
                configurationItems: new Mock<IConfiguration>().Object);

            // Act
            var result = await controller.Index() as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Failure_View_Test()
        {
            var content = "Test content";
            var mockHttpMessageHandler = GetMockHttpMessageHandlerBadRequestMessage(content);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            var controller = new DashboardController(
                configuration,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            var result = await controller.GetCalculations("2024-25") as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task DashboardController_Failure_WhenNullConfiguration_Test()
        {
            var content = "Test content";
            var mockHttpMessageHandler = GetMockHttpMessageHandlerBadRequestMessage(content);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var config = configuration;
            config.GetSection(ConfigSection.DashboardCalculatorRun).Value = string.Empty;
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            var mockClient = new TelemetryClient();
            var controller = new DashboardController(
                config,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                mockClient,
                new Mock<ICalculatorRunDetailsService>().Object);

            var result = await controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Failure_WhenNullConfiguration_Test()
        {
            var content = "Test content";
            var mockHttpMessageHandler = GetMockHttpMessageHandlerBadRequestMessage(content);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var config = configuration;
            config.GetSection(ConfigSection.DashboardCalculatorRun).Value = string.Empty;
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            var mockClient = new TelemetryClient();
            var controller = new DashboardController(
                config,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                mockClient,
                new Mock<ICalculatorRunDetailsService>().Object);

            var result = await controller.GetCalculations("2024-25") as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void Index_RedirectsToStandardError_WhenExceptionIsThrown()
        {
            // Arrange
            var content = "Test content";
            var mockHttpMessageHandler = GetMockHttpMessageHandlerBadRequestMessage(content);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Throws(new Exception()); // Ensure exception is thrown when CreateClient is called
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            var controller = new DashboardController(
                configuration,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            // Act
            var task = controller.Index();
            task.Wait();

            var result = task.Result as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void Index_With_FinancialYear_RedirectsToStandardError_WhenExceptionIsThrown()
        {
            // Arrange
            var content = "Test content";
            var mockHttpMessageHandler = GetMockHttpMessageHandlerBadRequestMessage(content);

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Throws(new Exception()); // Ensure exception is thrown when CreateClient is called
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            var controller = new DashboardController(
                configuration,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            // Act
            var task = controller.GetCalculations("2024-25");
            task.Wait();

            var result = task.Result as RedirectToActionResult;

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
                new CalculationRun { Id = 1, CalculatorRunClassificationId = RunClassification.QUEUE, Name = "Default cettings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = RunClassification.UNCLASSIFIED, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = RunClassification.TEST_RUN, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 5, CalculatorRunClassificationId = RunClassification.ERROR, Name = "Test 5", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
            };

            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<CalculationRunViewModel>();

            // Act
            if (calculationRuns.Count > 0)
            {
                foreach (var calculationRun in calculationRuns)
                {
                    var classification_val = runClassifications.FirstOrDefault(c => c == calculationRun.CalculatorRunClassificationId);

                    dashboardRunData.Add(new CalculationRunViewModel(calculationRun));
                }
            }

            // Assert
            Assert.AreEqual(4, dashboardRunData.Count);
            Assert.AreEqual(RunClassification.QUEUE, dashboardRunData.First().Status);
            Assert.AreEqual(RunClassification.UNCLASSIFIED, dashboardRunData[1].Status);
            Assert.AreEqual(RunClassification.TEST_RUN, dashboardRunData[2].Status);
            Assert.AreEqual(RunClassification.ERROR, dashboardRunData.Last().Status); // Default value
        }

        [TestMethod]
        public void Should_Classify_CalculationRuns_And_Handle_InitialRun()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 1, CalculatorRunClassificationId = RunClassification.QUEUE, Name = "Default cettings check", CreatedAt = DateTime.Parse("28/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 2, CalculatorRunClassificationId = RunClassification.UNCLASSIFIED, Name = "Alteration check", CreatedAt = DateTime.Parse("28/06/2025 12:19:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 3, CalculatorRunClassificationId = RunClassification.TEST_RUN, Name = "Test 10", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 4, CalculatorRunClassificationId = RunClassification.INITIAL_RUN, Name = "Test 4", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 5, CalculatorRunClassificationId = RunClassification.ERROR, Name = "Test 5", CreatedAt = DateTime.Parse("21/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
            };

            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<CalculationRunViewModel>();

            // Act
            if (calculationRuns.Count > 0)
            {
                foreach (var calculationRun in calculationRuns)
                {
                    var classification_val = runClassifications.FirstOrDefault(c => c == calculationRun.CalculatorRunClassificationId);

                    dashboardRunData.Add(new CalculationRunViewModel(calculationRun));
                }
            }

            // Assert
            Assert.AreEqual(5, dashboardRunData.Count);
            Assert.AreEqual(RunClassification.QUEUE, dashboardRunData.First().Status);
            Assert.AreEqual(RunClassification.UNCLASSIFIED, dashboardRunData[1].Status);
            Assert.AreEqual(RunClassification.TEST_RUN, dashboardRunData[2].Status);
            Assert.AreEqual(RunClassification.ERROR, dashboardRunData.Last().Status);
            Assert.AreEqual(RunClassification.INITIAL_RUN, dashboardRunData[3].Status); // Initial Run
        }

        [TestMethod]
        public async Task Index_ShowsErrorLink_WhenStatusIsError()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 5, CalculatorRunClassificationId = RunClassification.ERROR, Name = "Test Run", CreatedAt = DateTime.Parse("30/06/2025 10:01:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
                new CalculationRun { Id = 10, CalculatorRunClassificationId = RunClassification.QUEUE, Name = "Test 5", CreatedAt = DateTime.Parse("30/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, calculationRuns);

            // Act
            var result = await controller.Index() as ViewResult;
            var model = result?.Model as DashboardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Calculations.Count());
            Assert.AreEqual(RunClassification.ERROR, model.Calculations.First().Status);
            Assert.IsTrue(model.Calculations.First().ShowErrorLink);
        }

        [TestMethod]
        public async Task Index_ShowInitialRunCompleted_WhenStatusInitialRunCompleted()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 10, CalculatorRunClassificationId = RunClassification.INITIAL_RUN_COMPLETED, Name = "Test 6", CreatedAt = DateTime.Parse("30/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
            };

            var controller = BuildTestClass(this.Fixture, HttpStatusCode.OK, calculationRuns);

            // Act
            var result = await controller.Index() as ViewResult;
            var model = result?.Model as DashboardViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.Calculations.Count());
            Assert.AreEqual(RunClassification.INITIAL_RUN_COMPLETED, model.Calculations.First().Status);
        }

        [TestMethod]
        public void Index_ShowDetailedError_WhenExceptionIsThrown()
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
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            configuration["ShowDetailedError"] = "true";
            var controller = new DashboardController(
                configuration,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Session = new MockHttpSession()
                }
            };

            // Act
            var task = controller.Index();
            Assert.ThrowsException<AggregateException>(task.Wait);
        }

        [TestMethod]
        public async Task Index_ShowDetailedError_WhenExceptionIsThrown_TokenAsync()
        {
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null,
                    null, null))
                .Throws(new MsalUiRequiredException("Test", "No account or login hint passed"));
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

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(SessionConstants.FinancialYear, "2024-25");

            configuration["ShowDetailedError"] = "true";
            var controller = new DashboardController(
                configuration,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            var context = new DefaultHttpContext()
            {
                Session = mockHttpSession
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var task = controller.Index();
            // Assert
            AggregateException ex = Assert.ThrowsException<AggregateException>(task.Wait);
            Assert.IsInstanceOfType(ex.InnerException, typeof(MsalUiRequiredException));
            Assert.AreEqual("No account or login hint passed", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task GetCalculations_ShowDetailedError_WhenExceptionIsThrown_TokenAsync()
        {
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null,
                    null, null))
                .Throws(new MsalUiRequiredException("Test", "No account or login hint passed"));
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

            configuration["ShowDetailedError"] = "true";
            var controller = new DashboardController(
                configuration,
                new Mock<IApiService>().Object,
                mockAuthorizationHeaderProvider.Object,
                new TelemetryClient(),
                new Mock<ICalculatorRunDetailsService>().Object);

            var task = controller.GetCalculations("2024-25");
            // Assert
            AggregateException ex = Assert.ThrowsException<AggregateException>(task.Wait);
            Assert.AreEqual("One or more errors occurred. (No account or login hint passed)", ex.Message);
        }

        private static Mock<HttpMessageHandler> GetMockHttpMessageHandler()
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
            return mockHttpMessageHandler;
        }

        private static Mock<HttpMessageHandler> GetMockHttpMessageHandler(HttpStatusCode statusCode, string content)
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
                    StatusCode = statusCode,
                    Content = new StringContent(content)
                });
            return mockHttpMessageHandler;
        }

        private static Mock<HttpMessageHandler> GetMockHttpMessageHandlerNotFoundMessage(string content)
        {
            return GetMockHttpMessageHandler(HttpStatusCode.NotFound, content);
        }

        private static Mock<HttpMessageHandler> GetMockHttpMessageHandlerBadRequestMessage(string content)
        {
            return GetMockHttpMessageHandler(HttpStatusCode.BadRequest, content);
        }

        private DashboardController BuildTestClass(
            Fixture fixture,
            HttpStatusCode httpStatusCode,
            object data = null,
            CalculatorRunDetailsViewModel details = null,
            IConfiguration configurationItems = null)
        {
            data = data ?? MockData.GetCalculatorRun();
            configurationItems = configurationItems ?? ConfigurationItems.GetConfigurationValues();
            details = details ?? Fixture.Create<CalculatorRunDetailsViewModel>();
            var mockApiService = TestMockUtils.BuildMockApiService(
                httpStatusCode,
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun())).Object;

            var testClass = new DashboardController(
                configurationItems,
                mockApiService,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}
