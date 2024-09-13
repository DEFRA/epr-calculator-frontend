using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileProcessingController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        public UploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        [HttpPost]
        public IActionResult Index([FromBody] List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            try
            {
                var parameterSettingsApi = this.configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

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
            catch (Exception ex)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private string Transform(List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            var parameterSetting = new CreateDefaultParameterSettingDto
            {
                ParameterYear = this.configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value,
                SchemeParameterTemplateValues = schemeParameterValues,
            };

            return JsonConvert.SerializeObject(parameterSetting);
        }
    }
}