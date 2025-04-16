using System.Net;
using EPR.Calculator.Frontend.Common;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
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
        IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] LapcapRefreshViewModel lapcapRefreshViewModel)
        {
            try
            {
                var response = this.PostLapcapDataAsync(new CreateLapcapDataDto(
                    lapcapRefreshViewModel,
                    this.GetFinancialYear("LapcapSettings")));

                response.Wait();

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    return this.Ok(response.Result);
                }

                this.TelemetryClient.TrackTrace($"2.File name before BadRequest :{lapcapRefreshViewModel.FileName}");
                this.TelemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Result.Content.ReadAsStringAsync().Result}");
                return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        /// <summary>
        /// Calls the "postDefaultParameterSettings" POST endpoint.
        /// </summary>
        /// <param name="dto">The data transfer object to serialise and use as the body of the request.</param>
        /// <returns>The response message returned by the endpoint.</returns>
        private async Task<HttpResponseMessage> PostLapcapDataAsync(CreateLapcapDataDto dto)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.LapcapSettings,
                ConfigSection.LapcapSettingsApi);
            return await this.CallApi(HttpMethod.Post, apiUrl, string.Empty, dto);
        }
    }
}