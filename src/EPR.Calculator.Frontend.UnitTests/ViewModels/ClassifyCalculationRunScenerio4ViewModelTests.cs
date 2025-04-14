using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class ClassifyCalculationRunScenerio4ViewModelTests
    {
        private ClassifyCalculationRunScenerio4ViewModel _testClass;
        private CalculatorRunStatusUpdateDto _calculatorRunStatus;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _calculatorRunStatus = fixture.Create<CalculatorRunStatusUpdateDto>();
            _testClass = fixture.Create<ClassifyCalculationRunScenerio4ViewModel>();
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new ClassifyCalculationRunScenerio4ViewModel { CalculatorRunStatus = _calculatorRunStatus };

            // Assert
            Assert.IsNotNull(instance);
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
