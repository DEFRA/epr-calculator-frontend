using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Drawing.Printing;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Organisation Controller.
    /// </summary>
    [Route("[controller]")]
    public class OrganisationController : BaseController
    {
        public OrganisationController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient, IHttpClientFactory clientFactory)
            : base(configuration, tokenAcquisition, telemetryClient, clientFactory)
        {
        }

        public IActionResult Index(PaginationRequestViewModel request)
        {
            // Get the List of Organisation data
            var allOrgs = this.GetOrganisations();

            var pagedOrgs = allOrgs.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();

            var viewModel = new PaginatedTableViewModel
            {
                Records = pagedOrgs.Cast<object>(),
                Caption = "Billing Instructions",
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalRecords = allOrgs.Count,
                StartRecord = ((request.Page - 1) * request.PageSize) + 1,
                EndRecord = Math.Min(request.Page * request.PageSize, allOrgs.Count),
            };

            return this.View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessSelection(List<int> selectedIds)
        {
            return this.RedirectToAction("Index");
        }

        private List<Organisation> GetOrganisations() =>
     Enumerable.Range(1, 100).Select(i => new Organisation
     {
         Id = i,
         OrganisationName = $"Acme org Ltd {i}",
         OrganisationId = 215148 + i,
         BillingInstruction = (BillingInstruction)(i % 5),
         InvoiceAmount = 10000.00 + (i * 20),
         Status = (BillingStatus)(i % 4),
     }).ToList();
    }
}
