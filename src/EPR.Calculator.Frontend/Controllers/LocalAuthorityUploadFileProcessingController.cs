using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityUploadFileProcessingController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;
        private readonly TelemetryClient _telemetryClient;

        public LocalAuthorityUploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory, TelemetryClient telemetryClient)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
            this._telemetryClient = telemetryClient;
        }

        public string FileName { get; set; }

        [HttpPost]
        public IActionResult Index([FromBody] List<LapcapDataTemplateValueDto> lapcapDataTemplateValues, [FromQuery] FileNameViewModel model)
        {
            try
            {
                this._telemetryClient.TrackTrace("Index action started - Preparing to send data.");

                var lapcapSettingsApi = this.configuration.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value;

                if (string.IsNullOrWhiteSpace(lapcapSettingsApi))
                {
                    throw new ArgumentNullException(lapcapSettingsApi, "LapcapSettingsApi is null. Check the configuration settings for local authority");
                }

                this._telemetryClient.TrackEvent("1.Config settings", new Dictionary<string, string>
                {
                    { "lapcapSettingsApi", lapcapSettingsApi },
                    { "FileName", this.FileName },
                });

                this.FileName = this.HttpContext.Session.GetString(SessionConstants.LapcapFileName);
                this._telemetryClient.TrackTrace($"Retrieving Session data.{this.HttpContext.Session.GetString(SessionConstants.LapcapFileName)}");

                this.FileName = model.FileName;
                this._telemetryClient.TrackTrace($"Retrieving Session data from Query.{model.FileName}");

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(lapcapSettingsApi);

                this._telemetryClient.TrackEvent("2.HTTP Client Created", new Dictionary<string, string>
                {
                    { "ApiBaseAddress", lapcapSettingsApi },
                    { "FileName", this.FileName },
                });

                var payload = this.Transform(lapcapDataTemplateValues);

                this._telemetryClient.TrackTrace($"Payload created: {payload.Length} characters.");

                var content = new StringContent(payload, System.Text.Encoding.UTF8, StaticHelpers.MediaType);

                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(lapcapSettingsApi));
                request.Content = content;

                var response = client.SendAsync(request);

                response.Wait();

                var responseResult = response.Result;

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    this._telemetryClient.TrackEvent("3.Data Successfully Uploaded", new Dictionary<string, string>
                    {
                        { "StatusCode", responseResult.StatusCode.ToString() },
                        { "FileName", this.FileName },
                    });

                    return this.Ok(response.Result);
                }

                var errorMessage = responseResult.Content.ReadAsStringAsync().Result;
                this._telemetryClient.TrackEvent("4.Data Upload Failed", new Dictionary<string, string>
                {
                    { "StatusCode", responseResult.StatusCode.ToString() },
                    { "ErrorMessage", errorMessage },
                });

                return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private string Transform(List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            var parameterYear = this.configuration.GetSection("LapcapSettings").GetSection("ParameterYear").Value;
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