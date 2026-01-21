using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DefaultParameterControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };
        private static readonly int TotalRecords = 9;

        private Fixture Fixture { get; } = new Fixture();

        [TestMethod]
        public async Task DefaultParamerController_Success_View_Test()
        {
            // Arrange
            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.OK,
                MockData.GetDefaultParameterValues());

            // Act
            var result = await controller.Index() as ViewResult;
            var resultModel = result.ViewData.Model as DefaultParametersViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(TotalRecords, resultModel.SchemeParameters.Count);
            Assert.AreEqual(1, resultModel.SchemeParameters.Count(t => t.SchemeParameterName == ParameterType.CommunicationCostsByCountry.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.CommunicationCostsByMaterial.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.BadDebtProvision.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.LateReportingTonnage.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.LocalAuthorityDataPreparationCosts.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.MaterialityThreshold.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.SchemeAdministratorOperatingCosts.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.SchemeSetupCosts.GetDisplayName()));
            Assert.AreEqual(true, resultModel.SchemeParameters.Any(t => t.SchemeParameterName == ParameterType.TonnageChangeThreshold.GetDisplayName()));
        }

        [TestMethod]
        public async Task DefaultParameterController_Success_No_Data_View_Test()
        {
            // Arrange
            var content = "No data available for the specified year.Please check the year and try again.";

            var controller = BuildTestClass(
                Fixture,
                HttpStatusCode.NotFound,
                content);

            // Act
            var result = await controller.Index() as ViewResult;
            var defaultParametersViewModel = (DefaultParametersViewModel)result.Model as DefaultParametersViewModel;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(false, defaultParametersViewModel.IsDataAvailable);
        }

        private DefaultParametersController BuildTestClass(
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

            var testClass = new DefaultParametersController(
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