using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
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
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
        }

        private Fixture Fixture { get; init; }

        private Mock<HttpContext> MockHttpContext { get; init; }

        [TestMethod]
        public async Task ParameterUploadFileProcessingController_Success_Result_Test()
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
            controller.HttpContext.Session.SetString(SessionConstants.ParameterFileName, fileUploadFileName);

            // Act
            var result = await controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
            Assert.AreEqual("SchemeParameters.csv", controller.FileName);
            Assert.IsTrue(sessionStorage.ContainsKey(SessionConstants.ParameterFileName));
        }

        [TestMethod]
        public async Task ParameterUploadFileProcessingController_Failure_Result_Test()
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

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Set the session value to null to trigger the exception
            mockSession.Setup(s => s.TryGetValue(SessionConstants.ParameterFileName, out It.Ref<byte[]>.IsAny)).Returns(false);

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()));
            Assert.AreEqual("FileName is null. Check the session data for ParameterFileName", ex.Message);
        }

        [TestMethod]
        public async Task ParameterUploadFileProcessingController_ArgumentExceptionForAPIConfig_Test()
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

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()));
            Assert.AreEqual("ParameterSettingsApi is null. Check the configuration settings for default parameters", ex.Message);
        }

        [TestMethod]
        public async Task Index_ThrowsArgumentException_WhenParameterSettingsApiIsNullOrEmpty()
        {
            // Arrange
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var mockConfig = new Mock<IConfiguration>();
            var mockParameterSettingsSection = new Mock<IConfigurationSection>();
            mockParameterSettingsSection.Setup(s => s.Value).Returns(string.Empty);
            mockConfig.Setup(c => c.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi")).Returns(mockParameterSettingsSection.Object);

            var controller = new ParameterUploadFileProcessingController(mockConfig.Object, mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient());

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(() => controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()));
            Assert.AreEqual("ParameterSettingsApi is null. Check the configuration settings for default parameters", ex.Message);
        }

        [TestMethod]
        public async Task Index_ReturnsBadRequest_WhenResponseIsNotCreated()
        {
            // Arrange
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
                    Content = new StringContent("response content"),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition.Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null)).ReturnsAsync("somevalue");

            var controller = new ParameterUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient());

            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession.Object);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            // Set the session value
            var sessionStorage = new Dictionary<string, byte[]>();
            sessionStorage[SessionConstants.ParameterFileName] = System.Text.Encoding.UTF8.GetBytes("SchemeParameters.csv");
            mockSession.Setup(s => s.TryGetValue(SessionConstants.ParameterFileName, out It.Ref<byte[]>.IsAny))
                       .Returns((string key, out byte[] value) =>
                       {
                           var success = sessionStorage.TryGetValue(key, out var storedValue);
                           value = storedValue;
                           return success;
                       });

            // Act
            var result = await controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("response content", result.Value);
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