namespace EPR.Calculator.Frontend.Controllers
{
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization;
    using EPR.Calculator.Frontend.Constants;
    using EPR.Calculator.Frontend.Models;
    using EPR.Calculator.Frontend.ViewModels;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    public class DashboardController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardController"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="clientFactory"></param>
        public DashboardController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            try
            {
                var dashboardcalculatorrunApi = this.configuration.GetSection("DashboardCalculatorRun").GetSection("DashboardCalculatorRunApi").Value;
                var year = this.configuration.GetSection("DashboardCalculatorRun").GetSection("RunParameterYear").Value;
                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(dashboardcalculatorrunApi);
                var request = new HttpRequestMessage(HttpMethod.Post, new Uri(dashboardcalculatorrunApi));

                var runParms = new CalculatorRunParamsDto
                {
                    FinancialYear = year,
                };
                var content = new StringContent(JsonConvert.SerializeObject(runParms), System.Text.Encoding.UTF8, StaticHelpers.MediaType);
                request.Content = content;
                var response = client.SendAsync(request);
                response.Wait();

                var dashboardRunData = new List<DashboardViewModel>();

                if (response.Result.IsSuccessStatusCode)
                {
                    dashboardRunData = this.GetCalulationRunsData(JsonConvert.DeserializeObject<List<CalculationRun>>(response.Result.Content.ReadAsStringAsync().Result));

                    return this.View(ViewNames.DashboardIndex, dashboardRunData);
                }

                if (response.Result.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.View();
                }

                return RedirectToAction(ViewNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ViewNames.StandardErrorIndex, "StandardError");
            }
        }

        private List<DashboardViewModel> GetCalulationRunsData(List<CalculationRun> calculationRuns)
        {
            var runClassifications = Enum.GetValues(typeof(RunClassification)).Cast<RunClassification>().ToList();
            var dashboardRunData = new List<DashboardViewModel>();

            if (calculationRuns.Count > 0)
            {
                foreach (var calculationRun in calculationRuns)
                {
                    var classification_val = runClassifications.FirstOrDefault(c => (int)c == calculationRun.CalculatorRunClassificationId);
                    calculationRun.Status = typeof(RunClassification).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == classification_val.ToString())?.GetCustomAttribute<EnumMemberAttribute>(false)?.Value;

                    dashboardRunData.Add(new DashboardViewModel(calculationRun));
                }
            }

            return dashboardRunData;
        }
    }
}