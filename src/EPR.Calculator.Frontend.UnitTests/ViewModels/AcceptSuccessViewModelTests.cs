using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels;

[TestClass]
public class AcceptSuccessViewModelTests
{
    [DataTestMethod]
    [DataRow(1, "1 billing instruction accepted")]
    [DataRow(3, "3 billing instructions accepted")]
    [DataRow(2008, "2,008 billing instructions accepted")]
    public void GetHeading_ReturnsNumberOfInstructionsAccepted(int count, string expected)
    {
        // Arrange
        var viewModel = new AcceptSuccessViewModel { Count = count };

        // Act
        var heading = viewModel.GetHeading();

        // Assert
        Assert.AreEqual(expected, heading);
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(3)]
    [DataRow(2008)]
    public void GetBody_WhenNothingPending_ReturnsAllDecisionsSaved(int count)
    {
        // Arrange
        var viewModel = new AcceptSuccessViewModel { Count = count, OrganisationId = count == 1 ? 123456 : null };

        // Act
        var body = viewModel.GetBody(pendingCount: 0);

        // Assert
        Assert.AreEqual("All billing decisions for this run have been saved.", body);
    }

    [TestMethod]
    public void GetBody_WhenOneAcceptedAndSomePending_ReturnsOrganisationNameAndId()
    {
        // Arrange
        var viewModel = new AcceptSuccessViewModel { Count = 1, OrganisationId = 123456, OrganisationName = "Global Solutions Inc." };

        // Act
        var body = viewModel.GetBody(pendingCount: 2007);

        // Assert
        Assert.AreEqual("Your decision for Global Solutions Inc. (123456) has been saved.", body);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    public void GetBody_WhenOneAcceptedWithoutName_ReturnsOrganisationIdOnly(string? organisationName)
    {
        // Arrange
        var viewModel = new AcceptSuccessViewModel { Count = 1, OrganisationId = 123456, OrganisationName = organisationName };

        // Act
        var body = viewModel.GetBody(pendingCount: 2007);

        // Assert
        Assert.AreEqual("Your decision for (123456) has been saved.", body);
    }

    [DataTestMethod]
    [DataRow(3, "Your decisions for the 3 selected organisations have been saved.")]
    [DataRow(2008, "Your decisions for the 2,008 selected organisations have been saved.")]
    public void GetBody_WhenSeveralAcceptedAndSomePending_ReturnsNumberOfOrganisations(int count, string expected)
    {
        // Arrange
        var viewModel = new AcceptSuccessViewModel { Count = count };

        // Act
        var body = viewModel.GetBody(pendingCount: 5);

        // Assert
        Assert.AreEqual(expected, body);
    }
}
