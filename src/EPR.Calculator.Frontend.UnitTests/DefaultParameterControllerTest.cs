using System.Net;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DefaultParameterControllerTest
    {
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

            var controller = new DefaultParametersController(GetConfigurationValues(), mockHttpClientFactory.Object);

            var result = await controller.Index() as ViewResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(result.ViewData.Count, 10);
            Assert.IsNotNull(result.ViewData["CommunicationData"]);
            Assert.IsNotNull(result.ViewData["OperatingCosts"]);
            Assert.IsNotNull(result.ViewData["PreparationCosts"]);
            Assert.IsNotNull(result.ViewData["SchemeSetupCosts"]);
            Assert.IsNotNull(result.ViewData["LateReportingTonnage"]);
            Assert.IsNotNull(result.ViewData["MaterialityThreshold"]);
            Assert.IsNotNull(result.ViewData["BadDebtProvision"]);
            Assert.IsNotNull(result.ViewData["Levy"]);
            Assert.IsNotNull(result.ViewData["TonnageChange"]);

            Assert.AreEqual(true, result.ViewData["IsDataAvailable"]);
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

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new DefaultParametersController(GetConfigurationValues(), mockHttpClientFactory.Object);

            var result = await controller.Index() as ViewResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(false, result.ViewData["IsDataAvailable"]);
        }

        [TestMethod]
        public async Task DefaultParamerController_Failure_View_Test()
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
            var controller = new DefaultParametersController(GetConfigurationValues(), mockHttpClientFactory.Object);

            var result = await controller.Index() as BadRequestObjectResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
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
