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

            var testValue = fixture.Create<ErrorViewModel>();

            // Act
            TestClass.Errors = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.Errors);
        }

        [TestMethod]
        public void ImplementsIEquatable_LapcapUploadViewModel()
        {
            // Arrange
            var same = new LapcapUploadViewModel
            {
                CurrentUser = this.TestClass.CurrentUser,
                Errors = this.TestClass.Errors,
                LapcapDataTemplateValue = this.TestClass.LapcapDataTemplateValue,
                LapcapErrors = this.TestClass.LapcapErrors,
                ValidationErrors = this.TestClass.ValidationErrors,
            };

            var different = Fixture.Create<LapcapUploadViewModel>();

            // Assert
            Assert.IsFalse(this.TestClass.Equals(default(object)));
            Assert.IsFalse(this.TestClass.Equals(new object()));
            Assert.IsTrue(this.TestClass.Equals((object)same));
            Assert.IsFalse(this.TestClass.Equals((object)different));
            Assert.IsTrue(this.TestClass.Equals(same));
            Assert.IsFalse(this.TestClass.Equals(different));
            Assert.AreEqual(same.GetHashCode(), this.TestClass.GetHashCode());
            Assert.AreNotEqual(different.GetHashCode(), this.TestClass.GetHashCode());
            Assert.IsTrue(this.TestClass == same);
            Assert.IsFalse(this.TestClass == different);
            Assert.IsFalse(this.TestClass != same);
            Assert.IsTrue(this.TestClass != different);
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