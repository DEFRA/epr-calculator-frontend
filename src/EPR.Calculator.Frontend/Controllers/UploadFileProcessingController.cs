using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileProcessingController : Controller
    {
        private readonly IConfiguration _configuration;

        public UploadFileProcessingController(IConfiguration configuration) {
            _configuration = configuration;
        }

        [HttpPost]
        public IActionResult Index([FromBody]List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            using (var client = new HttpClient())
            {
                var parameterSettingsApi = _configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

                client.BaseAddress = new Uri(parameterSettingsApi);

                var payload = Transform(schemeParameterValues);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync(parameterSettingsApi, content);

                response.Wait();

                if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
                {
                    return Ok(response.Result);
                }

                return BadRequest(response.Result);
            }
        }

        private string Transform(List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            var parameterSetting = new CreateDefaultParameterSettingDto
            {
                ParameterYear = _configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value,
                SchemeParameterTemplateValues = schemeParameterValues,
            };

            return JsonConvert.SerializeObject(parameterSetting);
        }
    }
}
