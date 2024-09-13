using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    public class DefaultParametersController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory clientFactory;

        public DefaultParametersController(IConfiguration configuration, IHttpClientFactory clientFactory)
        {
            this.configuration = configuration;
            this.clientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var parameterSettingsApi = this.configuration.GetSection("ParameterSettings").GetSection("DefaultParameterSettingsApi").Value;

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(parameterSettingsApi);
                var year = this.configuration.GetSection("ParameterSettings").GetSection("ParameterYear").Value;
                var uri = new Uri(string.Format("{0}/{1}", parameterSettingsApi, year));
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var defaultSchemeParameters = JsonConvert.DeserializeObject<List<DefaultSchemeParameters>>(data);
                    this.ViewBag.CommunicationData = this.CalculateTotal(defaultSchemeParameters, ParameterType.CommunicationCosts, true);
                    this.ViewBag.OperatingCosts = this.CalculateTotal(defaultSchemeParameters, ParameterType.SchemeAdministratorOperatingCosts, true);
                    this.ViewBag.PreparationCosts = this.CalculateTotal(defaultSchemeParameters, ParameterType.LocalAuthorityDataPreparationCosts, true);
                    this.ViewBag.SchemeSetupCosts = this.CalculateTotal(defaultSchemeParameters, ParameterType.SchemeSetupCosts, true);
                    this.ViewBag.LateReportingTonnage = this.CalculateTotal(defaultSchemeParameters, ParameterType.LateReportingTonnage);
                    this.ViewBag.MaterialityThreshold = this.CalculateTotal(defaultSchemeParameters, ParameterType.MaterialityThreshold);
                    this.ViewBag.BadDebtProvision = this.CalculateTotal(defaultSchemeParameters, ParameterType.BadDebtProvision);
                    this.ViewBag.Levy = this.CalculateTotal(defaultSchemeParameters, ParameterType.Levy);
                    this.ViewBag.TonnageChange = this.CalculateTotal(defaultSchemeParameters, ParameterType.TonnageChangeThreshold);
                    this.ViewBag.EffectiveFrom = defaultSchemeParameters[0].EffectiveFrom;

                    this.ViewBag.IsDataAvailable = true;

                    return this.View();
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    this.ViewBag.IsDataAvailable = false;
                    return this.View();
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
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
