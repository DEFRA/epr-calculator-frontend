using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Drawing.Printing;
using System.Reflection;
using System.Security.Cryptography;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Organisation Controller.
    /// </summary>
    public class BillingInstructionsController : BaseController
    {
        public BillingInstructionsController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient, IHttpClientFactory clientFactory)
            : base(configuration, tokenAcquisition, telemetryClient, clientFactory)
        {
        }

        [HttpGet("BillingInstructions/{calculationRunId}", Name = "BillingInstructions_Index")]
        public IActionResult Index([FromRoute] int calculationRunId, [FromQuery] PaginationRequestViewModel request)
        {
            if (calculationRunId <= 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var billingData = GetBillingData(calculationRunId);

            var viewModel = MapToViewModel(billingData, request);

            return this.View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessSelection(int calculationRunId, [FromForm] OrganisationSelectionsViewModel selections)
        {
            return RedirectToAction("Index", new { calculationRunId });
        }

        private CalculationRunOrganisationBillingInstructionsDTO GetBillingData(int calculationRunId)
        {
            return new CalculationRunOrganisationBillingInstructionsDTO
            {
                Organisations = Enumerable.Range(1, 100).Select(i => new Organisation
                {
                    Id = i,
                    OrganisationName = $"Acme org Ltd {i}",
                    OrganisationId = 215148 + i,
                    BillingInstruction = (BillingInstruction)(i % 5),
                    InvoiceAmount = 10000.00 + (i * 20),
                    Status = (BillingStatus)(i % 4),
                }).ToList(),
                CalculationRun = new CalculationRunForBillingInstructionsDTO { Id = calculationRunId, Name = $"Calculation run {calculationRunId}" },
            };
        }

        private BillingInstructionsViewModel MapToViewModel(
            CalculationRunOrganisationBillingInstructionsDTO billingData,
            PaginationRequestViewModel request)
        {
            var pagedOrganisations = billingData.Organisations
               .Skip((request.Page - 1) * request.PageSize)
               .Take(request.PageSize)
               .ToList();

            var viewModel = new BillingInstructionsViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRun = billingData.CalculationRun,
                TablePaginationModel = new PaginationViewModel
                {
                    Records = pagedOrganisations,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = billingData.Organisations.Count,
                    RouteName = "BillingInstructions_Index",
                    RouteValues = new Dictionary<string, object>
                    {
                         { "calculationRunId", billingData.CalculationRun.Id },
                    },
                },
            };
            return viewModel;
        }
    }
}
