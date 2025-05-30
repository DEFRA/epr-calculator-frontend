﻿using System.Net;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterUploadFileProcessingController"/> class.
    /// </summary>
    /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
    /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
    /// <param name="tokenAcquisition">The token acquisition service.</param>
    /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
    public class ParameterUploadFileProcessingController(IConfiguration configuration,
        IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition,
        TelemetryClient telemetryClient)
        : BaseController(configuration, tokenAcquisition, telemetryClient, clientFactory)
    {
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] ParameterRefreshViewModel parameterRefreshViewModel)
        {
            try
            {
                var response = await this.PostDefaultParametersAsync(
                    new CreateDefaultParameterSettingDto(
                        parameterRefreshViewModel,
                        this.GetFinancialYear()));

                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Created)
                {
                    return this.Ok(response);
                }

                this.TelemetryClient.TrackTrace($"2.File name before BadRequest :{parameterRefreshViewModel.FileName}");
                this.TelemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Content.ReadAsStringAsync().Result}");
                return this.BadRequest(response.Content.ReadAsStringAsync().Result);
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
        protected async Task<HttpResponseMessage> PostDefaultParametersAsync(CreateDefaultParameterSettingDto dto)
        {
            var apiUrl = this.GetApiUrl(
                ConfigSection.ParameterSettings,
                ConfigSection.DefaultParameterSettingsApi);
            return await this.CallApi(HttpMethod.Post, apiUrl, string.Empty, dto);
        }
    }
}