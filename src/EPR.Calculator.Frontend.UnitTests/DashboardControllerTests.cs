using AutoFixture;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Principal;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DashboardControllerTests
    {
        private readonly IConfiguration configuration = ConfigurationItems.GetConfigurationValues();
        private readonly List<FinancialYearDto> financialYears;

        public DashboardControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            this.MockHttpContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);

            this.financialYears = new List<FinancialYearDto>
            {
                new FinancialYearDto { Name = "2025-26" },
                new FinancialYearDto { Name = "2024-25" },
                new FinancialYearDto { Name = "2023-24" }
            };
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public async Task DashboardController_Success_View_Test()
        {
            // Arrange
            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, MockData.GetCalculationRuns());

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            mockContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();

            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var mockClient = new TelemetryClient(new TelemetryConfiguration());

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, mockClient);
            controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

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
            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, MockData.GetCalculationRuns());

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            mockContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();

            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var mockClient = new TelemetryClient();

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, mockClient);
            controller.ControllerContext = new ControllerContext { HttpContext = mockContext.Object };

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
            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var noDataContent = "No data available for the specified year.Please check the year and try again.";
            var calculationRunsResponse = CreateResponse(HttpStatusCode.NotFound, noDataContent);

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, telemetryClient);

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");
            mockHttpSession.SetString(SessionConstants.FinancialYear, "2024-25");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Success_No_Data_View_Test()
        {
            // Arrange
            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var noDataContent = "No data available for the specified year.Please check the year and try again.";
            var calculationRunsResponse = CreateResponse(HttpStatusCode.NotFound, noDataContent);

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var telemetryClient = new TelemetryClient(new TelemetryConfiguration());

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, telemetryClient);

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString("accessToken", "something");
            mockHttpSession.SetString(SessionConstants.FinancialYear, "2024-25");

            var context = new DefaultHttpContext()
            {
                User = principal,
                Session = mockHttpSession
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DashboardController_Failure_View_Test()
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
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, new TelemetryClient());

            var result = await controller.Index() as RedirectToActionResult;
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
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, new TelemetryClient());

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
            var controller = new DashboardController(config, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, mockClient);

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
            var controller = new DashboardController(config, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, mockClient);

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
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, new TelemetryClient());

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
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, new TelemetryClient());

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

            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, calculationRuns);

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, telemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

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
                new CalculationRun
                {
                    Id = 10,
                    CalculatorRunClassificationId = RunClassification.INITIAL_RUN_COMPLETED,
                    Name = "Test 6",
                    CreatedAt = DateTime.Parse("30/06/2025 12:09:00", new CultureInfo("en-GB")),
                    CreatedBy = "Jamie Roberts",
                    Financial_Year = "2024-25"
                }
            };

            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, calculationRuns);

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var telemetryClient = new TelemetryClient(TelemetryConfiguration.CreateDefault());

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, telemetryClient);
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

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
        public async Task Index_CalculationsShouldBeNull_WhenApiReturnsBadRequest()
        {
            // Arrange
            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.BadRequest, "Test content");

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, new TelemetryClient(TelemetryConfiguration.CreateDefault()));
            controller.ControllerContext.HttpContext = this.MockHttpContext.Object;

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as DashboardViewModel;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Calculations == null || !model.Calculations.Any());
        }

        [TestMethod]
        public async Task Index_ShowDetailedError_WhenExceptionIsThrown_TokenAsync()
        {
            // Arrange
            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .Throws(new MsalUiRequiredException("Test", "No account or login hint passed"));

            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, new List<CalculationRun>());

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(SessionConstants.FinancialYear, "2024-25");

            configuration["ShowDetailedError"] = "true";
            var telemetryClient = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration());
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, telemetryClient);

            var context = new DefaultHttpContext()
            {
                Session = mockHttpSession
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act & Assert
            var task = controller.Index();
            AggregateException ex = Assert.ThrowsException<AggregateException>(task.Wait);
            Assert.IsInstanceOfType(ex.InnerException, typeof(MsalUiRequiredException));
            Assert.AreEqual("No account or login hint passed", ex.InnerException.Message);
        }

        [TestMethod]
        public async Task GetCalculations_ShowDetailedError_WhenExceptionIsThrown_TokenAsync()
        {
            // Arrange
            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, MockData.GetCalculationRuns());

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null,
                    null, null))
                .Throws(new MsalUiRequiredException("Test", "No account or login hint passed"));

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

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient); // Ensure exception is thrown when CreateClient is called

            configuration["ShowDetailedError"] = "true";
            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, new TelemetryClient(TelemetryConfiguration.CreateDefault()));

            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var mockHttpSession = new MockHttpSession();
            mockHttpSession.SetString(SessionConstants.FinancialYear, "2024-25");

            var context = new DefaultHttpContext
            {
                User = principal,
                Session = mockHttpSession
            };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var task = controller.GetCalculations("2024-25");

            // Assert
            AggregateException ex = Assert.ThrowsException<AggregateException>(task.Wait);
            Assert.AreEqual("One or more errors occurred. (No account or login hint passed)", ex.Message);
        }

        [TestMethod]
        public async Task DashboardController_Index_Populates_FinancialYearSelectList()
        {
            // Arrange
            var calculationRuns = new List<CalculationRun>
            {
                new CalculationRun { Id = 10, CalculatorRunClassificationId = RunClassification.INITIAL_RUN_COMPLETED, Name = "Test 6", CreatedAt = DateTime.Parse("30/06/2025 12:09:00", new CultureInfo("en-GB")), CreatedBy = "Jamie Roberts", Financial_Year = "2024-25" },
            };

            var financialYearsResponse = CreateResponse(HttpStatusCode.OK, this.financialYears);
            var calculationRunsResponse = CreateResponse(HttpStatusCode.OK, calculationRuns);

            var mockHandler = CreateMockHttpMessageHandler(financialYearsResponse, calculationRunsResponse);
            var httpClient = new HttpClient(mockHandler);

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockAuthorizationHeaderProvider = new Mock<ITokenAcquisition>();
            mockAuthorizationHeaderProvider
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");

            var telemetryClient = new TelemetryClient();

            var controller = new DashboardController(configuration, mockHttpClientFactory.Object,
                mockAuthorizationHeaderProvider.Object, telemetryClient);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = this.MockHttpContext.Object
            };

            controller.HttpContext.Session.SetString(SessionConstants.FinancialYear, "2024-25");

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as DashboardViewModel;
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.FinancialYearSelectList);
            Assert.IsTrue(model.FinancialYearSelectList.Any());
            Assert.AreEqual("2025-26", model.FinancialYearSelectList.First().Value);
        }

        private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, object content)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content))
            };
        }

        private static HttpMessageHandler CreateMockHttpMessageHandler(
            HttpResponseMessage financialYearsResponse,
            HttpResponseMessage calculationRunsResponse)
        {
            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns<HttpRequestMessage, CancellationToken>((request, cancellationToken) =>
                {
                    var path = request.RequestUri!.AbsolutePath.ToLower();

                    if (path.Contains("financialyears"))
                    {
                        return Task.FromResult(financialYearsResponse);
                    }
                    else if (path.Contains("calculatorruns"))
                    {
                        return Task.FromResult(calculationRunsResponse);
                    }

                    return Task.FromResult(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.NotFound
                    });
                });

            return mockHandler.Object;
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
    }
}
