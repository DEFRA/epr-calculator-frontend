namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ClassifyRunConfirmationViewModelTests
    {
        private ClassifyRunConfirmationViewModel _testClass;
        private CalculatorRunDto _data;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _data = fixture.Create<CalculatorRunDto>();
            _testClass = fixture.Create<ClassifyRunConfirmationViewModel>();
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new ClassifyRunConfirmationViewModel { Data = _data };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanSetAndGetDownloadResultURL()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<Uri>();

            // Act
            _testClass.DownloadResultURL = testValue;

            // Assert
            Assert.AreSame(testValue, _testClass.DownloadResultURL);
        }

        [TestMethod]
        public void CanSetAndGetDownloadErrorURL()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.DownloadErrorURL = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.DownloadErrorURL);
        }

        [TestMethod]
        public void CanSetAndGetDownloadTimeout()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<int?>();

            // Act
            _testClass.DownloadTimeout = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.DownloadTimeout);
        }

        [TestMethod]
        public void CanSetAndGetDownloadFileName()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.DownloadBillingFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.DownloadBillingFileName);
        }
    }
}