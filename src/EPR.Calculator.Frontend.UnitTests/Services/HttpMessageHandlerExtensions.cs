using Moq;
using Moq.Protected;

namespace EPR.Calculator.Frontend.UnitTests.Services
{
    // Extension methods for mocking
    public static class HttpMessageHandlerExtensions
    {
        public static void SetupSendAsync(this Mock<HttpMessageHandler> mock, HttpMethod method, string url, HttpResponseMessage response)
        {
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == method && m.RequestUri.ToString() == url),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        public static void SetupSendAsyncThrows(this Mock<HttpMessageHandler> mock, HttpMethod method, string url, Exception ex)
        {
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == method && m.RequestUri.ToString() == url),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(ex);
        }
    }
}
