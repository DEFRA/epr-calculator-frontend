using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class ParameterUploadFileProcessingController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        public ParameterUploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        public string FileName { get; set; }

        [HttpPost]
        public IActionResult Index([FromBody] List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            try
            {
                var parameterSettingsApi = this.configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

                if (string.IsNullOrWhiteSpace(parameterSettingsApi))
                {
                    throw new ArgumentNullException(parameterSettingsApi, "ParameterSettingsApi is null. Check the configuration settings for default parameters");
                }

                this.FileName = this.HttpContext.Session.GetString("FileName");

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(parameterSettingsApi);

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

        private string Transform(List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            var parameterYear = this.configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value;
            if (string.IsNullOrWhiteSpace(parameterYear))
            {
                throw new ArgumentNullException(parameterYear, "ParameterYear is null. Check the configuration settings for default parameters");
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