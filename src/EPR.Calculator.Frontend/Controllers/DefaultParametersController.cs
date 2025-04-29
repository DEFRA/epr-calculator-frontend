using System.Net;
using EPR.Calculator.Frontend.Common;
using EPR.Calculator.Frontend.Common.Constants;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace EPR.Calculator.Frontend.Controllers
{
    /// <summary>
    /// Controller for handling default parameter settings.
    /// </summary>
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

                var uri = new Uri(string.Format("{0}/{1}", parameterSettingsApi, this.GetFinancialYear()));
                var response = await client.GetAsync(uri);

                if (response.IsSuccessStatusCode)
                {
                    var viewModel = new DefaultParametersViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        LastUpdatedBy = CommonUtil.GetUserName(this.HttpContext),
                        SchemeParameters = new List<SchemeParametersViewModel>(),
                    };
                    var data = await response.Content.ReadAsStringAsync();
                    var defaultSchemeParameters = JsonConvert.DeserializeObject<List<DefaultSchemeParameters>>(data);

                    if (defaultSchemeParameters != null)
                    {
                        foreach (ParameterType name in (ParameterType[])Enum.GetValues(typeof(ParameterType)))
                        {
                            viewModel.SchemeParameters.Add(GetSchemeParametersBasedonCategory(defaultSchemeParameters, name));
                        }

                        viewModel.EffectiveFrom = defaultSchemeParameters.First().EffectiveFrom;
                        viewModel.IsDataAvailable = true;

                        return this.View(viewModel);
                    }
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return this.View(new DefaultParametersViewModel
                    {
                        CurrentUser = CommonUtil.GetUserName(this.HttpContext),
                        LastUpdatedBy = string.Empty,
                        IsDataAvailable = false,
                    });
                }

                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
            catch (Exception ex)
            {
                return this.RedirectToAction(ActionNames.StandardErrorIndex, "StandardError");
            }
        }

        private static SchemeParametersViewModel GetSchemeParametersBasedonCategory(List<DefaultSchemeParameters> defaultSchemeParameters, ParameterType parameterType)
        {
            var type = parameterType.GetDisplayName();
            bool shouldDisplayPrefix = !IsExcludedFromPrefixDisplay(parameterType);

            return new SchemeParametersViewModel
            {
                DefaultSchemeParameters = defaultSchemeParameters.Where(t => t.ParameterType == type).ToList(),
                IsDisplayPrefix = shouldDisplayPrefix,
                SchemeParameterName = type,
            };
        }

        private static bool IsExcludedFromPrefixDisplay(ParameterType parameterType)
        {
            return parameterType == ParameterType.LateReportingTonnage || parameterType == ParameterType.BadDebtProvision;
        }
    }
}