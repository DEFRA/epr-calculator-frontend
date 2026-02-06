using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseController"/> class.
    /// </summary>
    /// <param name="clientFactory">an HTTP client factory that will be used for making connections to the calculator API.</param>
    public class BaseController(
        IConfiguration configuration,
        TelemetryClient telemetryClient,
        IApiService apiService,
        ICalculatorRunDetailsService calculatorRunDetailsService) : Controller
    {
        protected IApiService ApiService { get; init; } = apiService;

        protected ICalculatorRunDetailsService CalculatorRunDetailsService { get; init; }
            = calculatorRunDetailsService;

        protected TelemetryClient TelemetryClient { get; init; } = telemetryClient;

        /// <summary>Gets the configuration object to retrieve API URL and parameters.</summary>
        protected IConfiguration Configuration { get; init; } = configuration;

        protected string GetBackLink()
        {
            var referrer = this.Request.Headers.Referer.ToString();

            if (string.IsNullOrEmpty(referrer))
            {
                return string.Empty;
            }

            try
            {
                var uri = new Uri(referrer);
                var absolutePath = uri.AbsolutePath;
                if (absolutePath == "/")
                {
                    return ControllerNames.Dashboard;
                }

                var segments = absolutePath.TrimEnd('/').Split('/');

                if (segments.Length >= 2)
                {
                    return segments[^2];
                }
            }
            catch (UriFormatException)
            {
                return string.Empty;
            }

            return string.Empty;
        }
    }
}