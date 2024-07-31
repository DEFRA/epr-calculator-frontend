using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class UploadFileProcessingController : Controller
    {
        public IActionResult Index(string schemeParameterValues)
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
                    // Success, redirect to the confirmation page
                    return RedirectToAction("Index", "ParameterConfirmation");
                }

                // Failed, redirect to list the error messages with an option to upload again
                return RedirectToAction("Index", "UploadCSVError");
            }
        }

        private string Transform(string schemeParameterValues)
        {
            var parameterSetting = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024",
                SchemeParameterTemplateValues = JsonSerializer.Deserialize<List<SchemeParameterTemplateValue>>(schemeParameterValues),
            };

            return System.Text.Json.JsonSerializer.Serialize(parameterSetting);
        }
    }
}
