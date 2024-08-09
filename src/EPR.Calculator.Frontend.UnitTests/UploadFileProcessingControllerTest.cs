using System.Net;
using System.Net.Http;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class UploadFileProcessingControllerTest
    {
        [TestMethod]
        public void UploadFileProcessingController_Success_Result_Test()
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

            // Create controller with the mocked factory
            var controller = new UploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object);

            // Act
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, 200);
        }

        [TestMethod]
        public void UploadFileProcessingController_Failure_Result_Test()
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
            var controller = new UploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object);
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreNotEqual(result.StatusCode, 201);
        }

        private IConfiguration GetConfigurationValues()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(new string[] { @"bin\" }, StringSplitOptions.None)[0];
            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("appsettings.Test.json")
               .Build();

            return config;
        }
    }
}
