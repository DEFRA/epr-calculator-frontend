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
        public IActionResult Index([FromBody] List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            try
            {
                this._telemetryClient.TrackTrace("Index action started - Preparing to send data.");

                var lapcapSettingsApi = this.configuration.GetSection("LapcapSettings").GetSection("LapcapSettingsApi").Value;

                if (string.IsNullOrWhiteSpace(lapcapSettingsApi))
                {
                    throw new ArgumentNullException(lapcapSettingsApi, "LapcapSettingsApi is null. Check the configuration settings for local authority");
                }

                this._telemetryClient.TrackEvent("5.Config settings", new Dictionary<string, string>
                {
                    { "lapcapSettingsApi", lapcapSettingsApi },
                    { "FileName", this.FileName },
                });

                this._telemetryClient.TrackTrace($"LapcapSettingsApi retrieved from configuration.{lapcapSettingsApi}"); // last trace

                this._telemetryClient.TrackTrace($"Retrieving TempData.{this.TempData["LapcapFileName"].ToString()}");

                this._telemetryClient.TrackEvent("6.TempData", new Dictionary<string, string>
                {
                    { "lapcapSettingsApi", lapcapSettingsApi },
                    { "Tempdata", this.TempData["LapcapFileName"].ToString() },
                });

                this.FileName = this.TempData["LapcapFileName"].ToString();
                this._telemetryClient.TrackTrace(FileName);
                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(lapcapSettingsApi);

                this._telemetryClient.TrackTrace(client.BaseAddress.ToString());

                this._telemetryClient.TrackEvent("7.HTTP Client Created", new Dictionary<string, string>
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

                if (responseResult.IsSuccessStatusCode && responseResult.StatusCode == HttpStatusCode.Created)
                {
                    this._telemetryClient.TrackEvent("Data Successfully Uploaded", new Dictionary<string, string>
                    {
                        { "StatusCode", responseResult.StatusCode.ToString() },
                        { "FileName", this.FileName },
                    });

                    return this.Ok(response);
                }

                var errorMessage = responseResult.Content.ReadAsStringAsync().Result;
                this._telemetryClient.TrackEvent("Data Upload Failed", new Dictionary<string, string>
                {
                    { "StatusCode", responseResult.StatusCode.ToString() },
                    { "ErrorMessage", errorMessage },
                });

                return this.BadRequest(response.Result.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                this._telemetryClient.TrackException(ex, new Dictionary<string, string>
                {
                    { "Action", "Index" },
                    { "FileName", this.FileName ?? "N/A" },
                });
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private string Transform(List<LapcapDataTemplateValueDto> lapcapDataTemplateValues)
        {
            this._telemetryClient.TrackTrace("Transform method started - Preparing to serialize data.");
            var parameterYear = this.configuration.GetSection("LapcapSettings").GetSection("ParameterYear").Value;

            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                this._telemetryClient.TrackException(new ArgumentNullException(parameterYear, "ParameterYear is null. Check the configuration settings."));
                throw new ArgumentNullException(parameterYear, "ParameterYear is null. Check the configuration settings.");
            }

            this._telemetryClient.TrackTrace($"ParameterYear retrieved: {parameterYear}.");

            var lapcapData = new CreateLapcapDataDto
            {
                ParameterYear = parameterYear,
                LapcapDataTemplateValues = lapcapDataTemplateValues,
                LapcapFileName = this.FileName,
            };

            var serializedData = JsonConvert.SerializeObject(lapcapData);

            this._telemetryClient.TrackEvent("Data Serialized", new Dictionary<string, string>
                {
                    { "SerializedDataLength", serializedData.Length.ToString() },
                    { "ParameterYear", parameterYear },
                });

            return serializedData;
        }
    }
}