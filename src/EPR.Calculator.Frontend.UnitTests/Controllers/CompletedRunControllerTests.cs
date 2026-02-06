namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    using System.Net;
    using AutoFixture;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Controllers;
    using EPR.Calculator.Frontend.Helpers;
    using EPR.Calculator.Frontend.UnitTests.HelpersTest;
    using EPR.Calculator.Frontend.UnitTests.Mocks;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.ApplicationInsights;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CompletedRunControllerTests
    {
        private Fixture Fixture { get; } = new Fixture();

        [TestMethod]
        public async Task Index_ValidRunId_ReturnsViewResult()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                MockData.GetCalculatorRun(),
                Fixture.Create<CalculatorRunDetailsViewModel>());

            // Act
            var result = await controller.Index(10) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ViewNames.PostBillingFileIndex, result.ViewName);
        }

        [TestMethod]
        public async Task Index_InvalidModelState_ReturnsRedirectToAction()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.NotFound,
                MockData.GetCalculatorRun(),
                Fixture.Create<CalculatorRunDetailsViewModel>());

            // Arrange
            var runId = 10;

            // Act
            var result = await controller.Index(runId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual(CommonUtil.GetControllerName(typeof(StandardErrorController)), result.ControllerName);
        }

        private CompletedRunController BuildTestClass(
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

            var testClass = new CompletedRunController(
                configurationItems,
                new TelemetryClient(),
                mockApiService,
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return testClass;
        }
    }
}