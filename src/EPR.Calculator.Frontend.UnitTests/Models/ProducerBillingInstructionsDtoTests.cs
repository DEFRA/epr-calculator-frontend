namespace EPR.Calculator.Frontend.UnitTests.Models
{
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerBillingInstructionsDtoTests
    {
        private ProducerBillingInstructionsDto _testClass;
        private string _producerName;
        private int _producerId;
        private string _suggestedBillingInstruction;
        private decimal _suggestedInvoiceAmount;
        private string _billingInstructionAcceptReject;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _producerName = fixture.Create<string>();
            _producerId = fixture.Create<int>();
            _suggestedBillingInstruction = fixture.Create<string>();
            _suggestedInvoiceAmount = fixture.Create<decimal>();
            _billingInstructionAcceptReject = fixture.Create<string>();
            _testClass = fixture.Create<ProducerBillingInstructionsDto>();
        }

        [TestMethod]
        public void CanInitialize()
        {
            // Act
            var instance = new ProducerBillingInstructionsDto
            {
                ProducerName = _producerName,
                ProducerId = _producerId,
                SuggestedBillingInstruction = _suggestedBillingInstruction,
                SuggestedInvoiceAmount = _suggestedInvoiceAmount,
                BillingInstructionAcceptReject = _billingInstructionAcceptReject
            };

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}