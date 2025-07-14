using System.Text.Json;
using EPR.Calculator.Frontend.Models;

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
                AuthorizationToken = "Bearer token"
            };

            CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, new List<int>(dto.OrganisationIds));
            Assert.AreEqual("Accepted", dto.Status);
            Assert.IsNull(dto.ReasonForRejection);
            Assert.AreEqual("Bearer token", dto.AuthorizationToken);
        }

        [TestMethod]
        public void Can_Serialize_And_Deserialize()
        {
            var dto = new ProducerBillingInstructionsHttpPutRequestDto
            {
                OrganisationIds = new List<int> { 10, 20 },
                Status = "Rejected",
                ReasonForRejection = "Invalid data",
                AuthorizationToken = "Bearer xyz"
            };

            var json = JsonSerializer.Serialize(dto);
            var deserialized = JsonSerializer.Deserialize<ProducerBillingInstructionsHttpPutRequestDto>(json);

            Assert.IsNotNull(deserialized);
            CollectionAssert.AreEqual(new List<int> { 10, 20 }, new List<int>(deserialized.OrganisationIds));
            Assert.AreEqual("Rejected", deserialized.Status);
            Assert.AreEqual("Invalid data", deserialized.ReasonForRejection);
            Assert.AreEqual("Bearer xyz", deserialized.AuthorizationToken);
        }

        [TestMethod]
        public void Deserialization_Without_AuthorizationToken_Throws_JsonException()
        {
            // JSON missing the required AuthorizationToken property
            var json = @"{
                    ""OrganisationIds"": [1, 2],
                    ""Status"": ""Accepted""
                }";

            Assert.ThrowsException<System.Text.Json.JsonException>(() =>
            {
                JsonSerializer.Deserialize<ProducerBillingInstructionsHttpPutRequestDto>(json);
            });
        }
    }
}