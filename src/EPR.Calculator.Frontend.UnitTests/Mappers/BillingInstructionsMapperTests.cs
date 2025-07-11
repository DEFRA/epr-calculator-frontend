using System.Collections.Generic;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Mappers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.Mappers
{
    [TestClass]
    public class BillingInstructionsMapperTests
    {
        private readonly BillingInstructionsMapper _mapper = new();

        [TestMethod]
        [DataRow("Noaction", BillingInstruction.Noaction)]
        [DataRow("Initial", BillingInstruction.Initial)]
        [DataRow("Delta", BillingInstruction.Delta)]
        [DataRow("Rebill", BillingInstruction.Rebill)]
        [DataRow("Cancelbill", BillingInstruction.Cancelbill)]
        [DataRow("noaction", BillingInstruction.Noaction)]
        [DataRow("initial", BillingInstruction.Initial)]
        [DataRow("delta", BillingInstruction.Delta)]
        [DataRow("rebill", BillingInstruction.Rebill)]
        [DataRow("cancelbill", BillingInstruction.Cancelbill)]
        [DataRow("No Action", BillingInstruction.Noaction)]
        [DataRow("Initial ", BillingInstruction.Initial)]
        [DataRow(" DELTA", BillingInstruction.Delta)]
        [DataRow("Re-bill", BillingInstruction.Rebill)]
        [DataRow("cancel_bill", BillingInstruction.Cancelbill)]
        [DataRow("", BillingInstruction.Noaction)]
        [DataRow(null, BillingInstruction.Noaction)]
        [DataRow("unknown", BillingInstruction.Noaction)]
        public void MapBillingInstruction_Handles_All_Values(string input, BillingInstruction expected)
        {
            // Use reflection to call the private method
            var method = typeof(BillingInstructionsMapper).GetMethod("MapBillingInstruction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (BillingInstruction)method.Invoke(_mapper, new object[] { input });
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow("Noaction", BillingStatus.Noaction)]
        [DataRow("Accepted", BillingStatus.Accepted)]
        [DataRow("Rejected", BillingStatus.Rejected)]
        [DataRow("Pending", BillingStatus.Pending)]
        [DataRow("noaction", BillingStatus.Noaction)]
        [DataRow("accepted", BillingStatus.Accepted)]
        [DataRow("rejected", BillingStatus.Rejected)]
        [DataRow("pending", BillingStatus.Pending)]
        [DataRow("No Action", BillingStatus.Noaction)]
        [DataRow("Accepted ", BillingStatus.Accepted)]
        [DataRow(" REJECTED", BillingStatus.Rejected)]
        [DataRow("pending", BillingStatus.Pending)]
        [DataRow("no_action", BillingStatus.Noaction)]
        [DataRow("", BillingStatus.Pending)]
        [DataRow(null, BillingStatus.Pending)]
        [DataRow("unknown", BillingStatus.Noaction)]
        public void MapBillingStatus_Handles_All_Values(string input, BillingStatus expected)
        {
            // Use reflection to call the private method
            var method = typeof(BillingInstructionsMapper).GetMethod("MapBillingStatus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (BillingStatus)method.Invoke(_mapper, new object[] { input });
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void MapToViewModel_Maps_All_Properties_Correctly()
        {
            var billingData = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 123,
                RunName = "Test Run",
                PageNumber = 2,
                PageSize = 10,
                TotalRecords = 1,
                Records = new List<ProducerBillingInstructionsDto>
                {
                    new ProducerBillingInstructionsDto
                    {
                        ProducerId = 1,
                        ProducerName = "Producer A",
                        SuggestedBillingInstruction = "Initial",
                        SuggestedInvoiceAmount = 100.5m,
                        BillingInstructionAcceptReject = "Accepted"
                    }
                }
            };
            var request = new PaginationRequestViewModel { Page = 2, PageSize = 10 };
            var currentUser = "Test User";

            var result = _mapper.MapToViewModel(billingData, request, currentUser, true, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(currentUser, result.CurrentUser);
            Assert.AreEqual(123, result.CalculationRun.Id);
            Assert.AreEqual("Test Run", result.CalculationRun.Name);
            Assert.AreEqual(2, result.TablePaginationModel.CurrentPage);
            Assert.AreEqual(10, result.TablePaginationModel.PageSize);
            Assert.AreEqual(1, result.TablePaginationModel.TotalRecords);
            Assert.AreEqual(RouteNames.BillingInstructionsIndex, result.TablePaginationModel.RouteName);
            Assert.AreEqual(123, result.TablePaginationModel.RouteValues[BillingInstructionConstants.CalculationRunIdKey]);
            Assert.AreEqual(null, result.TablePaginationModel.RouteValues[BillingInstructionConstants.OrganisationIdKey]);
            Assert.AreEqual(null, result.TablePaginationModel.RouteValues[BillingInstructionConstants.BillingStatus]);

            var orgs = result.TablePaginationModel.Records as List<Organisation>;
            Assert.IsNotNull(orgs);
            Assert.AreEqual(1, orgs.Count);
            var org = orgs[0];
            Assert.AreEqual(1, org.Id);
            Assert.AreEqual("Producer A", org.OrganisationName);
            Assert.AreEqual(BillingInstruction.Initial, org.BillingInstruction);
            Assert.AreEqual(100.5m, org.InvoiceAmount);
            Assert.AreEqual(BillingStatus.Accepted, org.Status);
        }

        [TestMethod]
        public void MapToViewModel_Handles_Empty_Records()
        {
            var billingData = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 1,
                RunName = "Empty Run",
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 0,
                Records = new List<ProducerBillingInstructionsDto>()
            };
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var currentUser = "User";

            var result = _mapper.MapToViewModel(billingData, request, currentUser, false, false);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, ((List<Organisation>)result.TablePaginationModel.Records).Count);
        }

        [TestMethod]
        public void MapToViewModel_Handles_Null_And_Empty_Strings()
        {
            var billingData = new ProducerBillingInstructionsResponseDto
            {
                CalculatorRunId = 2,
                RunName = null,
                PageNumber = 1,
                PageSize = 10,
                TotalRecords = 1,
                Records = new List<ProducerBillingInstructionsDto>
                {
                    new ProducerBillingInstructionsDto
                    {
                        ProducerId = 0,
                        ProducerName = null,
                        SuggestedBillingInstruction = null,
                        SuggestedInvoiceAmount = 0,
                        BillingInstructionAcceptReject = null
                    }
                }
            };
            var request = new PaginationRequestViewModel { Page = 1, PageSize = 10 };
            var currentUser = "User";

            var result = _mapper.MapToViewModel(billingData, request, currentUser, false, true);

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.CalculationRun.Name);
            var orgs = result.TablePaginationModel.Records as List<Organisation>;
            Assert.IsNotNull(orgs);
            Assert.AreEqual(1, orgs.Count);
            var org = orgs[0];
            Assert.AreEqual(string.Empty, org.OrganisationName);
            Assert.AreEqual(BillingInstruction.Noaction, org.BillingInstruction);
            Assert.AreEqual(BillingStatus.Pending, org.Status);
        }
    }
}