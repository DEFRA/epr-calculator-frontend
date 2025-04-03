using System.Net;
using System.Text;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
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
            this.MockSesion = TestMockUtils.BuildMockSession(this.Fixture);
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            this.MockHttpContext.Setup(c => c.Session).Returns(this.MockSesion.Object);

            this.Configuration = TestMockUtils.BuildConfiguration();
            this.Configuration
                .GetSection("ParameterSettings")["ParameterYear"] = this.Fixture.Create<string>();

            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler();
            Mock<IHttpClientFactory> mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(
                this.MockMessageHandler.Object);
            this.TestClass = new ParameterUploadFileProcessingController(
                this.Configuration,
                mockHttpClientFactory.Object,
                new Mock<ITokenAcquisition>().Object,
                new TelemetryClient())
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>()),
            };
            this.TestClass.ControllerContext = new ControllerContext { HttpContext = this.MockHttpContext.Object };
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        private Mock<HttpMessageHandler> MockMessageHandler { get; init; }

        private Mock<ISession> MockSesion { get; init; }

        private IConfiguration Configuration { get; init; }

        private ParameterUploadFileProcessingController TestClass { get; init; }

        [TestMethod]
        public void ParameterUploadFileProcessingController_Success_Result_Test()
        {
            // Mock HttpMessageHandler
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Created,
                        Content = new StringContent("response content"),
                    });

            // Create HttpClient with the mocked handler
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            // Create controller with the mocked factory
            var controller = new ParameterUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient())
            {
                TempData = tempData
            };
            controller.ControllerContext = new ControllerContext { HttpContext = this.MockHttpContext.Object };

            var fileUploadFileName = "SchemeParameters.csv";

            var sessionMock = new Mock<ISession>();
            var sessionStorage = new Dictionary<string, byte[]>();

            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                       .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                       .Returns((string key, out byte[] value) =>
                       {
                           var success = sessionStorage.TryGetValue(key, out var storedValue);
                           value = storedValue;
                           return success;
                       });

            sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
                       .Callback<string>((key) => sessionStorage.Remove(key));

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.Session).Returns(sessionMock.Object);
            controller.ControllerContext.HttpContext = httpContextMock.Object;

            // Act
            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValue = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(viewModel);
            task.Wait();

            var result = task.Result as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public void ParameterUploadFileProcessingController_Failure_Result_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("response content"),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            var controller = new ParameterUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient())
            {
                TempData = tempData
            };
            controller.ControllerContext = new ControllerContext { HttpContext = this.MockHttpContext.Object };

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValue = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(viewModel);
            task.Wait();
            var result = task.Result as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreNotEqual(201, result.StatusCode);
        }

        [TestMethod]
        public void ParameterUploadFileProcessingController_ArgumentNullExceptionForAPIConfig_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("response content"),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = GetConfigurationValues();
            config.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value = string.Empty;
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new ParameterUploadFileProcessingController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValue = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(viewModel);
            task.Wait();
            var result = task.Result as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void ParameterUploadFileProcessingController_ArgumentNullExceptionForYearConfig_Test()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()).ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("response content"),
                    });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var config = TestMockUtils.BuildConfiguration();
            config.GetSection("ParameterSettings").GetSection("ParameterYear").Value = string.Empty;
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new ParameterUploadFileProcessingController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());

            var viewModel = new ParameterRefreshViewModel()
            {
                ParameterTemplateValue = MockData.GetSchemeParameterTemplateValues().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(viewModel);
            task.Wait();
            var result = task.Result as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Index_SendDateFromConfigWhenFeatureFlagDisabled(bool featureFlagEnabled)
        {
            // Arrange
            var configValue = "This value comes from the config.";
            this.Configuration
                .GetSection("ParameterSettings")["ParameterYear"] = configValue;
            this.Configuration
                .GetSection("FeatureManagement")["ShowFinancialYear"] = featureFlagEnabled.ToString();
            this.MockSesion.Object.Set(
                SessionConstants.FinancialYear,
                Encoding.UTF8.GetBytes("This value comes from the session."));
            var expectedTimesCalled = featureFlagEnabled ? Times.Never() : Times.Once();

            // Act
            var result = await TestClass.Index(new ParameterRefreshViewModel());

            // Assert
            this.MockMessageHandler.Protected().Verify(
                "SendAsync",
                expectedTimesCalled,
                ItExpr.Is<HttpRequestMessage>(m =>
                    m.Content.ReadAsStringAsync().Result.Contains($"\"ParameterYear\":\"{configValue}\"")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Index_SendDateFromSessionWhenFeatureFlagEnabled(bool featureFlagEnabled)
        {
            // Arrange
            var sessionMessage = "This value comes from the session.";
            this.MockSesion.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
            .Returns((string key, out byte[] value) =>
            {
                value = Encoding.UTF8.GetBytes(sessionMessage);
                return true;
            });

            this.Configuration
                .GetSection("FeatureManagement")["ShowFinancialYear"] = featureFlagEnabled.ToString();
            var expectedTimesCalled = featureFlagEnabled ? Times.Once() : Times.Never();

            // Act
            var result = await TestClass.Index(new ParameterRefreshViewModel());

            // Assert
            this.MockMessageHandler.Protected().Verify(
                "SendAsync",
                expectedTimesCalled,
                ItExpr.Is<HttpRequestMessage>(m =>
                    m.Content.ReadAsStringAsync().Result.Contains($"\"ParameterYear\":\"{sessionMessage}\"")),
                ItExpr.IsAny<CancellationToken>());
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
    }
}