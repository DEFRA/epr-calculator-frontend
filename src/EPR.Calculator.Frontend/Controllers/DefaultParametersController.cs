using System.Net;
using Azure.Core;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling default parameter settings.
    /// </summary>
    [Authorize(Roles = "SASuperUser")]
    public class DefaultParametersController : BaseController
    {
        /// <summary>
        /// The factory for creating HTTP clients.
        /// </summary>
        private readonly IHttpClientFactory clientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultParametersController"/> class.
        /// </summary>
        /// <param name="configuration">The configuration settings for the application.</param>
        /// <param name="clientFactory">The factory for creating HTTP clients.</param>
        /// <param name="tokenAcquisition">The token acquisition service.</param>
        /// <param name="telemetryClient">The telemetry client for logging and monitoring.</param>
        public DefaultParametersController(IConfiguration configuration, IHttpClientFactory clientFactory, ITokenAcquisition tokenAcquisition, TelemetryClient telemetryClient)
            : base(configuration, tokenAcquisition, telemetryClient)
        {
            this.clientFactory = clientFactory;
        }

        /// <summary>
        /// Handles the Index action for retrieving and displaying default scheme parameters.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> that renders the view with the retrieved data or redirects to an error page.
        /// </returns>
        [Authorize(Roles = "SASuperUser")]
        [Route("ViewDefaultParameters")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var parameterSettingsApi = this.Configuration.GetSection(ConfigSection.ParameterSettings).GetSection(ConfigSection.DefaultParameterSettingsApi).Value;

                if (string.IsNullOrWhiteSpace(parameterSettingsApi))
                {
                    throw new ArgumentNullException(parameterSettingsApi, "ParameterSettingsApi is null. Check the configuration settings for default parameters");
                }

                var client = this.clientFactory.CreateClient();
                client.BaseAddress = new Uri(parameterSettingsApi);
                var accessToken = await this.AcquireToken();
                client.DefaultRequestHeaders.Add("Authorization", accessToken);

                var year = this.Configuration.GetSection(ConfigSection.ParameterSettings).GetSection(ConfigSection.ParameterYear).Value;

                var uri = new Uri(string.Format("{0}/{1}", parameterSettingsApi, year));
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var defaultSchemeParameters = JsonConvert.DeserializeObject<List<DefaultSchemeParameters>>(data);

                    if (defaultSchemeParameters != null)
                    {
                        this.ViewBag.CommunicationData = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.CommunicationCostsByMaterial);
                        this.ViewBag.CommunicationCostsByCountry = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.CommunicationCostsByCountry);
                        this.ViewBag.OperatingCosts = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.SchemeAdministratorOperatingCosts);
                        this.ViewBag.PreparationCosts = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.LocalAuthorityDataPreparationCosts);
                        this.ViewBag.SchemeSetupCosts = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.SchemeSetupCosts);
                        this.ViewBag.LateReportingTonnage = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.LateReportingTonnage);
                        this.ViewBag.MaterialityThreshold = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.MaterialityThreshold);
                        this.ViewBag.BadDebtProvision = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.BadDebtProvision);
                        this.ViewBag.TonnageChange = GetSchemeParametersBasedonCategory(defaultSchemeParameters, ParameterType.TonnageChangeThreshold);
                        this.ViewBag.EffectiveFrom = defaultSchemeParameters.First().EffectiveFrom;
                        this.ViewBag.IsDataAvailable = true;

                        return this.View(
                            new DefaultParametersViewModel
                            {
                                CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                                LastUpdatedBy = defaultSchemeParameters.First().CreatedBy,
                            });
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    this.ViewBag.IsDataAvailable = false;
                    return this.View(new DefaultParametersViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        LastUpdatedBy = string.Empty,
                    });
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private static List<DefaultSchemeParameters> GetSchemeParametersBasedonCategory(List<DefaultSchemeParameters> defaultSchemeParameters, string type)
        {
            var schemeParametersBasedonCategory = defaultSchemeParameters.Where(t => t.ParameterType == type).ToList();
            return schemeParametersBasedonCategory;
        }
    }
}