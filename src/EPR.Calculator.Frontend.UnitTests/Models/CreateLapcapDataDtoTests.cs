namespace EPR.Calculator.Frontend.UnitTests.Models
{
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CreateLapcapDataDtoTests
    {
        public CreateLapcapDataDtoTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            this.TestClass = Fixture.Create<CreateLapcapDataDto>();
            this.Original = new LapcapRefreshViewModel
            {
                FileName = TestClass.LapcapFileName,
                LapcapTemplateValue = TestClass.LapcapDataTemplateValues,
            };
            this.ParameterYear = TestClass.ParameterYear;
        }

        private CreateLapcapDataDto TestClass { get; init; }

        private IFixture Fixture { get; init; }

        private LapcapRefreshViewModel Original { get; init; }

        private string ParameterYear { get; init; }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CreateLapcapDataDto(this.Original, this.ParameterYear);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_CreateLapcapDataDto()
        {
            // Arrange
            var same = new CreateLapcapDataDto(this.Original, this.ParameterYear);
            var different = Fixture.Create<CreateLapcapDataDto>();

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
        public void CanSetAndGetParameterYear()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ParameterYear = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterYear);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataTemplateValues()
        {
            // Arrange
            var testValue = Fixture.Create<IEnumerable<LapcapDataTemplateValueDto>>();

            // Act
            this.TestClass.LapcapDataTemplateValues = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.LapcapDataTemplateValues);
        }

        [TestMethod]
        public void CanSetAndGetLapcapFileName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.LapcapFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LapcapFileName);
        }
    }
}