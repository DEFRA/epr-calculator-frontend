namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PostBillingFileViewModelTests
    {
        private PostBillingFileViewModel _testClass;
        private CalculatorRunStatusUpdateDto _calculatorRunStatus;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _calculatorRunStatus = fixture.Create<CalculatorRunStatusUpdateDto>();
            _testClass = fixture.Create<PostBillingFileViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetBillingFileSentDate()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.BillingFileSentDate = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BillingFileSentDate);
        }

        [TestMethod]
        public void CanSetAndGetBillingFileRunBy()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.BillingFileRunBy = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BillingFileRunBy);
        }

        [TestMethod]
        public void CanSetAndGetBillingFileSentBy()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.BillingFileSentBy = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.BillingFileSentBy);
        }

        [TestMethod]
        public void CanSetAndGetSelectedCalcRunOption()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<string>();

            // Act
            _testClass.SelectedCalcRunOption = testValue;

            // Assert
            Assert.AreEqual(testValue, _testClass.SelectedCalcRunOption);
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