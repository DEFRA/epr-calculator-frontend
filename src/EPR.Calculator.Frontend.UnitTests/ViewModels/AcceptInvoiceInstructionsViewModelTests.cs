namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class AcceptInvoiceInstructionsViewModelTests
    {
        private AcceptInvoiceInstructionsViewModel _testClass;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<AcceptInvoiceInstructionsViewModel>();
        }

        [TestMethod]
        public void CanSetAndGetCalculationRunTitle()
        {
            var value = "Sample Title";

            _testClass.CalculationRunTitle = value;

            Assert.AreEqual(value, _testClass.CalculationRunTitle);
        }

        [TestMethod]
        public void CanSetAndGetAcceptAll()
        {
            var value = true;

            _testClass.AcceptAll = value;

            Assert.IsTrue(_testClass.AcceptAll);
        }

        [TestMethod]
        public void CanSetAndGetReturnUrl()
        {
            var value = "/home/return";

            _testClass.ReturnUrl = value;

            Assert.AreEqual(value, _testClass.ReturnUrl);
        }
    }
}