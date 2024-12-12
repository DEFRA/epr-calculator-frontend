using System.Net;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.HelpersTest;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class DefaultParameterControllerTests
    {
        private static readonly string[] Separator = new string[] { @"bin\" };
        private static readonly int TotalRecords = 11;

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

            var controller = new DefaultParametersController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);

            var mockContext = new Mock<HttpContext>();
            mockContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            controller.ControllerContext.HttpContext = mockContext.Object;

            var result = await controller.Index() as ViewResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(TotalRecords, result.ViewData.Count);
            Assert.IsNotNull(result.ViewData["CommunicationData"]);
            Assert.IsNotNull(result.ViewData["OperatingCosts"]);
            Assert.IsNotNull(result.ViewData["PreparationCosts"]);
            Assert.IsNotNull(result.ViewData["SchemeSetupCosts"]);
            Assert.IsNotNull(result.ViewData["LateReportingTonnage"]);
            Assert.IsNotNull(result.ViewData["MaterialityThreshold"]);
            Assert.IsNotNull(result.ViewData["BadDebtProvision"]);
            Assert.IsNotNull(result.ViewData["CommunicationCostsByCountry"]);
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
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var controller = new DefaultParametersController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

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
            var controller = new DefaultParametersController(ConfigurationItems.GetConfigurationValues(), mockHttpClientFactory.Object);

            var result = await controller.Index() as RedirectToActionResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.StandardErrorIndex, result.ActionName);
            Assert.AreEqual("StandardError", result.ControllerName);
        }
    }
}
