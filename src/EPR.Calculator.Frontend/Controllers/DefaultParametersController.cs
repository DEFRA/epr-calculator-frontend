using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DefaultParametersController : Controller
    {
        private readonly IConfiguration _configuration;
        private HttpClient _client;

        public DefaultParametersController(IConfiguration configuration, HttpClient client)
        {
            _configuration = configuration;
            _client = client;
        }

        public async Task<IActionResult> Index()
        {
            var parameterSettingsApi = _configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

            _client.BaseAddress = new Uri(parameterSettingsApi);
            var year = _configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value;
            var uri = new Uri(string.Format("{0}/{1}", parameterSettingsApi, year));
            var response = await _client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var dataFull = JsonConvert.DeserializeObject<List<DefaultSchemeParameters>>(data);
                ViewBag.CommunicationData = CalculateTotal(dataFull, ParameterCategory.CommunicationCosts, true);
                ViewBag.OperatingCosts = CalculateTotal(dataFull, ParameterCategory.SchemeAdministratorOperatingCosts,true);
                ViewBag.PreparationCosts = CalculateTotal(dataFull, ParameterCategory.LocalAuthorityDataPreparationCosts, true);
                ViewBag.SchemeSetupCosts = CalculateTotal(dataFull, ParameterCategory.SchemeSetupCosts, true);
                ViewBag.LateReportingTonnage = CalculateTotal(dataFull, ParameterCategory.LateReportingTonnage);
                ViewBag.MaterialityThreshold = CalculateTotal(dataFull, ParameterCategory.MaterialityThreshold);
                ViewBag.BadDebtProvision = CalculateTotal(dataFull, ParameterCategory.BadDebtProvision);
                ViewBag.Levy = CalculateTotal(dataFull, ParameterCategory.Levy);
                ViewBag.TonnageChange = CalculateTotal(dataFull, ParameterCategory.TonnageChangeThreshold);

                return View();
            }

            return BadRequest(response.Content.ReadAsStringAsync().Result);

        }


        private List<DefaultSchemeParameters> CalculateTotal(List<DefaultSchemeParameters> defaultSchemeParameters,string category,bool isTotalRequired = false)
        {
            var schemeParametersBasedonCategory = defaultSchemeParameters.Where(t=>t.ParameterCategory == category).ToList();
            if (isTotalRequired) schemeParametersBasedonCategory.Add(new DefaultSchemeParameters() { ParameterCategory = category, ParameterType = ParameterCategory.Total, ParameterValue = schemeParametersBasedonCategory.Sum(t => t.ParameterValue) });

            return schemeParametersBasedonCategory;
        }


    }
}
