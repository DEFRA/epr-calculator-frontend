using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.UnitTests.Models
{
    [TestClass]
    public class CalculationRunOrganisationBillingInstructionsDTOTests
    {
        [TestMethod]
        public void CalculationRunOrganisationBillingInstructionsDTO_DefaultInitialization_ShouldInitializeProperties()
        {
            // Act
            var dto = new CalculationRunOrganisationBillingInstructionsDto();

            // Assert
            Assert.IsNotNull(dto.CalculationRun);
            Assert.IsNotNull(dto.Organisations);
            Assert.AreEqual(0, dto.Organisations.Count);
        }

        [TestMethod]
        public void CalculationRunOrganisationBillingInstructionsDTO_SetProperties_ShouldRetainValues()
        {
            // Arrange
            var calculationRun = new CalculationRunForBillingInstructionsDto();
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
            var dto = new CalculationRunOrganisationBillingInstructionsDto
            {
                CalculationRun = calculationRun,
                Organisations = organisations
            };

            // Assert
            Assert.AreEqual(calculationRun, dto.CalculationRun);
            Assert.AreEqual(organisations, dto.Organisations);
            Assert.AreEqual(2, dto.Organisations.Count);
        }
    }
}
