using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        public DashboardController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            List<CalculationRun> dashboards = new List<CalculationRun>();
            var dashboardRunData = new List<DashboardViewModel>();
            var parameterSettingsApi = this.configuration.GetSection("DashboardCalculatorRun").GetSection("DashboardCalculatorRunApi").Value;
            var year = this.configuration.GetSection("DashboardCalculatorRun").GetSection("RunParameterYear").Value;
            var client = this.clientFactory.CreateClient();
            client.BaseAddress = new Uri(parameterSettingsApi);
            var runParms = new CalculatorRunParamsDto
            {
                FinancialYear = year,
            };


            var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(parameterSettingsApi));
            request.Content = content;

            var response = client.SendAsync(request);

            response.Wait();

            if (response.Result.IsSuccessStatusCode)
            {
                dashboards = JsonConvert.DeserializeObject<List<CalculationRun>>(response.Result.Content.ReadAsStringAsync().Result);

                if (dashboards.Count > 0)
                {
                    foreach (var calculationRun in dashboards)
                    {
                        dashboardRunData.Add(new DashboardViewModel(calculationRun));
                    }
                }

                return this.View(ViewNames.DashboardIndex, dashboardRunData);
            }

            return BadRequest(response.Result.Content.ReadAsStringAsync().Result);
        }
    }
}