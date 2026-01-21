using System.Net;
using System.Text;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class ParameterUploadFileProcessingControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };

        public ParameterUploadFileProcessingControllerTests()
        {
            this.Fixture = new Fixture();

            (this.TestClass, this.MockApiService) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Accepted,
                MockData.GetCalculatorRun(),
                null,
                TestMockUtils.BuildConfiguration());
        }

        private Fixture Fixture { get; init; }

        private ParameterUploadFileProcessingController TestClass { get; init; }

        private Mock<IApiService> MockApiService { get; init; }

        [TestMethod]
        public async Task ParameterUploadFileProcessingController_Success_Result_Test()
        {
            // Arrange
            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValues = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            (var testClass, var mockApiService) = BuildTestClass(
                this.Fixture,
                HttpStatusCode.Created,
                MockData.GetCalculatorRun(),
                null,
                TestMockUtils.BuildConfiguration());

            // Act
            var result = (OkObjectResult)(await testClass.Index(viewModel));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task ParameterUploadFileProcessingController_Failure_Result_Test()
        {
            // Arrange
            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValues = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            // Act
            var result = (BadRequestObjectResult)(await this.TestClass.Index(viewModel));

            // Assert
            Assert.IsNotNull(result);
            Assert.AreNotEqual(201, result.StatusCode);
        }

        [TestMethod]
        public async Task Index_SendDateFromSession()
        {
            // Arrange
            var data = Fixture.Create<ParameterRefreshViewModel>();

            // Act
            var result = await TestClass.Index(data);

            // Assert
            this.MockApiService.Verify(
               x => x.CallApi(
                   TestClass.HttpContext,
                   HttpMethod.Post,
                   It.IsAny<Uri>(),
                   It.IsAny<string>(),
                   It.Is<CreateDefaultParameterSettingDto>(dto =>
                       dto.ParameterTemplateValues.SequenceEqual(data.ParameterTemplateValues) &&
                       dto.FileName == data.FileName &&
                       dto.ParameterYear == CommonUtil.GetDefaultFinancialYear(DateTime.UtcNow, 4))),
               Times.Once);
        }

        private static IConfiguration GetConfigurationValues()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(Separator, StringSplitOptions.None)[0];
            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("appsettings.Test.json")
               .Build();

            return config;
        }

        private (ParameterUploadFileProcessingController Controller, Mock<IApiService> MockApiService) BuildTestClass(
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
                System.Text.Json.JsonSerializer.Serialize(data ?? MockData.GetCalculatorRun()));

            var testClass = new ParameterUploadFileProcessingController(
                configurationItems,
                mockApiService.Object,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient(),
                TestMockUtils.BuildMockCalculatorRunDetailsService(details).Object);
            testClass.ControllerContext.HttpContext = new DefaultHttpContext()
            {
                Session = TestMockUtils.BuildMockSession(fixture).Object,
            };

            return (testClass, mockApiService);
        }
    }
}