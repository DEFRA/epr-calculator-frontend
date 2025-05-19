using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.ViewModels;
using EPR.Calculator.Frontend.ViewModels.Enums;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class SetRunClassificationViewModelTests
    {
        private SetRunClassificationViewModel _testClass;

        private IFixture Fixture { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = Fixture.Create<SetRunClassificationViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetRunId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            _testClass.CalculatorRunDetails.RunId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CalculatorRunDetails.RunId);
        }

        [TestMethod]
        public void CanSetAndGetClassifyRunType()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            _testClass.ClassifyRunType = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ClassifyRunType);
        }
    }
}