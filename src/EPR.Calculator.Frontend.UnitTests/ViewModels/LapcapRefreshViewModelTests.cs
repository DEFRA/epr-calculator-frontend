namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System.Collections.Generic;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LapcapRefreshViewModelTests
    {
        public LapcapRefreshViewModelTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            TestClass = fixture.Create<LapcapRefreshViewModel>();
        }

        private LapcapRefreshViewModel TestClass { get; set; }

        private IFixture Fixture { get; init; }

        [TestMethod]
        public void CanSetAndGetLapcapTemplateValue()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            var testValue = fixture.Create<List<LapcapDataTemplateValueDto>>();

            // Act
            TestClass = new LapcapRefreshViewModel
            {
                LapcapTemplateValue = testValue,
            };

            // Assert
            Assert.AreSame(testValue, TestClass.LapcapTemplateValue);
        }

        [TestMethod]
        public void ImplementsIEquatable_LapcapRefreshViewModel()
        {
            // Arrange
            var same = new LapcapRefreshViewModel
            {
                LapcapTemplateValue = TestClass.LapcapTemplateValue,
            };
            var different = Fixture.Create<LapcapRefreshViewModel>();

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
    }
}