using Azure.Core;
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

            var viewModel = this.MapToViewModel(billingData, request);

            var selectAllStored = this.HttpContext.Session.GetString(SessionConstants.BillingInstructionsSelectAll);

            if (bool.TryParse(selectAllStored, out var isSelectAll) && isSelectAll)
            {
                viewModel.SelectAll = true;
                foreach (var organisation in billingData.Organisations.Where(t => t.Status != BillingStatus.Noaction))
                {
                    organisation.IsSelected = true;
                }
            }

            return this.View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessSelection(int calculationRunId, [FromForm] OrganisationSelectionsViewModel selections)
        {
            return this.RedirectToAction("Index", new { calculationRunId });
        }

        /// <summary>
        /// Handles the POST request to select all billing instructions.
        /// </summary>
        /// <param name="model">The view model containing billing instructions and selection state.</param>
        /// <param name="request">Pagination parameters for the current page of billing instructions.</param>
        /// <returns>An <see cref="ActionResult"/> that renders the updated view or redirects as appropriate.</returns>
        [HttpPost]
        public ActionResult SelectAll(BillingInstructionsViewModel model, int pageSize, int currentPage)
        {
            this.HttpContext.Session.SetString(SessionConstants.BillingInstructionsSelectAll, model.SelectAll.ToString());

            var billingData = GetBillingData(model.CalculationRun.Id);

            if (model.SelectAll)
            {
                foreach (var organisation in billingData.Organisations.Where(t => t.Status != BillingStatus.Noaction))
                {
                    organisation.IsSelected = true;
                }
            }

            if (model.SelectAllOnPage && billingData.Organisations.Any())
            {
                billingData.Organisations.Skip((currentPage - 1) * pageSize).Take(pageSize).Where(t => t.Status != BillingStatus.Noaction).All(t => t.IsSelected = model.SelectAllOnPage);
            }

            var viewModel = this.MapToViewModel(billingData, new PaginationRequestViewModel() { Page = currentPage, PageSize = pageSize });
            return this.View("Index", viewModel);
        }

        private static CalculationRunOrganisationBillingInstructionsDto GetBillingData(int calculationRunId)
        {
            return new CalculationRunOrganisationBillingInstructionsDto
            {
                Organisations = Enumerable.Range(1, 100).Select(i => new Organisation
                {
                    Id = i,
                    OrganisationName = $"Acme org Ltd {i}",
                    OrganisationId = 215148 + i,
                    BillingInstruction = (BillingInstruction)(i % 5),
                    InvoiceAmount = 10000.00 + (i * 20),
                    Status = (BillingStatus)(i % 4),
                    IsSelected = false,
                }).ToList(),
                CalculationRun = new CalculationRunForBillingInstructionsDto { Id = calculationRunId, Name = $"Calculation run {calculationRunId}" },
            };
        }

        private BillingInstructionsViewModel MapToViewModel(
            CalculationRunOrganisationBillingInstructionsDto billingData,
            PaginationRequestViewModel request)
        {
            var pagedOrganisations = billingData.Organisations
               .Skip((request.Page - 1) * request.PageSize)
               .Take(request.PageSize)
               .ToList();

            var isSelectAll = !billingData.Organisations.Where(t => t.Status != BillingStatus.Noaction).Any(s => s.IsSelected == false);

            var viewModel = new BillingInstructionsViewModel
            {
                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                CalculationRun = billingData.CalculationRun,
                OrganisationBillingInstructions = billingData.Organisations,
                TablePaginationModel = new PaginationViewModel
                {
                    Records = pagedOrganisations,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = billingData.Organisations.Count,
                    RouteName = "BillingInstructions_Index",
                    RouteValues = new Dictionary<string, object?>
                    {
                         { "calculationRunId", billingData.CalculationRun.Id },
                    },
                },
                SelectAll = isSelectAll,
            };
            return viewModel;
        }
    }
}
