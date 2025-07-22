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
    }
}