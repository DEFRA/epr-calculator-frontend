using System.Security.Claims;
using EPR.Calculator.Frontend.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class StandardErrorControllerTest
{
    private const string TestUserName = "test.user";

    private StandardErrorController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        controller = CreateController(TestUserName);
    }

    [TestMethod]
    public void Index()
    {
        // Act
        var result = controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);

        Assert.AreEqual(null, result.ViewName);
    }

    private static StandardErrorController CreateController(string? userName)
    {
        var identity = string.IsNullOrWhiteSpace(userName)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Name, userName)
            ]);

        return new StandardErrorController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            }
        };
    }
}
