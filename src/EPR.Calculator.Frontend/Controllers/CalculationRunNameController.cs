using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<CalculationRunNameController> logger;

        public CalculationRunNameController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunNameController> logger)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        [HttpPost]
        public IActionResult RunCalculator(InitiateCalculatorRunModel calculationRunModel)
        {
            if (!this.ModelState.IsValid)
            {
                var errorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                this.ViewBag.Errors = CreateErrorViewModel(errorMessages.First());
                return this.View(CalculationRunNameIndexView);
            }

            if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
            {
                this.HttpContext.Session.SetString(SessionConstants.CalculationName, calculationRunModel.CalculationName);
            }

            return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
        }

        public async Task<IActionResult> Confirmation()
        {
            bool confirmationShown = false;
            try
            {
                string runName = this.HttpContext.Session.GetString(SessionConstants.CalculationName) ?? string.Empty;

                var response = await this.GetHttpRequestAsync(runName);

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Accepted)
                {
                    confirmationShown = true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error during confirmation process.");
            }

            if (confirmationShown)
            {
                return this.View(ViewNames.CalculationRunConfirmation);
            }
            else
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private static ErrorViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.CalculationRunName,
                ErrorMessage = errorMessage,
            };
        }

        private async Task<HttpResponseMessage> GetHttpRequestAsync(string calculatorRunName)
        {
            var calculatorRunApi = this.configuration[$"{ConfigSection.CalculatorRun}:{ConfigSection.CalculatorRunApi}"];

            if (string.IsNullOrEmpty(calculatorRunApi))
            {
                throw new ArgumentNullException(nameof(calculatorRunApi), "The API URL cannot be null or empty.");
            }

            var year = this.configuration[$"{ConfigSection.CalculatorRun}:{ConfigSection.RunParameterYear}"] ?? string.Empty;

            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(calculatorRunApi);

            var runParms = new CreateCalculatorRunDto
            {
                CalculatorRunName = calculatorRunName,
                FinancialYear = year,
                CreatedBy = "Test User",
            };

            var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, StaticHelpers.MediaType);
            var request = new HttpRequestMessage(HttpMethod.Post, calculatorRunApi) { Content = content };

            return await client.SendAsync(request);
        }
    }
}