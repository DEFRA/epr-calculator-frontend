using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class ParameterConfirmationControllerTests
{
    private const string SuperUserRole = "SASuperUser";
    private ParameterConfirmationController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        controller = new ParameterConfirmationController();
    }

    [TestMethod]
    public void Index_ReturnsExpectedViewAndConfirmationModel()
    {
        // Arrange

        // Act
        var result = controller.Index();
        var model = GetConfirmationModel(result);

        // Assert
        Assert.IsNotNull(result as ViewResult);
        Assert.AreEqual(ViewNames.ParameterConfirmationIndex, ((ViewResult)result).ViewName);
        Assert.AreEqual(ParameterConfirmation.Title, model.Title);
        Assert.AreEqual(ParameterConfirmation.Body, model.Body);
        Assert.AreEqual(ParameterConfirmation.RedirectController, model.RedirectController);
        Assert.AreEqual(ParameterConfirmation.SubmitText, model.SubmitText);
        CollectionAssert.AreEqual(ParameterConfirmation.AdditionalParagraphs.ToList(), model.AdditionalParagraphs);
    }

    [TestMethod]
    public void Index_CreatesNewModelAndParagraphCollectionPerCall()
    {
        // Arrange

        // Act
        var firstModel = GetConfirmationModel(controller.Index());
        var secondModel = GetConfirmationModel(controller.Index());

        // Assert
        Assert.AreNotSame(firstModel, secondModel);
        Assert.AreNotSame(firstModel.AdditionalParagraphs, secondModel.AdditionalParagraphs);
        CollectionAssert.AreEqual(firstModel.AdditionalParagraphs, secondModel.AdditionalParagraphs);
    }

    [TestMethod]
    public void ParameterConfirmationController_HasExpectedAuthorizeRole()
    {
        // Arrange

        // Act
        var authoriseAttribute = typeof(ParameterConfirmationController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        // Assert
        Assert.IsNotNull(authoriseAttribute);
        Assert.AreEqual(SuperUserRole, authoriseAttribute.Roles);
    }

    [TestMethod]
    public void Index_HasExpectedAuthorizeRole()
    {
        // Arrange
        var indexMethod = typeof(ParameterConfirmationController).GetMethod(nameof(ParameterConfirmationController.Index));

        // Act
        var authoriseAttribute = indexMethod?
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Cast<AuthorizeAttribute>()
            .SingleOrDefault();

        // Assert
        Assert.IsNotNull(indexMethod);
        Assert.IsNotNull(authoriseAttribute);
        Assert.AreEqual(SuperUserRole, authoriseAttribute.Roles);
    }

    private static ConfirmationViewModel GetConfirmationModel(IActionResult actionResult)
    {
        var viewResult = actionResult as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.IsInstanceOfType<ConfirmationViewModel>(viewResult.Model);

        return (ConfirmationViewModel)viewResult.Model;
    }
}
