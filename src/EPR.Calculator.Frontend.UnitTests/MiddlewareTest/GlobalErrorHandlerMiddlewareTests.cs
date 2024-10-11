using System.Text.Json;
using EPR.Calculator.Frontend.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.MiddlewareTest
{
    [TestClass]
    public class GlobalErrorHandlerMiddlewareTests
    {
        [TestMethod]
        public async Task InvokeAsync_ExceptionThrown_ReturnsInternalServerError()
        {
            var loggerMock = new Mock<ILogger<GlobalErrorHandlerMiddleware>>();
            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Test Exception"));

            var middleware = new GlobalErrorHandlerMiddleware(nextMock.Object, loggerMock.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            var session = new Mock<ISession>();
            context.Session = session.Object;

            await middleware.InvokeAsync(context);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, problemDetails.Status);
            Assert.AreEqual("Server Error", problemDetails.Title);
            Assert.AreEqual("Test Exception", problemDetails.Detail);
        }

        [TestMethod]
        public async Task InvokeAsync_ExceptionThrown_And_LogError()
        {
            var loggerMock = new Mock<ILogger<GlobalErrorHandlerMiddleware>>();
            var nextMock = new Mock<RequestDelegate>();
            nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Test Exception"));

            var middleware = new GlobalErrorHandlerMiddleware(nextMock.Object, loggerMock.Object);

            var context = new DefaultHttpContext();
            var session = new Mock<ISession>();
            context.Session = session.Object;

            await middleware.InvokeAsync(context);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}