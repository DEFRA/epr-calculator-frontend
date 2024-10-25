using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public CalculationRunNameController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this._configuration = configuration;
            this._clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return this.View(CalculationRunNameIndexView);
        }

        [HttpPost]
        public async Task<IActionResult> RunCalculator(InitiateCalculatorRunModel calculationRunModel)
        {
            if (!this.ModelState.IsValid)
            {
                var errorMessages = this.ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage);
                this.ViewBag.Errors = CreateErrorViewModel(errorMessages.First());
                return this.View(CalculationRunNameIndexView);
            }

            try
            {
                if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
                {
                    var calculationNameExistsResponse = await this.CheckIfCalculationNameExistsAsync(calculationRunModel.CalculationName);
                    if (calculationNameExistsResponse.IsSuccessStatusCode)
                    {
                        this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.CalculationRunNameExists);
                        return this.View(CalculationRunNameIndexView);
                    }

                    this.HttpContext.Session.SetString(SessionConstants.CalculationName, calculationRunModel.CalculationName);
                }

                return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(ex.ToString());
            }
        }

        public ViewResult Confirmation()
        {
            return this.View(ViewNames.CalculationRunConfirmation);
        }

        private static ErrorViewModel CreateErrorViewModel(string errorMessage)
        {
            return new ErrorViewModel
            {
                DOMElementId = ViewControlNames.CalculationRunName,
                ErrorMessage = errorMessage,
            };
        }

        private async Task<HttpResponseMessage> CheckIfCalculationNameExistsAsync(string calculationName)
        {
            var apiUrl = this._configuration
                          .GetSection(ConfigSection.CalculationRunNameSettings)
                          .GetValue<string>(ConfigSection.CalculationRunNameApi);

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ArgumentNullException(apiUrl, "CalculationRunNameApi is null or empty. Please check the configuration settings.");
            }

            var client = this._clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);

            var requestUri = new Uri($"{apiUrl}/{calculationName}", UriKind.Absolute);
            return await client.GetAsync(requestUri);
        }
    }
}