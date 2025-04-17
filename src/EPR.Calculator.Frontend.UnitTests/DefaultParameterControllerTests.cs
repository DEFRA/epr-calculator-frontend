﻿using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Extensions;
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
                    Content = new StringContent(JsonConvert.SerializeObject(MockData.GetDefaultParameterValues()))
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();

            var controller = new DefaultParametersController(
                ConfigurationItems.GetConfigurationValues(),
                mockHttpClientFactory.Object,
                mockTokenAcquisition.Object,
                new TelemetryClient());

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            mockContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);
            controller.ControllerContext.HttpContext = mockContext.Object;

            var result = await controller.Index() as ViewResult;

            var resultModel = result.ViewData.Model as DefaultParametersViewModel;

            Assert.IsNotNull(result);
            Assert.AreEqual(TotalRecords, resultModel.SchemeParameters.Count());
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
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            mockHttpContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();

            var controller = new DefaultParametersController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient());

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var result = await controller.Index() as ViewResult;
            var defaultParametersViewModel = result.Model as DefaultParametersViewModel;

            Assert.IsNotNull(result);
            Assert.AreEqual(false, defaultParametersViewModel.IsDataAvailable);
        }

        [TestMethod]
        public async Task DefaultParameterController_Failure_View_Test()
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
                    Content = new StringContent("Sample content")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new DefaultParametersController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient());

            var result = await controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public async Task DefaultParameterController_ParameterYearMissing_RedirectsToDashboard()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();

            var httpClient = new HttpClient();
            mockHttpClientFactory
                .Setup(f => f.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mockConfigSection = new Mock<IConfigurationSection>();
            mockConfigSection.Setup(x => x.Value).Returns("https://mock.api");

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c.GetSection(ConfigSection.ParameterSettings))
                             .Returns(mockConfigSection.Object);
            mockConfiguration.Setup(c => c.GetSection(ConfigSection.ParameterSettings)
                                          .GetSection(ConfigSection.DefaultParameterSettingsApi))
                             .Returns(mockConfigSection.Object);

            var controller = new TestableDefaultParametersController(
                mockConfiguration.Object,
                mockHttpClientFactory.Object,
                mockTokenAcquisition.Object,
                new TelemetryClient());

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns("TestUser");
            mockContext.Setup(c => c.Session).Returns(TestMockUtils.BuildMockSession(Fixture).Object);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockContext.Object
            };

            // Act
            var result = await controller.Index();

            // Assert
            var redirect = result as RedirectToActionResult;
            Assert.IsNotNull(redirect);
            Assert.AreEqual(ActionNames.Index, redirect.ActionName);
            Assert.AreEqual("Dashboard", redirect.ControllerName);
        }

        private class TestableDefaultParametersController : DefaultParametersController
        {
            public TestableDefaultParametersController(
                IConfiguration configuration,
                IHttpClientFactory clientFactory,
                ITokenAcquisition tokenAcquisition,
                TelemetryClient telemetryClient)
                : base(configuration, clientFactory, tokenAcquisition, telemetryClient)
            {
            }

            protected override string GetFinancialYear(string configSection)
            {
                throw new ArgumentNullException("ParameterYear", "ParameterYear is null. Check the configuration settings.");
            }
        }
    }
}