using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels.Post;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class ClassifyCalculationRunScenerio1SubmitViewModelTests
    {
        private ClassifyCalculationRunScenerio1SubmitViewModel _testClass;

        private IFixture Fixture { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = Fixture.Create<ClassifyCalculationRunScenerio1SubmitViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetRunId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            _testClass.RunId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.RunId);
        }

        [TestMethod]
        public void CanSetAndGetClassifyRunType()
        {
            // Arrange
            var testValue = Fixture.Create<ClassifyRunType?>();

            // Act
            _testClass.ClassifyRunType = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ClassifyRunType);
        }
    }
}