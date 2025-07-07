using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

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

        private CalculationRunOrganisationBillingInstructionsDto billingData { get; set; }

        [HttpGet("BillingInstructions/{calculationRunId}", Name="BillingInstructions_Index")]
        public IActionResult Index([FromRoute] int calculationRunId, [FromQuery] PaginationRequestViewModel request)
        {
            if (calculationRunId <= 0)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, CommonUtil.GetControllerName(typeof(StandardErrorController)));
            }

            this.billingData = GetBillingData(calculationRunId);
            
           // var selectAllPage = this.HttpContext.Session.GetString(SessionConstants.BillingInstructionsSelectAllPage);

           // this.billingData = !(string.IsNullOrEmpty(t) || t == "null") ? JsonConvert.DeserializeObject<CalculationRunOrganisationBillingInstructionsDto>(selectAllPage) : GetBillingData(calculationRunId);

            var viewModel = this.MapToViewModel(this.billingData, request);

            return this.View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessSelection(int calculationRunId, [FromForm] OrganisationSelectionsViewModel selections)
        {
            return this.RedirectToAction("Index", new { calculationRunId });
        }

        [HttpPost]
        public IActionResult SelectAllPage([FromForm] BillingInstructionsViewModel vm, [FromQuery] PaginationRequestViewModel request)
        {
            var selectAllPage = this.HttpContext.Session.GetString(SessionConstants.BillingInstructionsSelectAllPage);

            this.billingData = !(string.IsNullOrEmpty(selectAllPage) || selectAllPage == "null") ? JsonConvert.DeserializeObject<CalculationRunOrganisationBillingInstructionsDto>(selectAllPage) : GetBillingData(vm.CalculationRun.Id);

            var pageSize = this.Request.QueryString;

            if (this.billingData.Organisations.Any())
            {
                this.billingData.Organisations.Skip(request.Page * request.PageSize).Take(request.PageSize).Where(t => t.Status != BillingStatus.Noaction).All(t => t.IsSelected = vm.IsSelectAllPage);
            }

            var k = JsonConvert.SerializeObject(this.billingData);
            this.HttpContext.Session.SetString(SessionConstants.BillingInstructionsSelectAllPage, k);

            var viewModel = this.MapToViewModel(billingData, request);

            return RedirectToRoute("BillingInstructions_Index", new { calculationRunId = vm.CalculationRun.Id, pageSize = request.PageSize, page = request.Page });
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
                    RouteValues = new Dictionary<string, object?>
                    {
                         { "calculationRunId", billingData.CalculationRun.Id },
                    },
                },
            };
            return viewModel;
        }
    }
}
