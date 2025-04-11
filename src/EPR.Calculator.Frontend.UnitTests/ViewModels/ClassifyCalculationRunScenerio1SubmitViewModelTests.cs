using AutoFixture;
using AutoFixture.AutoMoq;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class ClassifyCalculationRunScenerio1SubmitViewModelTests
    {
        private ClassifyCalculationRunScenerio1SubmitViewModel _testClass;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<ClassifyCalculationRunScenerio1SubmitViewModel>();
        }

        [TestMethod]
        public void ImplementsIEquatable_ClassifyCalculationRunScenerio1SubmitViewModel()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var same = new ClassifyCalculationRunScenerio1SubmitViewModel();
            var different = fixture.Create<ClassifyCalculationRunScenerio1SubmitViewModel>();

            // Assert
            Assert.IsFalse(_testClass.Equals(default(object)));
            Assert.IsFalse(_testClass.Equals(new object()));
            Assert.IsTrue(_testClass.Equals((object)same));
            Assert.IsFalse(_testClass.Equals((object)different));
            Assert.IsTrue(_testClass.Equals(same));
            Assert.IsFalse(_testClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), _testClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), _testClass.GetHashCode());
            Assert.IsTrue(_testClass == same);
            Assert.IsFalse(_testClass == different);
            Assert.IsFalse(_testClass != same);
            Assert.IsTrue(_testClass != different);
        }

        [TestMethod]
        public void CanSetAndGetRunId()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<int>();

            // Act
            _testClass.RunId = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.RunId);
        }

        [TestMethod]
        public void CanSetAndGetClassifyRunType()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<ClassifyRunType?>();

            // Act
            _testClass.ClassifyRunType = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.ClassifyRunType);
        }
    }
}