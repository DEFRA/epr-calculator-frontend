using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class CalculationRunNameController : Controller
    {
        private const string CalculationRunNameIndexView = ViewNames.CalculationRunNameIndex;
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly ILogger<CalculationRunNameController> _logger;

        public CalculationRunNameController(IConfiguration configuration, IHttpClientFactory clientFactory, ILogger<CalculationRunNameController> logger)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            _logger = logger;
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
                this._logger.LogInformation("Run Calculator started");
                if (!string.IsNullOrEmpty(calculationRunModel.CalculationName))
                {
                    var calculationNameExistsResponse = await this.CheckIfCalculationNameExistsAsync(calculationRunModel.CalculationName);
                    if (calculationNameExistsResponse.IsSuccessStatusCode)
                    {
                        this.ViewBag.Errors = CreateErrorViewModel(ErrorMessages.CalculationRunNameExists);
                        return this.View(CalculationRunNameIndexView);
                    }

                    this._logger.LogInformation($"Successfull call to api");
                    this.HttpContext.Session.SetString(SessionConstants.CalculationName, calculationRunModel.CalculationName);
                }

                this._logger.LogInformation($"Run Calculator Success");
                return this.RedirectToAction(ActionNames.RunCalculatorConfirmation);
            }
            catch (Exception ex)
            {
                this._logger.LogError($"Run Calculator Error:{ex}");
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
            var apiUrl = this.configuration
                          .GetSection(ConfigSection.CalculationRunNameSettings)
                          .GetValue<string>(ConfigSection.CalculationRunNameApi);

            if (string.IsNullOrWhiteSpace(apiUrl))
            {
                throw new ArgumentNullException(apiUrl, "CalculationRunNameApi is null or empty. Please check the configuration settings.");
            }

            this._logger.LogInformation($"API Url: {apiUrl}");
            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(apiUrl);

            var requestUri = new Uri($"{apiUrl}/{calculationName}", UriKind.Absolute);
            return await client.GetAsync(requestUri);
        }
    }
}