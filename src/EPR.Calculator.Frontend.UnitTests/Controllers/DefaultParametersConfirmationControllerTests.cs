using System.Security.Claims;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class DefaultParametersConfirmationControllerTests
{
    private const int RelativeYearStartingMonth = 4;
    private const int SelectedRelativeYear = 2026;
    private IConfiguration configuration = null!;
    private InMemorySession session = null!;

    private DefaultParametersConfirmationController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    CommonConstants.RelativeYearStartingMonth,
                    RelativeYearStartingMonth.ToString())
            ])
            .Build();

        session = new InMemorySession();
        session.SetInt32(SessionConstants.RelativeYear, SelectedRelativeYear);
        controller = BuildController(session);
    }

    [TestMethod]
    public void Index_ReturnsExpectedViewAndConfirmationModel()
    {
        // Act
        var result = controller.Index();
        var model = GetViewModel(result);

        // Assert
        Assert.IsNotNull(result as ViewResult);
        Assert.AreEqual("Views/CsvUpload/Success", ((ViewResult)result).ViewName);
        Assert.AreEqual("Calculator parameters saved for 2026 to 2027", model.Title);
    }

    private static CsvUploadSuccessViewModel GetViewModel(IActionResult actionResult)
    {
        var viewResult = actionResult as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.IsInstanceOfType<CsvUploadSuccessViewModel>(viewResult.Model);

        return (CsvUploadSuccessViewModel)viewResult.Model;
    }

    private DefaultParametersConfirmationController BuildController(ISession controllerSession)
    {
        var urlHelper = new Mock<IUrlHelper>();
        urlHelper
            .Setup(helper => helper.Action(It.IsAny<UrlActionContext>()))
            .Returns((UrlActionContext context) => $"/{context.Controller}/{context.Action}");

        return new DefaultParametersConfirmationController(configuration)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = controllerSession,
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "test.user@paycal")], "TestAuth"))
                }
            },
            Url = urlHelper.Object
        };
    }
}
