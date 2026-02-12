using System.Globalization;
using System.Net;
using AutoFixture;
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
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DashboardControllerTests
    {
        private const string YearBody = "{\"RelativeYear\":2024}";

        private readonly Fixture fixture = new Fixture();
        private readonly List<int> relativeYears;
        private readonly string relativeYearsApiUrl = "v1/RelativeYears";
        private readonly string dashboardCalculatorRunUrl = "v1/calculatorRuns";

        public DashboardControllerTests()
        {
            this.relativeYears = [2025, 2024, 2023];
        }

        [TestMethod]
        public async Task GetCalculations_ReturnsFilteredRuns()
        {
            Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)> apiResponses =
                this.BuildApiResponses(MockData.GetCalculationRuns());

            DashboardController controller = this.BuildController(apiResponses);
            controller.HttpContext.Session.SetInt32(SessionConstants.RelativeYear, 2024);

            PartialViewResult? result =
                await controller.GetCalculations(2024) as PartialViewResult;

            Assert.IsNotNull(result);

            IEnumerable<CalculationRunViewModel>? model =
                result.Model as IEnumerable<CalculationRunViewModel>;

            Assert.IsNotNull(model);
            Assert.AreEqual(10, model.Count());
            Assert.AreEqual(0, model.Count(x => x.Id == 12));
        }

        [TestMethod]
        public async Task Index_ShowsErrorLink_WhenStatusIsError()
        {
            List<CalculationRun> runs = new List<CalculationRun>
            {
                new CalculationRun
                {
                    Id = 5,
                    CalculatorRunClassificationId = RunClassification.ERROR,
                    Name = "Test Run",
                    CreatedAt = DateTime.Parse("30/06/2025 10:01:00", new CultureInfo("en-GB")),
                    CreatedBy = "Jamie Roberts",
                    RelativeYear = new RelativeYear(2024)
                }
            };

            Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)> apiResponses =
                this.BuildApiResponses(runs);

            DashboardController controller = this.BuildController(apiResponses);
            controller.HttpContext.Session.SetInt32(SessionConstants.RelativeYear, 2024);

            ViewResult? result = await controller.Index() as ViewResult;

            Assert.IsNotNull(result);

            DashboardViewModel? model = result.Model as DashboardViewModel;

            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Calculations);
            Assert.AreEqual(1, model.Calculations.Count());
            Assert.AreEqual(RunClassification.ERROR, model.Calculations.First().Status);
            Assert.IsTrue(model.Calculations.First().ShowErrorLink);
        }

        [TestMethod]
        public async Task Index_ShowsInitialRunCompleted()
        {
            List<CalculationRun> runs = new List<CalculationRun>
            {
                new CalculationRun
                {
                    Id = 10,
                    CalculatorRunClassificationId = RunClassification.INITIAL_RUN_COMPLETED,
                    Name = "Test",
                    CreatedAt = DateTime.Parse("30/06/2025 12:09:00", new CultureInfo("en-GB")),
                    CreatedBy = "Jamie Roberts",
                    RelativeYear = new RelativeYear(2024)
                }
            };

            Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)> apiResponses =
                this.BuildApiResponses(runs);

            DashboardController controller = this.BuildController(apiResponses);
            controller.HttpContext.Session.SetInt32(SessionConstants.RelativeYear, 2024);

            ViewResult? result = await controller.Index() as ViewResult;

            Assert.IsNotNull(result);

            DashboardViewModel? model = result.Model as DashboardViewModel;

            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Calculations);
            Assert.AreEqual(1, model.Calculations.Count());
            Assert.AreEqual(
                RunClassification.INITIAL_RUN_COMPLETED,
                model.Calculations.First().Status);
        }

        [TestMethod]
        public async Task Index_Calculations_Null_WhenApiFails()
        {
            Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)> apiResponses =
                new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
                {
                    {
                        (HttpMethod.Get, this.relativeYearsApiUrl, string.Empty),
                        (HttpStatusCode.OK, JsonConvert.SerializeObject(this.relativeYears))
                    },
                    {
                        (HttpMethod.Post, this.dashboardCalculatorRunUrl, YearBody),
                        (HttpStatusCode.BadRequest, "error")
                    }
                };

            DashboardController controller = this.BuildController(apiResponses);
            controller.HttpContext.Session.SetInt32(SessionConstants.RelativeYear, 2024);

            ViewResult? result = await controller.Index() as ViewResult;

            Assert.IsNotNull(result);

            DashboardViewModel? model = result.Model as DashboardViewModel;

            Assert.IsNotNull(model);
            Assert.IsTrue(model.Calculations == null || !model.Calculations.Any());
        }

        [TestMethod]
        public async Task Index_Populates_RelativeYearSelectList()
        {
            Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)> apiResponses =
                this.BuildApiResponses(MockData.GetCalculationRuns());

            DashboardController controller = this.BuildController(apiResponses);
            controller.HttpContext.Session.SetInt32(SessionConstants.RelativeYear, 2024);

            ViewResult? result = await controller.Index() as ViewResult;

            Assert.IsNotNull(result);

            DashboardViewModel? model = result!.Model as DashboardViewModel;

            Assert.IsNotNull(model);
            Assert.IsNotNull(model.RelativeYearSelectList);
            Assert.IsTrue(model.RelativeYearSelectList.Count > 0);
            Assert.AreEqual("2025", model.RelativeYearSelectList.First().Value);
        }

        private Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            BuildApiResponses(object runs)
        {
            return new Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)>
            {
                {
                    (HttpMethod.Get, this.relativeYearsApiUrl, string.Empty),
                    (HttpStatusCode.OK, JsonConvert.SerializeObject(this.relativeYears))
                },
                {
                    (HttpMethod.Post, this.dashboardCalculatorRunUrl, YearBody),
                    (HttpStatusCode.OK, JsonConvert.SerializeObject(runs))
                }
            };
        }

        private DashboardController BuildController(
            Dictionary<(HttpMethod, string, string), (HttpStatusCode, string)> apiResponses)
        {
            IConfiguration configuration = ConfigurationItems.GetConfigurationValues();

            var mockApiService =
                TestMockUtils.BuildMockApiService(apiResponses).Object;

            DashboardController controller = new DashboardController(
                configuration,
                mockApiService,
                new TelemetryClient(TelemetryConfiguration.CreateDefault()),
                TestMockUtils.BuildMockCalculatorRunDetailsService(this.fixture.Create<CalculatorRunDetailsViewModel>()).Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = TestMockUtils.BuildMockSession(this.fixture).Object
                }
            };

            return controller;
        }
    }
}
