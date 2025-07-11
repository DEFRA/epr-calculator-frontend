using System.Text.Json;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class ProducerBillingInstructionsResponseDtoTests
    {
        [TestMethod]
        public void Can_Construct_And_Assign_Properties()
        {
            var record = new ProducerBillingInstructionsDto
            {
                ProducerName = "Test",
                ProducerId = 1,
                SuggestedBillingInstruction = "Initial",
                SuggestedInvoiceAmount = 10.0m,
                BillingInstructionAcceptReject = "Accepted"
            };

            var response = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 100,
                RunName = "Run 2025",
                PageNumber = 2,
                PageSize = 50,
                TotalRecords = 200,
                Records = new List<ProducerBillingInstructionsDto> { record },
                AllProducerIds = new List<int> { 1, 2, 3 },
                TotalAcceptedRecords = 150,
                TotalRejectedRecords = 30,
                TotalPendingRecords = 20
            };

            Assert.AreEqual(100, response.CalculatorRunId);
            Assert.AreEqual("Run 2025", response.RunName);
            Assert.AreEqual(2, response.PageNumber);
            Assert.AreEqual(50, response.PageSize);
            Assert.AreEqual(200, response.TotalRecords);
            Assert.AreEqual(1, response.Records.Count);
            Assert.AreEqual("Test", response.Records[0].ProducerName);
            Assert.AreEqual(3, response.AllProducerIds?.Count());
            Assert.AreEqual(150, response.TotalAcceptedRecords);
            Assert.AreEqual(30, response.TotalRejectedRecords);
            Assert.AreEqual(20, response.TotalPendingRecords);
        }

        [TestMethod]
        public void Can_Serialize_And_Deserialize()
        {
            var record = new ProducerBillingInstructionsDto
            {
                ProducerName = "SerializeTest",
                ProducerId = 2,
                SuggestedBillingInstruction = "Rebill",
                SuggestedInvoiceAmount = 99.9m,
                BillingInstructionAcceptReject = "Pending"
            };

            var response = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 123,
                RunName = "RunName",
                PageNumber = 1,
                PageSize = 20,
                TotalRecords = 1,
                Records = new List<ProducerBillingInstructionsDto> { record }
            };

            var json = JsonSerializer.Serialize(response);
            var deserialized = JsonSerializer.Deserialize<ProducerBillingInstructionsResponseDto>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(response.CalculatorRunId, deserialized.CalculatorRunId);
            Assert.AreEqual(response.RunName, deserialized.RunName);
            Assert.AreEqual(response.PageNumber, deserialized.PageNumber);
            Assert.AreEqual(response.PageSize, deserialized.PageSize);
            Assert.AreEqual(response.TotalRecords, deserialized.TotalRecords);
            Assert.AreEqual(1, deserialized.Records.Count);
            Assert.AreEqual("SerializeTest", deserialized.Records[0].ProducerName);
        }
    }
}