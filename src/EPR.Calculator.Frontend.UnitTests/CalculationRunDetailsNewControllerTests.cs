using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class CalculationRunDetailsNewControllerTests
    {
        private readonly IConfiguration _configuration = ConfigurationItems.GetConfigurationValues();
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<ILogger<CalculationRunDetailsNewController>> _mockLogger;
        private Mock<ITokenAcquisition> _mockTokenAcquisition;
        private TelemetryClient _telemetryClient;
        private CalculationRunDetailsNewController _controller;
        private Mock<HttpContext> _mockHttpContext;

        [TestInitialize]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<CalculationRunDetailsNewController>>();
            _mockTokenAcquisition = new Mock<ITokenAcquisition>();
            _telemetryClient = new TelemetryClient();
            _mockHttpContext = new Mock<HttpContext>();

            _controller = new CalculationRunDetailsNewController(
                   _configuration,
                   _mockHttpClientFactory.Object,
                   _mockLogger.Object,
                   _mockTokenAcquisition.Object,
                   _telemetryClient);

            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "Test User")
            }));

            // Setting the mocked HttpContext for the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task Index_ValidRunId_ReturnsViewResult()
        {
            int runId = 240008;

            var result = await _controller.Index(runId);

            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(CalculatorRunDetailsNewViewModel));
        }

        [TestMethod]
        public async Task Submit_InvalidModelState_ReturnsRedirectToAction()
        {
            // Arrange
            int runId = 240008;
            _controller.ModelState.AddModelError("Error", "Model error");

            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
            {
                RunId = runId,
                RunName = "Test Run"
            },
                SelectedCalcRunOption = null
            };

            // Act
            var result = await _controller.Submit(model) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewIndex, result.ViewName);
            var viewModel = result.Model as CalculatorRunDetailsNewViewModel;
            Assert.IsNotNull(viewModel);
            Assert.AreEqual(model.RunId, viewModel.RunId);
        }

        [TestMethod]
        public async Task Submit_ValidModelState_RedirectsToCorrectAction()
        {
            // Arrange
            int runId = 240008;
            var selectedOption = CalculationRunOption.OutputClassify;

            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = runId,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = selectedOption
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyingCalculationRun, result.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ValidModelState_OutputClassify_ReturnsRedirectToAction()
        {
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 240008,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = CalculationRunOption.OutputClassify
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.ClassifyingCalculationRun, result.ControllerName);
            Assert.AreEqual(240008, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async void Submit_ValidModelState_OutputDelete_ReturnsRedirectToAction()
        {
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 240008,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = CalculationRunOption.OutputDelete
            };
            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(ControllerNames.CalculationRunDelete, result.ControllerName);
            Assert.AreEqual(240008, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async void Submit_ValidModelState_Default_ReturnsRedirectToAction()
        {
            var model = new CalculatorRunDetailsNewViewModel()
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel()
                {
                    RunId = 240008,
                    RunName = "Test Run"
                },
                SelectedCalcRunOption = (CalculationRunOption)999
            };

            // Act
            var result = await _controller.Submit(model) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
            Assert.AreEqual(240008, result.RouteValues["runId"]);
        }

        [TestMethod]
        public async void Index_WhenRunIsNotEligible_ReturnsErrorView()
        {
            // Arrange
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);
            var context = new DefaultHttpContext()
            {
                User = principal,
            };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            int runId = 190508;
            var run = new CalculatorRunDto { RunClassificationId = 5 };

            // Act
            var result = await _controller.Index(runId) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.CalculationRunDetailsNewErrorPage, result.ViewName);
        }
    }
}
