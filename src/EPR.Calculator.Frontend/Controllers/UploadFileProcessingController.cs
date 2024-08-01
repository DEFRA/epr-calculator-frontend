using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileProcessingController : Controller
    {
        [HttpPost]
        public IActionResult Index([FromBody]List<SchemeParameterTemplateValue> schemeParameterValues)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://devrwdwebwab422.azurewebsites.net/api/defaultParameterSetting");

                var payload = Transform(schemeParameterValues);

                var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync("https://devrwdwebwab422.azurewebsites.net/api/defaultParameterSetting", content);

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
                ParameterYear = "2025",
                SchemeParameterTemplateValues = schemeParameterValues,
            };

            return JsonConvert.SerializeObject(parameterSetting);
        }
    }
}
