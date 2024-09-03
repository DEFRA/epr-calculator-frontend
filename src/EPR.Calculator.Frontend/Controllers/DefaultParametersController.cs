using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DefaultParametersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _clientFactory;

        public DefaultParametersController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            _configuration = configuration;
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var parameterSettingsApi = _configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

                var client = _clientFactory.CreateClient();
                client.BaseAddress = new Uri(parameterSettingsApi);
                var year = _configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value;
                var uri = new Uri(string.Format("{0}/{1}", parameterSettingsApi, year));
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var defaultSchemeParameters = JsonConvert.DeserializeObject<List<DefaultSchemeParameters>>(data);
                    ViewBag.CommunicationData = CalculateTotal(defaultSchemeParameters, ParameterType.CommunicationCosts, true);
                    ViewBag.OperatingCosts = CalculateTotal(defaultSchemeParameters, ParameterType.SchemeAdministratorOperatingCosts, true);
                    ViewBag.PreparationCosts = CalculateTotal(defaultSchemeParameters, ParameterType.LocalAuthorityDataPreparationCosts, true);
                    ViewBag.SchemeSetupCosts = CalculateTotal(defaultSchemeParameters, ParameterType.SchemeSetupCosts, true);
                    ViewBag.LateReportingTonnage = CalculateTotal(defaultSchemeParameters, ParameterType.LateReportingTonnage);
                    ViewBag.MaterialityThreshold = CalculateTotal(defaultSchemeParameters, ParameterType.MaterialityThreshold);
                    ViewBag.BadDebtProvision = CalculateTotal(defaultSchemeParameters, ParameterType.BadDebtProvision);
                    ViewBag.Levy = CalculateTotal(defaultSchemeParameters, ParameterType.Levy);
                    ViewBag.TonnageChange = CalculateTotal(defaultSchemeParameters, ParameterType.TonnageChangeThreshold);
                    ViewBag.EffectiveFrom = defaultSchemeParameters[0].EffectiveFrom;

                    ViewBag.IsDataAvailable = true;

                    return View();
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    ViewBag.IsDataAvailable = false;
                    return View();
                }

                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                return RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private List<DefaultSchemeParameters> CalculateTotal(List<DefaultSchemeParameters> defaultSchemeParameters, string type, bool isTotalRequired = false)
        {
            var schemeParametersBasedonCategory = defaultSchemeParameters.Where(t => t.ParameterType == type).ToList();
            if (isTotalRequired)
            {
                schemeParametersBasedonCategory.Add(new DefaultSchemeParameters() { ParameterType = type, ParameterCategory = ParameterType.Total, ParameterValue = schemeParametersBasedonCategory.Sum(t => t.ParameterValue) });
            }

            return schemeParametersBasedonCategory;
        }
    }
}
