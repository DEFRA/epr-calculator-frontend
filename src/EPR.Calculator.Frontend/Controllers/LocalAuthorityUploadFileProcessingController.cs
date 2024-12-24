using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalAuthorityUploadFileProcessingController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration object to retrieve API URL and parameters.</param>
        /// <param name="clientFactory">The HTTP client factory to create an HTTP client.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public LocalAuthorityUploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.clientFactory = clientFactory;
        }

        public string? FileName { get; set; }

        [HttpPost]
        [Authorize(Roles = "SASuperUser")]
        public async Task<IActionResult> Index([FromBody] List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            try
            {
                var lapcapSettingsApi = this.Configuration.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value;

                if (string.IsNullOrWhiteSpace(lapcapSettingsApi))
                {
                    throw new ArgumentNullException(lapcapSettingsApi, "LapcapSettingsApi is null. Check the configuration settings for local authority");
                }

                this.FileName = this.HttpContext.Session.GetString(SessionConstants.LapcapFileName);

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(lapcapSettingsApi);
                var accessToken = await this.AcquireToken();
                client.DefaultRequestHeaders.Add("Authorization", accessToken);

                var payload = this.Transform(lapcapDataTemplateValues);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, StaticHelpers.MediaType);

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(lapcapSettingsApi));
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

        private string Transform(List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            var parameterYear = this.Configuration.GetSection("LapcapSettings").GetSection("ParameterYear").Value;
            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                throw new ArgumentNullException(parameterYear, "ParameterYear is null. Check the configuration settings for local authority");
            }

            var lapcapData = new CreateLapcapDataDto
            {
                ParameterYear = parameterYear,
                LapcapDataTemplateValues = lapcapDataTemplateValues,
                LapcapFileName = this.FileName,
            };

            return JsonConvert.SerializeObject(lapcapData);
        }
    }
}