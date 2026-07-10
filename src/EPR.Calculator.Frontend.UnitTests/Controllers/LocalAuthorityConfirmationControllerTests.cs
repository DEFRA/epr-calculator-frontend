using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class LocalAuthorityConfirmationControllerTests
{
    private LocalAuthorityConfirmationController controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        controller = new LocalAuthorityConfirmationController();
    }

    [TestMethod]
    public void Index_ReturnsExpectedConfirmationViewModel()
    {
        // Arrange

        // Act
        var viewResult = controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(ViewNames.LocalAuthorityConfirmationIndex, viewResult.ViewName);

        var viewModel = viewResult.Model as ConfirmationViewModel;
        Assert.IsNotNull(viewModel);
        Assert.AreEqual(LocalAuthorityConfirmation.Title, viewModel.Title);
        Assert.AreEqual(LocalAuthorityConfirmation.Body, viewModel.Body);
        Assert.AreEqual(LocalAuthorityConfirmation.RedirectController, viewModel.RedirectController);
        Assert.AreEqual(LocalAuthorityConfirmation.SubmitText, viewModel.SubmitText);
        Assert.AreEqual(LocalAuthorityConfirmation.AdditionalParagraphs.Count, viewModel.AdditionalParagraphs.Count);

        for (var index = 0; index < LocalAuthorityConfirmation.AdditionalParagraphs.Count; index++)
            Assert.AreEqual(LocalAuthorityConfirmation.AdditionalParagraphs[index], viewModel.AdditionalParagraphs[index]);
    }

    [TestMethod]
    public void Index_CreatesNewAdditionalParagraphCollection_ForEachCall()
    {
        // Arrange

        // Act
        var firstResult = controller.Index() as ViewResult;
        var firstModel = firstResult?.Model as ConfirmationViewModel;
        var secondResult = controller.Index() as ViewResult;
        var secondModel = secondResult?.Model as ConfirmationViewModel;

        // Assert
        Assert.IsNotNull(firstResult);
        Assert.IsNotNull(secondResult);
        Assert.IsNotNull(firstModel);
        Assert.IsNotNull(secondModel);
        Assert.AreNotSame(firstModel, secondModel);

        Assert.AreNotSame(firstModel.AdditionalParagraphs, secondModel.AdditionalParagraphs);
        CollectionAssert.AreEqual(firstModel.AdditionalParagraphs, secondModel.AdditionalParagraphs);
    }
}
