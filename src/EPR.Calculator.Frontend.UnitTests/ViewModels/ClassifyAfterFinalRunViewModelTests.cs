using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class ClassifyAfterFinalRunViewModelTests
    {
        private ClassifyAfterFinalRunViewModel _testClass;
        private CalculatorRunStatusUpdateDto _calculatorRunStatus;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _calculatorRunStatus = fixture.Create<CalculatorRunStatusUpdateDto>();
            _testClass = fixture.Create<ClassifyAfterFinalRunViewModel>();
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new ClassifyAfterFinalRunViewModel { CalculatorRunStatus = _calculatorRunStatus };

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
