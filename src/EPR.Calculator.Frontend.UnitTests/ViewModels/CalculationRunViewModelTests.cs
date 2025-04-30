using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class CalculationRunViewModelTests
    {
        private CalculationRunViewModel _testClass;
        private CalculationRun _calculationRun;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _calculationRun = fixture.Create<CalculationRun>();
            _testClass = fixture.Create<CalculationRunViewModel>();
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalculationRunViewModel(_calculationRun);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<int>();

            // Act
            _testClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Name);
        }

        [TestMethod]
        public void CanSetAndGetCreatedAt()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.CreatedAt = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CreatedAt);
        }

        [TestMethod]
        public void CanSetAndGetCreatedBy()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.CreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.CreatedBy);
        }

        [TestMethod]
        public void CanSetAndGetStatus()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<RunClassification>();

            // Act
            _testClass.Status = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.Status);
        }

        [TestMethod]
        public void CanSetAndGetTagStyle()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.TagStyle = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.TagStyle);
        }

        [TestMethod]
        public void CanSetAndGetShowRunDetailLink()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<bool>();

            // Act
            _testClass.ShowRunDetailLink = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ShowRunDetailLink);
        }

        [TestMethod]
        public void CanSetAndGetShowErrorLink()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<bool>();

            // Act
            _testClass.ShowErrorLink = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ShowErrorLink);
        }

        [TestMethod]
        public void CanGetTurnOffFeatureUrl()
        {
            // Assert
            Assert.IsInstanceOfType(_testClass.TurnOffFeatureUrl, typeof(string));

            Assert.AreEqual(_testClass.TurnOffFeatureUrl, $"/ViewCalculationRunDetails/{_testClass.Id}");
        }

        [TestMethod]
        public void CanGetTurnOnFeatureUrl()
        {
            // Assert
            Assert.IsInstanceOfType(_testClass.TurnOnFeatureUrl, typeof(string));

            Assert.AreEqual(_testClass.TurnOnFeatureUrl, "Dashboard");
        }
    }
}