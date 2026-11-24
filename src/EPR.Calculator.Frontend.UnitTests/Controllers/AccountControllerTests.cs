using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class AccountControllerTests
{
    private const string SignedOutUrl = "/Account/SignedOut";

    private Mock<ISession> session = null!;
    private AccountController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        session = new Mock<ISession>();

        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(u => u.Action(It.Is<UrlActionContext>(c => c.Action == nameof(AccountController.SignedOut))))
            .Returns(SignedOutUrl);

        controller = new AccountController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session.Object },
            },
            Url = urlHelper.Object,
        };
    }

    [TestMethod]
    public void SignOut_SignsOutOfCookieAndOpenIdConnect()
    {
        var result = controller.SignOutUser() as SignOutResult;

        Assert.IsNotNull(result);
        CollectionAssert.AreEquivalent(
            new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme },
            result.AuthenticationSchemes.ToArray());
    }

    [TestMethod]
    public void SignOut_ReturnsToSignedOutPage()
    {
        var result = controller.SignOutUser() as SignOutResult;

        Assert.IsNotNull(result);
        Assert.AreEqual(SignedOutUrl, result.Properties?.RedirectUri);
    }

    [TestMethod]
    public void SignOut_ClearsSession()
    {
        controller.SignOutUser();

        session.Verify(s => s.Clear(), Times.Once);
    }

    [TestMethod]
    public void SignedOut_ReturnsDefaultView()
    {
        var result = controller.SignedOut() as ViewResult;

        Assert.IsNotNull(result);
        Assert.IsNull(result.ViewName);
    }
}
