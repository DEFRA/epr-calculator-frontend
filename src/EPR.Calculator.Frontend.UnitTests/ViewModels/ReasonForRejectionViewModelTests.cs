namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ReasonForRejectionViewModelTests
    {
        private AcceptRejectConfirmationViewModel _testClass = null!;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<AcceptRejectConfirmationViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetCalculationRunId()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<int>();

            // Act
            _testClass.CalculationRunId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CalculationRunId);
        }

        [TestMethod]
        public void CanSetAndGetCalculationRunName()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.CalculationRunName = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CalculationRunName);
        }

        [TestMethod]
        public void CanSetAndGetReason()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Reason = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Reason);
        }
    }
}