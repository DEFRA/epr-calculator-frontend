using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class OrganisationModelTests
    {
        [TestMethod]
        public void Organisation_Properties_AreSetCorrectly()
        {
            // Arrange
            var billingInstruction = BillingInstruction.Initial; // Assuming Option1 exists
            var status = BillingStatus.Accepted; // Assuming Active exists

            // Act
            var org = new Organisation
            {
                Id = 1,
                OrganisationName = "Test Org",
                OrganisationId = 123,
                BillingInstruction = billingInstruction,
                InvoiceAmount = 99.99,
                Status = status
            };

            // Assert
            Assert.AreEqual(1, org.Id);
            Assert.AreEqual("Test Org", org.OrganisationName);
            Assert.AreEqual(123, org.OrganisationId);
            Assert.AreEqual(billingInstruction, org.BillingInstruction);
            Assert.AreEqual(99.99, org.InvoiceAmount);
            Assert.AreEqual(status, org.Status);
        }

        [TestMethod]
        public void Organisation_Equality_WorksForSameValues()
        {
            // Arrange
            var org1 = new Organisation
            {
                Id = 1,
                OrganisationName = "Org",
                OrganisationId = 100,
                BillingInstruction = BillingInstruction.Rebill,
                InvoiceAmount = 50,
                Status = BillingStatus.Pending
            };

            var org2 = new Organisation
            {
                Id = 1,
                OrganisationName = "Org",
                OrganisationId = 100,
                BillingInstruction = BillingInstruction.Rebill,
                InvoiceAmount = 50,
                Status = BillingStatus.Pending
            };

            // Assert
            Assert.AreEqual(org1, org2);
            Assert.IsTrue(org1 == org2);
        }

        [TestMethod]
        public void Organisation_IsImmutable()
        {
            // Arrange
            var org = new Organisation
            {
                Id = 1,
                OrganisationName = "Immutable Org",
                OrganisationId = 50,
                BillingInstruction = BillingInstruction.Delta,
                InvoiceAmount = 123.45,
                Status = BillingStatus.Pending
            };

            var modifiedOrg = org with { OrganisationName = "Modified Org" };

            Assert.AreNotEqual(org.OrganisationName, modifiedOrg.OrganisationName);
            Assert.AreEqual(org.Id, modifiedOrg.Id); // unchanged properties remain same
        }
    }
}
