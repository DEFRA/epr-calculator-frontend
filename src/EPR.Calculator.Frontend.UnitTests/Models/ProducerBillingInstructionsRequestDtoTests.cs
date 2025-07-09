using System.Collections.Generic;
using System.Text.Json;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class ProducerBillingInstructionsRequestDtoTests
    {
        [TestMethod]
        public void Can_Construct_And_Assign_Properties()
        {
            var searchQuery = new ProducerBillingInstructionsSearchQueryDto
            {
                OrganisationId = 42,
                Status = new List<string> { "Accepted", "Rejected" }
            };

            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = searchQuery,
                PageNumber = 2,
                PageSize = 50
            };

            Assert.AreEqual(searchQuery, dto.SearchQuery);
            Assert.AreEqual(2, dto.PageNumber);
            Assert.AreEqual(50, dto.PageSize);
            Assert.AreEqual(42, dto.SearchQuery.OrganisationId);
            CollectionAssert.AreEqual(new List<string> { "Accepted", "Rejected" }, new List<string>(dto.SearchQuery.Status));
        }

        [TestMethod]
        public void Can_Serialize_And_Deserialize()
        {
            var searchQuery = new ProducerBillingInstructionsSearchQueryDto
            {
                OrganisationId = 99,
                Status = new List<string> { "Pending" }
            };

            var dto = new ProducerBillingInstructionsRequestDto
            {
                SearchQuery = searchQuery,
                PageNumber = 1,
                PageSize = 10
            };

            var json = JsonSerializer.Serialize(dto);
            var deserialized = JsonSerializer.Deserialize<ProducerBillingInstructionsRequestDto>(json);

            Assert.IsNotNull(deserialized);
            Assert.IsNotNull(deserialized.SearchQuery);
            Assert.AreEqual(99, deserialized.SearchQuery.OrganisationId);
            CollectionAssert.AreEqual(new List<string> { "Pending" }, new List<string>(deserialized.SearchQuery.Status));
            Assert.AreEqual(1, deserialized.PageNumber);
            Assert.AreEqual(10, deserialized.PageSize);
        }

        [TestMethod]
        public void Can_Construct_With_Null_Properties()
        {
            var dto = new ProducerBillingInstructionsRequestDto();

            Assert.IsNull(dto.SearchQuery);
            Assert.IsNull(dto.PageNumber);
            Assert.IsNull(dto.PageSize);
        }
    }
}