using System.Net;
using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Extensions;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class DefaultParametersController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    /// <summary>
    ///     Handles the Index action for retrieving and displaying default scheme parameters.
    /// </summary>
    /// <returns>
    ///     An <see cref="IActionResult" /> that renders the view with the retrieved data or redirects to an error page.
    /// </returns>
    [Route("ViewDefaultParameters")]
    public async Task<IActionResult> Index()
    {
        var relativeYear = CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth);
        var response = await GetDefaultParametersAsync(relativeYear);

        if (response.IsSuccessStatusCode)
        {
            var viewModel = new DefaultParametersViewModel
            {
                LastUpdatedBy = CommonUtil.GetUserName(HttpContext),
                SchemeParameters = new List<SchemeParametersViewModel>(),
                LateReportingTonnageParams = new List<DefaultSchemeParametersLateReportingTonnage>()
            };
            var data = await response.Content.ReadAsStringAsync();
            var defaultSchemeParameters = await response.Content.ReadFromJsonAsync<List<DefaultSchemeParameters>>() ?? new List<DefaultSchemeParameters>();

            foreach (var name in (ParameterType[])Enum.GetValues(typeof(ParameterType)))
                viewModel.SchemeParameters.Add(GetSchemeParametersBasedonCategory(defaultSchemeParameters, name));

            var lateTonnage = viewModel.SchemeParameters.First(t => t.SchemeParameterName == ParameterType.LateReportingTonnage.GetDisplayName());
            viewModel.LateReportingTonnageParams = GetModulatedLateReportingTonnageParams(lateTonnage.DefaultSchemeParameters);

            viewModel.EffectiveFrom = defaultSchemeParameters.First().EffectiveFrom;
            viewModel.IsDataAvailable = true;

            return View(viewModel);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return View(new DefaultParametersViewModel
            {
                LastUpdatedBy = string.Empty,
                IsDataAvailable = false
            });
        }

        return RedirectToError();
    }

    private static IEnumerable<DefaultSchemeParametersLateReportingTonnage> GetModulatedLateReportingTonnageParams(IEnumerable<DefaultSchemeParameters> parameters)
    {
        return parameters
            .GroupBy(x => x.ParameterCategory.Split('-')[0].Trim())
            .Select(group => new DefaultSchemeParametersLateReportingTonnage
            {
                Material = group.Key,
                Red = group.First(x => x.ParameterCategory.EndsWith("-R")).ParameterValue,
                Amber = group.First(x => x.ParameterCategory.EndsWith("-A")).ParameterValue,
                Green = group.First(x => x.ParameterCategory.EndsWith("-G")).ParameterValue
            });
    }

    private static SchemeParametersViewModel GetSchemeParametersBasedonCategory(List<DefaultSchemeParameters> defaultSchemeParameters, ParameterType parameterType)
    {
        var type = parameterType.GetDisplayName();
        var shouldDisplayPrefix = !IsExcludedFromPrefixDisplay(parameterType);

        return new SchemeParametersViewModel
        {
            DefaultSchemeParameters = defaultSchemeParameters.Where(t => t.ParameterType == type).ToList(),
            IsDisplayPrefix = shouldDisplayPrefix,
            SchemeParameterName = type
        };
    }

    private static bool IsExcludedFromPrefixDisplay(ParameterType parameterType)
    {
        return parameterType == ParameterType.LateReportingTonnage || parameterType == ParameterType.BadDebtProvision || parameterType == ParameterType.RedModulationFactor;
    }

    private async Task<HttpResponseMessage> GetDefaultParametersAsync(RelativeYear relativeYear)
    {
        return await eprCalculatorApiService.CallApi(
            HttpContext,
            HttpMethod.Get,
            $"v1/defaultParameterSetting/{relativeYear}");
    }
}
