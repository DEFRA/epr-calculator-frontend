using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using AutoFixture;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
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

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DashboardControllerTests
    {
        private readonly IConfiguration configuration = ConfigurationItems.GetConfigurationValues();
        private readonly List<FinancialYearDto> financialYears;
        private readonly string financialYearsApiUrl;
        private readonly string dashboardCalculatorRunUrl;

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

            this.financialYearsApiUrl = "http://test/FinancialYearListApi";
            this.dashboardCalculatorRunUrl = "http://test/DashboardCalculatorRun/DashboardCalculatorRunApi";
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public async Task DashboardController_Success_View_Test()
        {
            // Arrange
            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(MockData.GetCalculationRuns())) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var resultModel = result.Model as DashboardViewModel;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(10, resultModel.Calculations.Count());
            Assert.AreEqual(0, resultModel.Calculations.Count(x => x.Id == 12));
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Success_View_Test()
        {
            // Arrange
            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(MockData.GetCalculationRuns())) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

            // Act
            var result = await controller.GetCalculations("2024-25") as PartialViewResult;

            // Assert
            Assert.IsNotNull(result);
            var resultModel = result.Model as IEnumerable<CalculationRunViewModel>;
            Assert.IsNotNull(resultModel);
            Assert.AreEqual(10, resultModel.Count());
            Assert.AreEqual(0, resultModel.Count(x => x.Id == 12));
        }

        [TestMethod]
        public async Task DashboardController_Success_No_Data_View_Test()
        {
            // Arrange
            var noDataContent = "No data available for the specified year.Please check the year and try again.";
            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, "2024-25"), (HttpStatusCode.NotFound, noDataContent) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task DashboardController_With_FinancialYear_Success_No_Data_View_Test()
        {
            // Arrange
            var noDataContent = "No data available for the specified year.Please check the year and try again.";
            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, "2024-25"), (HttpStatusCode.NotFound, noDataContent) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
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

            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(calculationRuns)) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

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

            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(calculationRuns)) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

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
            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, "2024-25"), (HttpStatusCode.BadRequest, "Test content") }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext { HttpContext = MockHttpContext.Object };

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
                {
                    Session = new MockHttpSession()
                }
            };

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
        public async Task DashboardController_Index_Populates_FinancialYearSelectList()
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
            var apiResponses = new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                { (HttpMethod.Get, this.financialYearsApiUrl, string.Empty), (HttpStatusCode.OK, JsonConvert.SerializeObject(financialYears)) },
                { (HttpMethod.Post, this.dashboardCalculatorRunUrl, "2024-25"), (HttpStatusCode.OK, JsonConvert.SerializeObject(calculationRuns)) }
            };

            var controller = BuildTestClass(Fixture, apiResponses);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = MockHttpContext.Object
            };

            controller.HttpContext.Session.SetString(SessionConstants.FinancialYear, "2024-25");

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as DashboardViewModel;
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.FinancialYearSelectList);
            Assert.IsTrue(model.FinancialYearSelectList.Count > 0);
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

        private static DashboardController BuildTestClass(
            Fixture fixture,
            Dictionary<(HttpMethod Method, string Url, string Argument), (HttpStatusCode StatusCode, string Response)> apiResponses,
            CalculatorRunDetailsViewModel details = null,
            IConfiguration configurationItems = null)
        {
            configurationItems ??= ConfigurationItems.GetConfigurationValues();
            details ??= fixture.Create<CalculatorRunDetailsViewModel>();

            var mockApiService = TestMockUtils.BuildMockApiService(apiResponses).Object;

            var testClass = new DashboardController(
                configurationItems,
                mockApiService,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);

            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}
