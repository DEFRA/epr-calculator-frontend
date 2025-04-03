using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class LocalAuthorityUploadFileProcessingController : BaseController
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAuthorityUploadFileProcessingController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public LocalAuthorityUploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient, clientFactory)
        {
            this.clientFactory = clientFactory;
            this._telemetryClient = telemetryClient;
        }

        public string? FileName { get; set; }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Index([FromBody] LapcapRefreshViewModel lapcapRefreshViewModel)
        {
            try
            {
                var lapcapSettingsApi = this.GetLapcapSettingsApi();
                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(lapcapSettingsApi);
                var accessToken = await this.AcquireToken();
                client.DefaultRequestHeaders.Add("Authorization", accessToken);

                this._telemetryClient.TrackTrace($"1.Lapcap File Name before Transform :{lapcapRefreshViewModel.FileName}");

                var payload = this.Transform(lapcapRefreshViewModel);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, StaticHelpers.MediaType);

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(lapcapSettingsApi));
                request.Content = content;

                var response = client.SendAsync(request);

                response.Wait();

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    return this.Ok(response.Result);
                }

                this._telemetryClient.TrackTrace($"2.File name before BadRequest :{lapcapRefreshViewModel.FileName}");
                this._telemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Result.Content.ReadAsStringAsync().Result}");
                return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private string GetLapcapSettingsApi()
        {
            var lapcapSettingsApi = this.Configuration.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value;

            if (string.IsNullOrWhiteSpace(lapcapSettingsApi))
            {
                throw new ArgumentNullException(lapcapSettingsApi, "LapcapSettingsApi is null. Check the configuration settings for local authority");
            }

            return lapcapSettingsApi;
        }

        private string Transform(LapcapRefreshViewModel lapcapRefreshViewModel)
        {
            var parameterYear = this.Configuration.GetSection("LapcapSettings").GetSection("ParameterYear").Value;
            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                throw new ArgumentNullException(parameterYear, "ParameterYear is null. Check the configuration settings for local authority");
            }

            var lapcapData = new CreateLapcapDataDto
            {
                ParameterYear = parameterYear,
                LapcapDataTemplateValues = lapcapRefreshViewModel.LapcapTemplateValue,
                LapcapFileName = lapcapRefreshViewModel.FileName,
            };

            this._telemetryClient.TrackTrace($"4.File Name in Transform :{lapcapRefreshViewModel.FileName}");

            return JsonConvert.SerializeObject(lapcapData);
        }
    }
}