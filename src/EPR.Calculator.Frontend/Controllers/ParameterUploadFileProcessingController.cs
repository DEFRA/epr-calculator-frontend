using System.Net;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    public class ParameterUploadFileProcessingController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly TelemetryClient _telemetryClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterUploadFileProcessingController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public ParameterUploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this._telemetryClient = telemetryClient;
        }

        public string? FileName { get; set; }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] ParameterRefreshViewModel parameterRefreshViewModel)
        {
            try
            {
                var parameterSettingsApi = this.GetParameterSettingsApi();
                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(parameterSettingsApi);
                var accessToken = await this.AcquireToken();
                client.DefaultRequestHeaders.Add("Authorization", accessToken);

                this._telemetryClient.TrackTrace($"1.Parameter File Name before Transform :{parameterRefreshViewModel.FileName}");

                var payload = this.Transform(parameterRefreshViewModel);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, StaticHelpers.MediaType);

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(parameterSettingsApi));
                request.Content = content;

                var response = client.SendAsync(request);

                response.Wait();

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    return this.Ok(response.Result);
                }

                this._telemetryClient.TrackTrace($"2.File name before BadRequest :{parameterRefreshViewModel.FileName}");
                this._telemetryClient.TrackTrace($"3.Reason for BadRequest :{response.Result.Content.ReadAsStringAsync().Result}");
                return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private string? GetParameterSettingsApi()
        {
            var parameterSettingsApi = this.configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

            if (string.IsNullOrWhiteSpace(parameterSettingsApi))
            {
                throw new ArgumentNullException(parameterSettingsApi, "ParameterSettingsApi is null. Check the configuration settings for default parameters");
            }

            return parameterSettingsApi;
        }

        private string Transform(ParameterRefreshViewModel parameterRefreshViewModel)
        {
            var parameterYear = this.GetFinancialYear(ConfigSection.ParameterSettings);

            var parameterSetting = new CreateDefaultParameterSettingDto
            {
                ParameterYear = parameterYear,
                SchemeParameterTemplateValues = parameterRefreshViewModel.ParameterTemplateValue,
                ParameterFileName = parameterRefreshViewModel.FileName,
            };

            this._telemetryClient.TrackTrace($"4.File Name in Transform :{parameterRefreshViewModel.FileName}");
            return JsonConvert.SerializeObject(parameterSetting);
        }
    }
}