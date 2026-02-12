using EPR.Calculator.Frontend.Models;
using Newtonsoft.Json;


namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class ProducerBillingInstructionsHttpPutRequestDtoTests
    {
        [TestMethod]
        public void Can_Construct_And_Assign_Properties()
        {
            var dto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = new List<int> { 1, 2, 3 },
                Status = "Accepted",
                ReasonForRejection = null,
            };

            CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, new List<int>(dto.OrganisationIds));
            Assert.AreEqual("Accepted", dto.Status);
            Assert.IsNull(dto.ReasonForRejection);
        }

        [TestMethod]
        public void Can_Serialize_And_Deserialize()
        {
            var dto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = new List<int> { 10, 20 },
                Status = "Rejected",
                ReasonForRejection = "Invalid data",
            };

            var json = JsonConvert.SerializeObject(dto);
            var deserialized = JsonConvert.DeserializeObject<ProducerBillingInstructionsHttpPutRequestDto>(json);

            Assert.IsNotNull(deserialized);
            CollectionAssert.AreEqual(new List<int> { 10, 20 }, new List<int>(deserialized.OrganisationIds));
            Assert.AreEqual("Rejected", deserialized.Status);
            Assert.AreEqual("Invalid data", deserialized.ReasonForRejection);
        }
    }
}