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
        public async Task<JsonResult> RunCalculator()
        {
            var apiUrl = this.configuration
                            .GetSection(ConfigSection.CalculationRunNameSettings)
                            .GetValue<string>(ConfigSection.CalculationRunNameApi);

            var array = new string[] { apiUrl };

            return Json(array);
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