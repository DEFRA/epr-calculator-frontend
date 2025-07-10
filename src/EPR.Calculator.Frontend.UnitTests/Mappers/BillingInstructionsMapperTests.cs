using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.Mappers
{
    [TestClass]
    public class BillingInstructionsMapperTests
    {
        private BillingInstructionsMapper _mapper;

        [TestInitialize]
        public void Setup()
        {
            _mapper = new BillingInstructionsMapper();
        }

        [TestMethod]
        public void MapToViewModel_ShouldMapBasicFieldsCorrectly()
        {
            // Arrange
            var billingData = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 101,
                RunName = "Test Run",
                TotalRecords = 1,
                Records = new List<ProducerBillingInstructionsDto>
            {
                new ProducerBillingInstructionsDto
                {
                    ProducerId = 1,
                    ProducerName = "Org A",
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 500.00m,
                    BillingInstructionAcceptReject = "Accepted"
                }
            },
                AllProducerIds = new List<int> { 1 }
            };

            var paginationRequest = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var currentUser = "user123";
            var isSelectAll = true;

            // Act
            var result = _mapper.MapToViewModel(billingData, paginationRequest, currentUser, isSelectAll);

            // Assert
            Assert.AreEqual(currentUser, result.CurrentUser);
            Assert.AreEqual(101, result.CalculationRun.Id);
            Assert.AreEqual("Test Run", result.CalculationRun.Name);
            Assert.AreEqual(1, result.OrganisationBillingInstructions.Count);

            var org = result.OrganisationBillingInstructions.First();
            Assert.AreEqual(1, org.Id);
            Assert.AreEqual("Org A", org.OrganisationName);
            Assert.AreEqual(BillingInstruction.Initial, org.BillingInstruction);
            Assert.AreEqual(500.00m, org.InvoiceAmount);
            Assert.AreEqual(BillingStatus.Accepted, org.Status);

            Assert.AreEqual(true, result.OrganisationSelections.SelectAll);
            Assert.AreEqual(1, result.ProducerIds.Count());
        }

        [TestMethod]
        public void MapBillingInstruction_ShouldHandleVariousFormats()
        {
            var privateMethod = typeof(BillingInstructionsMapper)
                .GetMethod("MapBillingInstruction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.AreEqual(BillingInstruction.Initial, privateMethod.Invoke(null, new object[] { "initial" }));
            Assert.AreEqual(BillingInstruction.Initial, privateMethod.Invoke(null, new object[] { " Initial " }));
            Assert.AreEqual(BillingInstruction.Initial, privateMethod.Invoke(null, new object[] { "INITIAL" }));
            Assert.AreEqual(BillingInstruction.Delta, privateMethod.Invoke(null, new object[] { "delta" }));
            Assert.AreEqual(BillingInstruction.Noaction, privateMethod.Invoke(null, new object[] { null }));
            Assert.AreEqual(BillingInstruction.Noaction, privateMethod.Invoke(null, new object[] { "unknown" }));
        }

        [TestMethod]
        public void MapBillingStatus_ShouldHandleVariousFormats()
        {
            var privateMethod = typeof(BillingInstructionsMapper)
                .GetMethod("MapBillingStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

            Assert.AreEqual(BillingStatus.Accepted, privateMethod.Invoke(null, new object[] { "Accepted" }));
            Assert.AreEqual(BillingStatus.Pending, privateMethod.Invoke(null, new object[] { " pending " }));
            Assert.AreEqual(BillingStatus.Rejected, privateMethod.Invoke(null, new object[] { "REJECTED" }));
            Assert.AreEqual(BillingStatus.Noaction, privateMethod.Invoke(null, new object[] { "unknown" }));
            Assert.AreEqual(BillingStatus.Pending, privateMethod.Invoke(null, new object[] { null }));
        }

        [TestMethod]
        public void MapToViewModel_ShouldHandleNullProducerName()
        {
            // Arrange
            var billingData = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 2,
                RunName = null,
                TotalRecords = 1,
                Records = new List<ProducerBillingInstructionsDto>
            {
                new ProducerBillingInstructionsDto
                {
                    ProducerId = 99,
                    ProducerName = null,
                    SuggestedBillingInstruction = "delta",
                    SuggestedInvoiceAmount = 250.50m,
                    BillingInstructionAcceptReject = "pending"
                }
            },
                AllProducerIds = new List<int> { 99 }
            };

            var paginationRequest = new PaginationRequestViewModel { Page = 2, PageSize = 5 };
            var currentUser = "tester";
            var isSelectAll = false;

            // Act
            var result = _mapper.MapToViewModel(billingData, paginationRequest, currentUser, isSelectAll);

            // Assert
            var org = result.OrganisationBillingInstructions.First();
            Assert.AreEqual(string.Empty, org.OrganisationName);
            Assert.AreEqual(BillingInstruction.Delta, org.BillingInstruction);
            Assert.AreEqual(BillingStatus.Pending, org.Status);
            Assert.AreEqual("BillingInstructions_Index", result.TablePaginationModel.RouteName);
        }
    }
}
