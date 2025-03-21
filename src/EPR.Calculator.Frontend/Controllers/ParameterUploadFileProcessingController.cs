using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    [Authorize(Roles = "SASuperUser")]
    public class ParameterUploadFileProcessingController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

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
        }

        public string? FileName { get; set; }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Index([FromBody] List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            try
            {
                var parameterSettingsApi = this.GetParameterSettingsApi();

                if (string.IsNullOrWhiteSpace(parameterSettingsApi))
                {
                    throw new ArgumentException("ParameterSettingsApi is null. Check the configuration settings for default parameters", nameof(schemeParameterValues));
                }

                this.FileName = this.HttpContext.Session.GetString(SessionConstants.ParameterFileName);

                if (string.IsNullOrWhiteSpace(this.FileName))
                {
                    throw new ArgumentException("FileName is null. Check the session data for ParameterFileName", nameof(schemeParameterValues));
                }

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(parameterSettingsApi);
                var accessToken = await this.AcquireToken();
                client.DefaultRequestHeaders.Add("Authorization", accessToken);

                var payload = this.Transform(schemeParameterValues);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, StaticHelpers.MediaType);

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(parameterSettingsApi));
                request.Content = content;

                var response = client.SendAsync(request);

                response.Wait();

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    return this.Ok(response.Result);
                }

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
                throw new ArgumentException("ParameterSettingsApi is null. Check the configuration settings for default parameters", parameterSettingsApi);
            }

            return parameterSettingsApi;
        }

        private string Transform(List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            var parameterYear = this.configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value;
            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                throw new ArgumentException("ParameterYear is null. Check the configuration settings for default parameters", parameterYear);
            }

            var fileName = this.FileName;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("FileName is null. Check the session data for ParameterFileName", nameof(schemeParameterValues));
            }

            var parameterSetting = new CreateDefaultParameterSettingDto
            {
                ParameterYear = parameterYear,
                SchemeParameterTemplateValues = schemeParameterValues,
                ParameterFileName = this.FileName,
            };

            return JsonConvert.SerializeObject(parameterSetting);
        }
    }
}