namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClassifyCalculationRunScenerio2ViewModelTests
    {
        private ClassifyCalculationRunScenerio2ViewModel _testClass;
        private CalculatorRunStatusUpdateDto _calculatorRunStatus;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _calculatorRunStatus = fixture.Create<CalculatorRunStatusUpdateDto>();
            _testClass = fixture.Create<ClassifyCalculationRunScenerio2ViewModel>();
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new ClassifyCalculationRunScenerio2ViewModel { CalculatorRunStatus = _calculatorRunStatus };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanSetAndGetBackLink()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.BackLink = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BackLink);
        }

        [TestMethod]
        public void CanSetAndGetErrors()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<ErrorViewModel>();

            // Act
            _testClass.Errors = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.Errors);
        }
    }
}