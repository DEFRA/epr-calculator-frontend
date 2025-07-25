﻿using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Humanizer;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels
{
    [TestClass]
    public class PaginationRequestViewModelTests
    {
        [TestMethod]
        public void PaginationRequestViewModel_DefaultValues_ShouldBeSetCorrectly()
        {
            // Arrange & Act
            var viewModel = new PaginationRequestViewModel();

            // Assert
            Assert.AreEqual(CommonConstants.DefaultPage, viewModel.Page);
            Assert.AreEqual(CommonConstants.DefaultPageSize, viewModel.PageSize);
            Assert.IsNull(viewModel.OrganisationId);
            Assert.IsNull(viewModel.BillingStatus);
            Assert.AreEqual(viewModel.GetType(), typeof(PaginationRequestViewModel));
        }

        [TestMethod]
        public void PaginationRequestViewModel_CustomValues_ShouldBeSetCorrectly()
        {
            // Arrange
            var page = 2;
            var pageSize = 50;
            var organisationId = 123;

            // Act
            var viewModel = new PaginationRequestViewModel
            {
                Page = page,
                PageSize = pageSize,
                OrganisationId = organisationId,
                BillingStatus = BillingStatus.Accepted // Example of setting a billing status
            };

            // Assert
            Assert.AreEqual(page, viewModel.Page);
            Assert.AreEqual(pageSize, viewModel.PageSize);
            Assert.AreEqual(organisationId, viewModel.OrganisationId);
            Assert.AreEqual(BillingStatus.Accepted, viewModel.BillingStatus);
        }
    }
}
