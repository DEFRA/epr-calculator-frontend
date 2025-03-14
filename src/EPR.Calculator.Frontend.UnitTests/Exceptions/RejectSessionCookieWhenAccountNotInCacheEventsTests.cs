namespace EPR.Calculator.Frontend.UnitTests.Exceptions
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Exceptions;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Identity.Client;
    using Microsoft.Identity.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class RejectSessionCookieWhenAccountNotInCacheEventsTests
    {
        private RejectSessionCookieWhenAccountNotInCacheEvents _testClass;
        private string[] _downstreamScopes;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture();
            _downstreamScopes = fixture.Create<string[]>();
            _testClass = new RejectSessionCookieWhenAccountNotInCacheEvents(_downstreamScopes);
        }

        [TestMethod]
        public async Task CanCallValidatePrincipal()
        {
            // Arrange
            var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());
            var username = fixture.Create<string>();
            var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim("access_token", "valid_token") };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var mockTokenAcquistion = new Mock<ITokenAcquisition>();
            mockTokenAcquistion.Setup(m => m.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, principal, null)).ReturnsAsync("valid_Token");

            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);

            var httpContext = new DefaultHttpContext();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(m => m.GetService(typeof(ITokenAcquisition))).Returns(mockTokenAcquistion.Object);

            httpContext.RequestServices = mockServiceProvider.Object;

            var o = new CookieAuthenticationOptions();

            var authScheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));

            var mockContext = new CookieValidatePrincipalContext(httpContext, authScheme, o, authTicket);

            // Act
            await _testClass.ValidatePrincipal(mockContext);

            // Assert
            Assert.AreEqual(username, mockContext.Principal.Identity.Name);
        }

        [TestMethod]
        public async Task ValidatePrincipal_Should_Reject_When_TokenAcquistion_Fails()
        {
            // Arrange
            var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());
            var username = fixture.Create<string>();
            var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim("access_token", "expired_token") };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var mockTokenAcquistion = new Mock<ITokenAcquisition>();
            mockTokenAcquistion.Setup(m => m.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, default, null)).
               ReturnsAsync(string.Empty);

            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);

            var httpContext = new DefaultHttpContext();

            var authProperties = fixture.Create<AuthenticationProperties>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(m => m.GetService(typeof(ITokenAcquisition))).Returns(mockTokenAcquistion.Object);

            httpContext.RequestServices = mockServiceProvider.Object;

            var o = new CookieAuthenticationOptions();

            var authScheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));

            var mockContext = new CookieValidatePrincipalContext(httpContext, authScheme, o, authTicket);

            // Act
            await _testClass.ValidatePrincipal(mockContext);

            // Assert
            Assert.IsNull(mockContext.Principal);
        }

        public async Task CannotCallAccountDoesNotExitInTokenCache()
        {
            // Arrange
            var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());
            var username = fixture.Create<string>();
            var claims = new[] { new Claim(ClaimTypes.Name, username), new Claim("access_token", "expired_token") };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal();

            var mockTokenAcquistion = new Mock<ITokenAcquisition>();
            mockTokenAcquistion.Setup(m => m.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, principal, null)).
                ThrowsAsync(new MicrosoftIdentityWebChallengeUserException(new MsalUiRequiredException("user_null", "login required"), null, null));

            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);

            var httpContext = new DefaultHttpContext();

            var authProperties = fixture.Create<AuthenticationProperties>();

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(m => m.GetService(typeof(ITokenAcquisition))).Returns(mockTokenAcquistion.Object);

            httpContext.RequestServices = mockServiceProvider.Object;

            var o = new CookieAuthenticationOptions();

            var authScheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));

            var mockContext = new CookieValidatePrincipalContext(httpContext, authScheme, o, authTicket);

            // Act
            await _testClass.ValidatePrincipal(mockContext);

            mockTokenAcquistion.Verify(m => m.GetAccessTokenForUserAsync(It.IsAny<IEnumerable<string>>(), null, null, principal, null), Times.Once);
            // Assert
            Assert.IsNull(mockContext.Principal);
        }
    }
}