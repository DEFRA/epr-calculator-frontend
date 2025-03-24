namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LapcapUploadViewModelTests
    {
        private LapcapUploadViewModel _testClass;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<LapcapUploadViewModel>();
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