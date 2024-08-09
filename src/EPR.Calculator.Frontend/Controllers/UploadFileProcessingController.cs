using System.Net;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileProcessingController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public UploadFileProcessingController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        [HttpPost]
        public IActionResult Index([FromBody] List<SchemeParameterTemplateValue> schemeParameterValues)
        {

            var parameterSettingsApi = _configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

            var client = _clientFactory.CreateClient();
            client.BaseAddress = new Uri(parameterSettingsApi);

            var payload = Transform(schemeParameterValues);

            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(parameterSettingsApi));
            request.Content = content;

            var response = client.SendAsync(request);

            response.Wait();

            if (response.Result.IsSuccessStatusCode && response.Result.StatusCode == HttpStatusCode.Created)
            {
                return Ok(response.Result);
            }
            return BadRequest(response.Result.Content.ReadAsStringAsync().Result);
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