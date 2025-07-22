namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LapcapUploadViewModelTests
    {
        public LapcapUploadViewModelTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            TestClass = fixture.Create<LapcapUploadViewModel>();
        }

        private IFixture Fixture { get; set; }

        private LapcapUploadViewModel TestClass { get; set; }

        [TestMethod]
        public void CanSetAndGetErrors()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<List<ErrorViewModel>>();

            // Act
            TestClass.Errors = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.Errors);
        }

        [TestMethod]
        public void CanSetAndGetLapcapErrors()
        {
            // Arrange
            var testValue = Fixture.Create<List<CreateLapcapDataErrorDto>>();

            // Act
            this.TestClass.LapcapErrors = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.LapcapErrors);
        }

        [TestMethod]
        public void CanSetAndGetValidationErrors()
        {
            // Arrange
            var testValue = Fixture.Create<List<ValidationErrorDto>>();

            // Act
            this.TestClass.ValidationErrors = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.ValidationErrors);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataTemplateValue()
        {
            // Arrange
            var testValue = Fixture.Create<List<LapcapDataTemplateValueDto>>();

            // Act
            this.TestClass.LapcapDataTemplateValue = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.LapcapDataTemplateValue);
        }
    }
}