using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers
{
    [TestClass]
    public class OrganisationControllerTests
    {
        private OrganisationController _controller;

        [TestInitialize]
        public void Setup()
        {
            var configMock = new Mock<IConfiguration>();
            var tokenAcquisitionMock = new Mock<ITokenAcquisition>();
            var telemetryMock = new TelemetryClient();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();

            _controller = new OrganisationController(
                configMock.Object,
                tokenAcquisitionMock.Object,
                telemetryMock,
                httpClientFactoryMock.Object);
        }

        [TestMethod]
        public void Index_ReturnsViewResult_WithCorrectPagination()
        {
            PaginationRequestViewModel viewModelRequest = new PaginationRequestViewModel() { Page = 2, PageSize = 10 };

            // Act
            var result = _controller.Index(viewModelRequest) as ViewResult;

            // Assert
            Assert.IsNotNull(result, "Expected a ViewResult");
            var model = result.Model as PaginatedTableViewModel;
            Assert.IsNotNull(model, "Expected model to be PaginatedTableViewModel");

            Assert.AreEqual(2, model.CurrentPage);
            Assert.AreEqual(10, model.PageSize);
            Assert.AreEqual(100, model.TotalRecords);
            Assert.AreEqual("Billing Instructions", model.Caption);

            // Verify pagination: should have 10 records, corresponding to items 11-20
            Assert.AreEqual(10, model.Records.Count());

            var firstOrg = model.Records.First() as Organisation;
            Assert.IsNotNull(firstOrg);
            Assert.AreEqual(11, firstOrg.Id);
        }

        [TestMethod]
        public void ProcessSelection_PostRedirectsToIndex()
        {
            // Arrange
            var selectedIds = new List<int> { 1, 2, 3 };

            // Act
            var result = _controller.ProcessSelection(selectedIds) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(ActionNames.Index, result.ActionName);
        }
    }
}
