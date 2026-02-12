using System.Net;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterUploadFileProcessingController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
    /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    public class ParameterUploadFileProcessingController(IConfiguration configuration,
        IEprCalculatorApiService eprCalculatorApiService,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            telemetryClient,
            eprCalculatorApiService,
            calculatorRunDetailsService)
    {
        private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] ParameterRefreshViewModel parameterRefreshViewModel)
        {
            var response = await this.PostDefaultParametersAsync(
                new CreateDefaultParameterSettingDto(
                    parameterRefreshViewModel,
                    CommonUtil.GetRelativeYear(this.HttpContext.Session, this.relativeYearStartingMonth)));

            if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Created)
            {
                return this.Ok(response);
            }

            this.TelemetryClient.TrackTrace($"2.File name before BadRequest :{parameterRefreshViewModel.FileName}");
            this.TelemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Content.ReadAsStringAsync().Result}");
            return this.BadRequest(response.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Calls the "postDefaultParameterSettings" POST endpoint.
        /// </summary>
        /// <param name="dto">The data transfer object to serialise and use as the body of the request.</param>
        /// <returns>The response message returned by the endpoint.</returns>
        protected async Task<HttpResponseMessage> PostDefaultParametersAsync(CreateDefaultParameterSettingDto dto)
        {
            return await this.EprCalculatorApiService.CallApi(
                httpContext: this.HttpContext,
                httpMethod: HttpMethod.Post,
                relativePath: "v1/defaultParameterSetting",
                body: dto);
        }
    }
}
