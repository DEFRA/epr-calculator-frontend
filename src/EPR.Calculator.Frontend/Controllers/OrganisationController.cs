using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

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

        public IActionResult Index(int page = 1, int pageSize = 10)
        {
            var allOrgs = this.GetOrganisations();
            var pagedOrgs = allOrgs.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new PaginatedTableViewModel
            {
                Records = pagedOrgs.Cast<object>(),
                Caption = "Billing Instructions",
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = allOrgs.Count,
            };

            return this.View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessSelection(List<int> selectedIds)
        {
            return this.RedirectToAction("Index");
        }

        private List<Organisation> GetOrganisations() =>
     Enumerable.Range(1, 1000).Select(i => new Organisation
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
