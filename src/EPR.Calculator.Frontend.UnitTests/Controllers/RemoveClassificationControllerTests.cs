using System.Net;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class RemoveClassificationControllerTests
    {
        private Mock<IConfiguration> configurationMock;
        private Mock<IApiService> apiServiceMock;
        private Mock<ILogger<RemoveClassificationController>> loggerMock;
        private Mock<ITokenAcquisition> tokenAcquisitionMock;
        private Mock<TelemetryClient> telemetryClientMock;
        private Mock<ICalculatorRunDetailsService> runDetailsServiceMock;

        private RemoveClassificationController controller;

        [TestInitialize]
        public void Setup()
        {
            configurationMock = new Mock<IConfiguration>();
            apiServiceMock = new Mock<IApiService>();
            loggerMock = new Mock<ILogger<RemoveClassificationController>>();
            tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            telemetryClientMock = new Mock<TelemetryClient>();
            runDetailsServiceMock = new Mock<ICalculatorRunDetailsService>();

            controller = new RemoveClassificationController(
                configurationMock.Object,
                apiServiceMock.Object,
                loggerMock.Object,
                tokenAcquisitionMock.Object,
                telemetryClientMock.Object,
                runDetailsServiceMock.Object);

            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestMethod]
        public async Task Index_WithValidRunId_ReturnsViewWithViewModel()
        {
            int runId = 123;
            var runDetails = new CalculatorRunDetailsViewModel { RunId = runId };
            runDetailsServiceMock
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), runId))
                .ReturnsAsync(runDetails);

            var result = await controller.Index(runId);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult);

            Assert.IsInstanceOfType(viewResult.Model, typeof(SetRunClassificationViewModel));
            var model = (SetRunClassificationViewModel)viewResult.Model;
            Assert.IsNotNull(model);
            Assert.AreEqual(runId, model.CalculatorRunDetails.RunId);
            Assert.IsNull(model.ClassifyRunType);
        }

        [TestMethod]
        public async Task Index_WhenExceptionThrown_RedirectsToError()
        {
            int runId = 123;
            runDetailsServiceMock
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), runId))
                .ThrowsAsync(new Exception("Test Exception"));

            var result = await controller.Index(runId);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task Submit_ModelStateInvalid_ReturnsViewWithViewModel()
        {
            // Arrange
            controller.ModelState.AddModelError("ClassifyRunType", "Required");

            var model = new SetRunClassificationViewModel
            {
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 }
            };

            runDetailsServiceMock
                .Setup(s => s.GetCalculatorRundetailsAsync(It.IsAny<HttpContext>(), model.CalculatorRunDetails.RunId))
                .ReturnsAsync(new CalculatorRunDetailsViewModel { RunId = model.CalculatorRunDetails.RunId });

            // Act
            var result = await controller.Submit(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;

            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SetRunClassificationViewModel));
            var vm = (SetRunClassificationViewModel)viewResult.Model;

            Assert.IsNotNull(vm);
            Assert.AreEqual(model.CalculatorRunDetails.RunId, vm.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public async Task Submit_WhenDeleteSelected_RedirectsToDeleteController()
        {
            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = (int)RunClassification.DELETED,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 }
            };

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("CalculationRunDelete", redirectResult.ControllerName);
            Assert.AreEqual(model.CalculatorRunDetails.RunId, redirectResult.RouteValues["runId"]);
        }

        [TestMethod]
        public async Task Submit_WhenUnexpectedClassifyRunType_RedirectsToError()
        {
            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = 999,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 }
            };

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }

        [TestMethod]
        public async Task Submit_WhenExceptionThrown_RedirectsToError()
        {
            var model = new SetRunClassificationViewModel
            {
                ClassifyRunType = (int)RunClassification.TEST_RUN,
                CalculatorRunDetails = new CalculatorRunDetailsViewModel { RunId = 123 }
            };

            apiServiceMock
                .Setup(s => s.GetApiUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Test Exception"));

            var result = await controller.Submit(model);

            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));
            var redirectResult = (RedirectToActionResult)result;
            Assert.IsNotNull(redirectResult);
            Assert.AreEqual("Index", redirectResult.ActionName);
            Assert.AreEqual("StandardError", redirectResult.ControllerName);
        }
    }
}
