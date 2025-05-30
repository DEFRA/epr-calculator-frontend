﻿using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using AutoFixture;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class LocalAuthorityUploadFileProcessingControllerTests
    {
        public LocalAuthorityUploadFileProcessingControllerTests()
        {
            this.Fixture = new Fixture();
            this.MockSesion = TestMockUtils.BuildMockSession(this.Fixture);
            this.MockHttpContext = new Mock<HttpContext>();
            this.MockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            this.MockHttpContext.Setup(c => c.Session).Returns(this.MockSesion.Object);

            this.Configuration = TestMockUtils.BuildConfiguration();
            this.Configuration
                .GetSection("ParameterSettings")["ParameterYear"] = this.Fixture.Create<string>();

            this.MockMessageHandler = TestMockUtils.BuildMockMessageHandler(HttpStatusCode.Accepted, new StringContent("response content"));
            Mock<IHttpClientFactory> mockHttpClientFactory = TestMockUtils.BuildMockHttpClientFactory(
                this.MockMessageHandler.Object);
            this.TestClass = new LocalAuthorityUploadFileProcessingController(
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

        private LocalAuthorityUploadFileProcessingController TestClass { get; init; }

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

            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");

            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            // Create controller with the mocked factory
            var controller = new LocalAuthorityUploadFileProcessingController(TestMockUtils.BuildConfiguration(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient())
            {
                TempData = tempData
            };
            controller.ControllerContext = new ControllerContext { HttpContext = context };

            var fileUploadFileName = "LocalAuthorityData.csv";

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
            controller.ControllerContext.HttpContext = context;

            // Act
            var viewModel = new LapcapRefreshViewModel()
            {
                LapcapTemplateValue = MockData.GetLocalAuthorityDisposalCostsToUpload().ToList(),
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

            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                    .Returns(httpClient);
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            mockTokenAcquisition
                .Setup(x => x.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, null, null))
                .ReturnsAsync("somevalue");
            var mockSession = new MockHttpSession();
            mockSession.SetString("accessToken", "something");
            var context = new DefaultHttpContext()
            {
                Session = mockSession
            };
            mockSession.SetString(SessionConstants.FinancialYear, "2024-25");
            var controller = new LocalAuthorityUploadFileProcessingController(TestMockUtils.BuildConfiguration(), mockHttpClientFactory.Object, mockTokenAcquisition.Object, new TelemetryClient())
            {
                TempData = tempData,
            };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(s => s.Session).Returns(mockSession);
            mockHttpContext.Setup(c => c.User.Identity.Name).Returns(Fixture.Create<string>);
            controller.ControllerContext.HttpContext = mockHttpContext.Object;

            var viewModel = new LapcapRefreshViewModel()
            {
                LapcapTemplateValue = MockData.GetLocalAuthorityDisposalCostsToUpload().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(viewModel);
            task.Wait();
            var result = task.Result as BadRequestObjectResult;
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
            var config = TestMockUtils.BuildConfiguration();
            config.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value = string.Empty;
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new LocalAuthorityUploadFileProcessingController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());
            var viewModel = new LapcapRefreshViewModel()
            {
                LapcapTemplateValue = MockData.GetLocalAuthorityDisposalCostsToUpload().ToList(),
                FileName = "Test Name",
            };

            var task = controller.Index(viewModel);
            var result = task.Result as RedirectToActionResult;
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
            var config = TestMockUtils.BuildConfiguration();
            config.GetSection("LapcapSettings").GetSection("ParameterYear").Value = string.Empty;
            var mockTokenAcquisition = new Mock<ITokenAcquisition>();
            var controller = new LocalAuthorityUploadFileProcessingController(config, mockHttpClientFactory.Object,
                mockTokenAcquisition.Object, new TelemetryClient());

            var viewModel = new LapcapRefreshViewModel()
            {
                LapcapTemplateValue = MockData.GetLocalAuthorityDisposalCostsToUpload().ToList(),
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
        public async Task Index_SendDateFromSession()
        {
            // Arrange
            var sessionMessage = "This value comes from the session.";
            this.MockSesion.Object.Set(
                SessionConstants.FinancialYear,
                Encoding.UTF8.GetBytes(sessionMessage));

            // Act
            var result = await TestClass.Index(new LapcapRefreshViewModel());

            // Assert
            this.MockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(m =>
                    m.Content.ReadAsStringAsync().Result.Contains($"\"ParameterYear\":\"{sessionMessage}\"")),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task Index_DefaultToCurrentYearWhenNoFinancialYearInSession()
        {
            var currentYear = CommonUtil.GetFinancialYear(DateTime.Now);

            // Act
            var result = await TestClass.Index(new LapcapRefreshViewModel());

            // Assert
            Assert.IsFalse(this.MockSesion.Object.Keys.Contains(SessionConstants.FinancialYear));
            this.MockMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(m =>
                    m.Content.ReadAsStringAsync().Result.Contains($"\"ParameterYear\":\"{currentYear}\"")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}