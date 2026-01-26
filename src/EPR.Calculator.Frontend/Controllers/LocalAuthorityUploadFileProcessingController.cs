using System.Net;
using EPR.Calculator.Frontend.Common;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalAuthorityUploadFileProcessingController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
    /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
    /// <param name="tokenAcquisition">The token acquisition service.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    public class LocalAuthorityUploadFileProcessingController(
        IConfiguration configuration,
        IApiService apiService,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient,
        ICalculatorRunDetailsService calculatorRunDetailsService)
        : BaseController(
            configuration,
            tokenAcquisition,
            telemetryClient,
            apiService,
            calculatorRunDetailsService)
    {
        private readonly int financialMonth = CommonUtil.GetFinancialYearStartingMonth(configuration);

        [HttpPost]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<IActionResult> Index([FromBody] LapcapRefreshViewModel lapcapRefreshViewModel)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            var response = this.PostLapcapDataAsync(new CreateLapcapDataDto(
                lapcapRefreshViewModel,
                CommonUtil.GetFinancialYear(this.HttpContext.Session, this.financialMonth)));

            response.Wait();

            if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
            {
                return this.Ok(response.Result);
            }

            this.TelemetryClient.TrackTrace($"2.File name before BadRequest :{lapcapRefreshViewModel.FileName}");
            this.TelemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Result.Content.ReadAsStringAsync().Result}");
            return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
        }

        /// <summary>
        /// Calls the "postDefaultParameterSettings" POST endpoint.
        /// </summary>
        /// <param name="dto">The data transfer object to serialise and use as the body of the request.</param>
        /// <returns>The response message returned by the endpoint.</returns>
        private async Task<HttpResponseMessage> PostLapcapDataAsync(CreateLapcapDataDto dto)
        {
            var apiUrl = this.ApiService.GetApiUrl(
                ConfigSection.LapcapSettings,
                ConfigSection.LapcapSettingsApi);
            return await this.ApiService.CallApi(
                this.HttpContext,
                HttpMethod.Post,
                apiUrl,
                string.Empty,
                dto);
        }
    }
}