using System.Net;
using Castle.Core.Logging;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityUploadFileProcessingControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_Success_Result_Test()
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
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            // Create controller with the mocked factory
            var controller = new LocalAuthorityUploadFileProcessingController(
                GetConfigurationValues(),
                mockHttpClientFactory.Object,
                new Mock<Microsoft.Extensions.Logging.ILogger<LocalAuthorityUploadFileProcessingController>>().Object)
            {
                TempData = tempData
            };

            // Act
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as OkObjectResult;

            // Assert
            Assert.AreEqual("LocalAuthorityData.csv", controller.FileName);
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_Failure_Result_Test()
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
            tempData["LapcapFileName"] = "LocalAuthorityData.csv";

            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);

            var controller = new LocalAuthorityUploadFileProcessingController(
                GetConfigurationValues(),
                mockHttpClientFactory.Object,
                new Mock<Microsoft.Extensions.Logging.ILogger<LocalAuthorityUploadFileProcessingController>>().Object)
            {
                TempData = tempData
            };
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreNotEqual(201, result.StatusCode);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_ArgumentNullExceptionForAPIConfig_Test()
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
            config.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value = string.Empty;
            var controller = new LocalAuthorityUploadFileProcessingController(
                config,
                mockHttpClientFactory.Object,
                new Mock<Microsoft.Extensions.Logging.ILogger<LocalAuthorityUploadFileProcessingController>>().Object);
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }

        [TestMethod]
        public void LocalAuthorityUploadFileProcessingController_ArgumentNullExceptionForYearConfig_Test()
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
            config.GetSection("LapcapSettings").GetSection("ParameterYear").Value = string.Empty;
            var controller = new LocalAuthorityUploadFileProcessingController(
                config,
                mockHttpClientFactory.Object,
                new Mock<Microsoft.Extensions.Logging.ILogger<LocalAuthorityUploadFileProcessingController>>().Object);
            var result = controller.Index(MockData.GetLocalAuthorityDisposalCostsToUpload().ToList()) as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
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