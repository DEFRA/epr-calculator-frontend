namespace EPR.Calculator.Frontend.UnitTests.Mappers
{
    using System;
    using AutoFixture;
    using AutoFixture.AutoMoq;
    using EPR.Calculator.Frontend.Mappers;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BillingInstructionsMapperTests
    {
        private BillingInstructionsMapper _testClass;

        [TestInitialize]
        public void SetUp()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _testClass = fixture.Create<BillingInstructionsMapper>();
        }

        [TestMethod]
        public void CanCallMapToViewModel()
        {
            // Arrange
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            var billingData = fixture.Create<ProducerBillingInstructionsResponseDto>();
            var request = fixture.Create<PaginationRequestViewModel>();
            var currentUser = fixture.Create<string>();
            var isSelectAll = fixture.Create<bool>();
            var isSelectAllPage = fixture.Create<bool>();

            // Act
            var result = _testClass.MapToViewModel(billingData, request, currentUser, isSelectAll, isSelectAllPage);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(isSelectAll, result.OrganisationSelections.SelectAll);
            Assert.AreEqual(isSelectAll, result.OrganisationSelections.SelectAll);
            Assert.AreEqual(billingData.CalculatorRunId, result.CalculationRun.Id);
            Assert.AreEqual(billingData.RunName, result.CalculationRun.Name);
            Assert.AreEqual(billingData.Records.Count, result.TablePaginationModel.Records.Count());
            Assert.IsNotNull(result.TablePaginationModel.Records.Contains("Noaction"));
        }
    }
}