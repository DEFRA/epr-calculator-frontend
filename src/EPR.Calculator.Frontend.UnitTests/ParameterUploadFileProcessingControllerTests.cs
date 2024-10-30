using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Common;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class ParameterUploadFileProcessingControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };

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

            // Create controller with the mocked factory
            var controller = new ParameterUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object);
            FileNameTest.AssignFileName(controller, "SchemeParameters.csv");

            // Act
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as OkObjectResult;

            Assert.AreEqual("SchemeParameters.csv", controller.FileName);

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
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var controller = new ParameterUploadFileProcessingController(GetConfigurationValues(), mockHttpClientFactory.Object);
            FileNameTest.AssignFileName(controller, "SchemeParameters.csv");
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as BadRequestObjectResult;
            FileNameTest.AssignFileName(controller, "SchemeParameters.csv");
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
            var controller = new ParameterUploadFileProcessingController(config, mockHttpClientFactory.Object);
            var result = controller.Index(MockData.GetSchemeParameterTemplateValues().ToList()) as RedirectToActionResult;
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