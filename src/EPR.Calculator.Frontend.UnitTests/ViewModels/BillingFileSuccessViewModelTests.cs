namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BillingFileSuccessViewModelTests
    {
        public BillingFileSuccessViewModelTests()
        {
            Fixture = new Fixture().Customize(new AutoMoqCustomization());
            this.TestClass = Fixture.Create<BillingFileSuccessViewModel>();
        }

        private IFixture Fixture { get; init; }

        private BillingFileSuccessViewModel TestClass { get; init; }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new BillingFileSuccessViewModel
            {
                ConfirmationViewModel = Fixture.Create<ConfirmationViewModel>(),
            };

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void ImplementsIEquatable_BillingFileSuccessViewModel()
        {
            // Arrange
            var same = new BillingFileSuccessViewModel
            {
                ConfirmationViewModel = this.TestClass.ConfirmationViewModel,
                CurrentUser = this.TestClass.CurrentUser,
                BackLinkViewModel = this.TestClass.BackLinkViewModel,
            };
            var different = Fixture.Create<BillingFileSuccessViewModel>();

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