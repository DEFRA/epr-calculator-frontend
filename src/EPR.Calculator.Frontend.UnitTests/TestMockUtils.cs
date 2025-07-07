using System.Net;
using System.Text;
using AutoFixture;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.UnitTests
{
    /// <summary>
    /// Static methods to help build commonly used test mocks.
    /// </summary>
    internal class TestMockUtils
    {
        public static IConfiguration BuildConfiguration()
        {
            string projectPath = AppDomain.CurrentDomain.BaseDirectory.Split(@"bin\", StringSplitOptions.None)[0];
            IConfiguration config = new ConfigurationBuilder()
               .SetBasePath(projectPath)
               .AddJsonFile("appsettings.Test.json")
               .Build();

            return config;
        }

        public static Mock<IHttpClientFactory> BuildMockHttpClientFactory(HttpStatusCode statusCode)
            => BuildMockHttpClientFactory(BuildMockMessageHandler(statusCode).Object);

        public static Mock<IHttpClientFactory> BuildMockHttpClientFactory(HttpMessageHandler httpMessageHandler)
        {
            // Create HttpClient with the mocked handler
            var httpClient = new HttpClient(httpMessageHandler);

            // Mock IHttpClientFactory
            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory
                .Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);
            return mockHttpClientFactory;
        }

        public static Mock<HttpMessageHandler> BuildMockMessageHandler(
            HttpStatusCode? statusCode = null,
            object content = null,
            bool shouldThrowException = false,
            Exception exceptionToThrow = null)
        {
            // Mock HttpMessageHandler
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            if (shouldThrowException)
            {
                mockHttpMessageHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ThrowsAsync(exceptionToThrow ?? new HttpRequestException("API failure"));
            }
            else
            {
                var responseMessage = new HttpResponseMessage
                {
                    StatusCode = statusCode ?? HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(content ?? new object()))
                };

                mockHttpMessageHandler
                    .Protected()
                    .Setup<Task<HttpResponseMessage>>(
                        "SendAsync",
                        ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(responseMessage);
            }

            return mockHttpMessageHandler;
        }

        public static Mock<ISession> BuildMockSession(Fixture fixture)
        {
            var sessionMock = new Mock<ISession>();
            var sessionStorage = new Dictionary<string, byte[]>
            {
                { "accessToken", Encoding.UTF8.GetBytes(fixture.Create<string>()) },
            };

            sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                       .Callback<string, byte[]>((key, value) => sessionStorage[key] = value);

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns((string key, out byte[] value) =>
                {
                    var success = sessionStorage.TryGetValue(key, out var storedValue);
                    value = storedValue;
                    return success;
                });

            sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
               .Returns((string key, out byte[]? value) =>
               {
                   var success = sessionStorage.TryGetValue(key, out var storedValue);
                   value = storedValue;
                   return success;
               });

            sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
           .Callback<string>((key) => sessionStorage.Remove(key));

            return sessionMock;
        }

        /// <summary>
        /// Creates a mock <see cref="IApiService"/> and configures it's methods to return values.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code to return in the response.</param>
        /// <returns>A mock <see cref="IApiService"/>.</returns>
        public static Mock<IApiService> BuildMockApiService(HttpStatusCode httpStatusCode)
            => BuildMockApiService(httpStatusCode, "{}");

        /// <summary>
        /// Creates a mock <see cref="IApiService"/> and configures it's methods to return values.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code to return in the response.</param>
        /// <param name="jsonContent">The JSON content to return in the response.</param>
        /// <returns>A mock <see cref="IApiService"/>.</returns>
        public static Mock<IApiService> BuildMockApiService(HttpStatusCode httpStatusCode, string jsonContent)
        {
            var service = new Mock<IApiService>();
            service.Setup(s => s.GetApiUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Uri("http://test.test"));
            service.Setup(s => s.CallApi(
                It.IsAny<HttpContext>(),
                It.IsAny<HttpMethod>(),
                It.IsAny<Uri>(),
                It.IsAny<string>(),
                It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode,
                    Content = new StringContent(jsonContent)
                });
            return service;
        }

        public static Mock<ICalculatorRunDetailsService> BuildMockCalculatorRunDetailsService(
            CalculatorRunDetailsViewModel data)
        {
            var service = new Mock<ICalculatorRunDetailsService>();
            service.Setup(s => s.GetCalculatorRundetailsAsync(
                It.IsAny<HttpContext>(),
                It.IsAny<int>()))
                .ReturnsAsync(data);

            return service;
        }
    }
}
