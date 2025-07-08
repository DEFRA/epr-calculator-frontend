using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Text.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Organisation Controller.
    /// </summary>
    public class BillingInstructionsController : BaseController
    {
        /// <summary>
        /// Default Selections.
        /// </summary>
        private static readonly OrganisationSelectionsViewModel DefaultSelections = new()
        {
            SelectAll = false,
            SelectPage = false,
            SelectedOrganisationIds = [],
        };

        public BillingInstructionsController(IConfiguration configuration, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient, IHttpClientFactory clientFactory)
            : base(configuration, tokenAcquisition, telemetryClient, clientFactory)
        {
        }

        /// <summary>
        /// Handles the HTTP GET request to display billing instructions for the specified calculation run.
        /// </summary>
        /// <param name="calculationRunId">The ID of the calculation run to retrieve billing data for.</param>
        /// <param name="request">The pagination and filtering parameters.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the billing instructions view,
        /// or redirects to a standard error page if the calculation run ID is invalid.
        /// </returns>
        [HttpGet("BillingInstructions/{calculationRunId}", Name = "BillingInstructions_Index")]
        public IActionResult Index([FromRoute] int calculationRunId, [FromQuery] PaginationRequestViewModel request)
        {
            if (calculationRunId <= 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            var organisationSelectionsViewModel = this.GetSelectedOrganisationsFromSession();

            var viewModel = this.BuildViewModel(calculationRunId, request, organisationSelectionsViewModel.SelectAll, organisationSelectionsViewModel.SelectPage);

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
        public IActionResult SelectAll(BillingInstructionsViewModel model, int pageSize, int currentPage)
        {
            var viewModel = this.BuildViewModel(model.CalculationRun.Id, new PaginationRequestViewModel() { Page = currentPage, PageSize = pageSize }, model.OrganisationSelections.SelectAll, model.OrganisationSelections.SelectPage);

            return this.View("Index", viewModel);
        }

        private static OrganisationSelectionsViewModel CreateSelectionsAndMarkOrganisations(CalculationRunOrganisationBillingInstructionsDto billingData)
        {
            var selections = new OrganisationSelectionsViewModel
            {
                SelectAll = true,
            };

            foreach (var organisation in billingData.Organisations.Where(o => o.Status != BillingStatus.Noaction))
            {
                organisation.IsSelected = true;
                selections.SelectedOrganisationIds.Add(organisation.Id);
            }

            return selections;
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
            PaginationRequestViewModel request,
            OrganisationSelectionsViewModel organisationSelectionsViewModel)
        {
            var pagedOrganisations = billingData.Organisations
               .Skip((request.Page - 1) * request.PageSize)
               .Take(request.PageSize)
               .ToList();

            var isSelectAll = !billingData.Organisations.Where(t => t.Status != BillingStatus.Noaction).Any(s => !s.IsSelected);

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
                OrganisationSelections = organisationSelectionsViewModel,
            };
            return viewModel;
        }

        /// <summary>
        /// Central method to build the view model and optionally apply SelectAll logic.
        /// </summary>
        private BillingInstructionsViewModel BuildViewModel(
            int calculationRunId,
            PaginationRequestViewModel request,
            bool selectAll, bool selectAllonPage)
        {
            var billingData = GetBillingData(calculationRunId);
            var selectedOrganisation = new OrganisationSelectionsViewModel();
            if (selectAll)
            {
                selectedOrganisation = CreateSelectionsAndMarkOrganisations(billingData);
                this.HttpContext.Session.SetString(SessionConstants.SelectedOrganisationIds, JsonSerializer.Serialize(selectedOrganisation));
            }

            if (selectAllonPage)
            {
                billingData.Organisations.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Where(t => t.Status != BillingStatus.Noaction).All(t => t.IsSelected = selectAllonPage);
                selectedOrganisation.SelectedOrganisationIds.AddRange(billingData.Organisations.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).Where(t => t.Status != BillingStatus.Noaction).Select(t => t.OrganisationId));
                selectedOrganisation.SelectPage = selectAllonPage;
            }

            var viewModel = this.MapToViewModel(billingData, request, selectedOrganisation);

            return viewModel;
        }

        /// <summary>
        /// Helper to read SelectAll state from the session.
        /// </summary>
        private OrganisationSelectionsViewModel GetSelectedOrganisationsFromSession()
        {
            var selectedOrganisationIds = this.HttpContext.Session.GetString(SessionConstants.SelectedOrganisationIds);
            if (string.IsNullOrEmpty(selectedOrganisationIds))
            {
                return DefaultSelections;
            }

            return JsonSerializer.Deserialize<OrganisationSelectionsViewModel>(selectedOrganisationIds) ?? DefaultSelections;
        }
    }
}
