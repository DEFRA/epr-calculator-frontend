using EPR.Calculator.Frontend.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

[TestClass]
public class GlobalExceptionFilterTests
{
    private Mock<ILogger<GlobalExceptionFilter>> _mockLogger;
    private GlobalExceptionFilter _filter;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<GlobalExceptionFilter>>();
        _filter = new GlobalExceptionFilter(_mockLogger.Object);
    }

    [TestMethod]
    public async Task OnExceptionAsync_IdentityWebException_ShouldTriggerChallenge()
    {
        // Arrange
        var msalEx = new Microsoft.Identity.Client.MsalUiRequiredException("user_null", "User not found");
        var exception = new MicrosoftIdentityWebChallengeUserException(msalEx, new[] { "scope1" }, null);

        var authServiceMock = new Mock<IAuthenticationService>();
        authServiceMock
            .Setup(s => s.ChallengeAsync(It.IsAny<HttpContext>(), OpenIdConnectDefaults.AuthenticationScheme, It.IsAny<AuthenticationProperties>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var serviceProvider = new ServiceCollection()
            .AddSingleton(authServiceMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        var context = new ExceptionContext(actionContext, new System.Collections.Generic.List<IFilterMetadata>())
        {
            Exception = exception
        };

        // Act
        await _filter.OnExceptionAsync(context);

        // Assert
        Assert.IsTrue(context.ExceptionHandled);
        authServiceMock.Verify();
        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public async Task OnExceptionAsync_GenericException_ShouldLogAndRedirect()
    {
        // Arrange
        var exception = new Exception("Something went wrong");

        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        var context = new ExceptionContext(actionContext, new System.Collections.Generic.List<IFilterMetadata>())
        {
            Exception = exception
        };

        // Act
        await _filter.OnExceptionAsync(context);

        // Assert
        Assert.IsTrue(context.ExceptionHandled);
        Assert.IsInstanceOfType(context.Result, typeof(RedirectToActionResult));

        var redirectResult = (RedirectToActionResult)context.Result;
        Assert.AreEqual("Index", redirectResult.ActionName);
        Assert.AreEqual("StandardError", redirectResult.ControllerName);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected error")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
