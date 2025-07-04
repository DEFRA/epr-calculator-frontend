using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class CalculationRunOrganisationBillingInstructionsDTOTests
    {
        [TestMethod]
        public void CalculationRunOrganisationBillingInstructionsDTO_ShouldInitializeProperties()
        {
            // Arrange
            var calculationRun = new CalculationRunForBillingInstructionsDTO { Id = 1, Name = "Test Run" };
            var organisations = new List<Organisation>
           {
               new Organisation
               {
                   Id = 1,
                   OrganisationName = "Org1",
                   OrganisationId = 101,
                   BillingInstruction = BillingInstruction.Initial,
                   InvoiceAmount = 100.50,
                   Status = BillingStatus.Accepted
               },
               new Organisation
               {
                   Id = 2,
                   OrganisationName = "Org2",
                   OrganisationId = 102,
                   BillingInstruction = BillingInstruction.Delta,
                   InvoiceAmount = 200.75,
                   Status = BillingStatus.Pending
               }
           };

            // Act
            var dto = new CalculationRunOrganisationBillingInstructionsDTO
            {
                CalculationRun = calculationRun,
                Organisations = organisations
            };

            // Assert
            Assert.AreEqual(calculationRun, dto.CalculationRun);
            Assert.AreEqual(organisations, dto.Organisations);
        }
    }
}
