using System.Text.Json;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class ProducerBillingInstructionsDtoTests
    {
        [TestMethod]
        public void Can_Construct_And_Assign_Properties()
        {
            var dto = new ProducerBillingInstructionsDto
            {
                ProducerName = "Test Producer",
                ProducerId = 42,
                SuggestedBillingInstruction = "Initial",
                SuggestedInvoiceAmount = 123.45m,
                BillingInstructionAcceptReject = "Accepted"
            };

            Assert.AreEqual("Test Producer", dto.ProducerName);
            Assert.AreEqual(42, dto.ProducerId);
            Assert.AreEqual("Initial", dto.SuggestedBillingInstruction);
            Assert.AreEqual(123.45m, dto.SuggestedInvoiceAmount);
            Assert.AreEqual("Accepted", dto.BillingInstructionAcceptReject);
        }

        [TestMethod]
        public void Can_Serialize_And_Deserialize()
        {
            var dto = new ProducerBillingInstructionsDto
            {
                ProducerName = "Serialize Producer",
                ProducerId = 99,
                SuggestedBillingInstruction = "Delta",
                SuggestedInvoiceAmount = 999.99m,
                BillingInstructionAcceptReject = "Rejected"
            };

            var json = JsonSerializer.Serialize(dto);
            var deserialized = JsonSerializer.Deserialize<ProducerBillingInstructionsDto>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(dto.ProducerName, deserialized.ProducerName);
            Assert.AreEqual(dto.ProducerId, deserialized.ProducerId);
            Assert.AreEqual(dto.SuggestedBillingInstruction, deserialized.SuggestedBillingInstruction);
            Assert.AreEqual(dto.SuggestedInvoiceAmount, deserialized.SuggestedInvoiceAmount);
            Assert.AreEqual(dto.BillingInstructionAcceptReject, deserialized.BillingInstructionAcceptReject);
        }
    }
}