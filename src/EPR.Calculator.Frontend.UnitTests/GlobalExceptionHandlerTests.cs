﻿using System.Text.Json;
using EPR.Calculator.Frontend.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests
{
    [TestClass]
    public class GlobalExceptionHandlerTests
    {
        private Mock<ILogger<GlobalExceptionHandler>> _mockLogger;
        private Mock<IHostEnvironment> _mockEnv;
        private GlobalExceptionHandler _exceptionHandler;
        private DefaultHttpContext _httpContext;
        private CancellationToken _cancellationToken;

        [TestInitialize]
        public void SetUp()
        {
            _mockLogger = new Mock<ILogger<GlobalExceptionHandler>>();
            _mockEnv = new Mock<IHostEnvironment>();
            _exceptionHandler = new GlobalExceptionHandler(_mockLogger.Object, _mockEnv.Object);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
            _cancellationToken = CancellationToken.None;
        }

        [TestMethod]
        public async Task TryHandleAsync_LogsException()
        {
            var exception = new Exception("Test exception");

            await _exceptionHandler.TryHandleAsync(_httpContext, exception, _cancellationToken);

            _mockLogger.Verify(
               logger => logger.Log(
                   LogLevel.Error,
                   It.IsAny<EventId>(),
                   It.Is<It.IsAnyType>((state, t) => state.ToString().Contains("An unhandled exception occurred.")),
                   exception,
                   It.IsAny<Func<It.IsAnyType, Exception, string>>()),
               Times.Once);
        }

        [TestMethod]
        public async Task TryHandleAsync_SetsInternalServerErrorAndJsonContentType()
        {
            var exception = new Exception("Test exception");

            await _exceptionHandler.TryHandleAsync(_httpContext, exception, _cancellationToken);

            Assert.AreEqual("application/json", _httpContext.Response.ContentType);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, _httpContext.Response.StatusCode);
        }

        [TestMethod]
        [DataRow("Development")]
        [DataRow("local")]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_InDevelopment_Or_InLocal(string environmentName)
        {
            _mockEnv.Setup(env => env.EnvironmentName).Returns(environmentName);
            var exception = new Exception("Test exception");

            await _exceptionHandler.TryHandleAsync(_httpContext, exception, _cancellationToken);

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
        }

        [TestMethod]
        [DataRow("Development")]
        [DataRow("local")]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_DummyStackTrace_InDevelopment_Or_InLocal(string environmentName)
        {
            _mockEnv.Setup(env => env.EnvironmentName).Returns(environmentName);
            var dummyStackTrace = "at DummyNamespace.DummyClass.DummyMethod() in /DummyFile.cs:line 42";
            var exception = GlobalExceptionHandlerTestsHelpers.CreateExceptionWithDummyStackTrace("Test exception with dummy stack trace", dummyStackTrace);

            await _exceptionHandler.TryHandleAsync(_httpContext, exception, _cancellationToken);

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
            Assert.AreEqual(exception.StackTrace, responseObject.GetProperty("Detail").GetString());
        }

        [TestMethod]
        public async Task TryHandleAsync_ReturnsExpectedJsonResponse_NotInDevelopment()
        {
            _mockEnv.Setup(env => env.EnvironmentName).Returns(Environments.Production);
            var exception = new Exception("Test exception");

            await _exceptionHandler.TryHandleAsync(_httpContext, exception, _cancellationToken);

            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var jsonResponse = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
            var responseObject = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            Assert.AreEqual(StatusCodes.Status500InternalServerError, responseObject.GetProperty("Status").GetInt32());
            Assert.AreEqual("An error occurred while processing your request.", responseObject.GetProperty("Title").GetString());
            Assert.AreEqual(exception.Message, responseObject.GetProperty("Message").GetString());
            Assert.IsFalse(responseObject.TryGetProperty("Detail", out _));
        }
    }
}