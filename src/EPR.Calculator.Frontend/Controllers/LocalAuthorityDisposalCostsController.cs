using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityDisposalCostsController(
    IConfiguration configuration,
    IEprCalculatorApiService eprCalculatorApiService)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    /// <summary>
    ///     Handles the Index action asynchronously. Sends an HTTP request and processes the response to display local
    ///     authority disposal costs.
    /// </summary>
    /// <returns>
    ///     An IActionResult that renders the LocalAuthorityDisposalCostsIndex view with grouped local authority data if the
    ///     request is successful.
    ///     Redirects to the StandardErrorIndex action in case of an error.
    /// </returns>
    [Route("ViewLocalAuthorityDisposalCosts")]
    public async Task<IActionResult> Index()
    {
        var relativeYear = CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth);

        var currentCosts = await eprCalculatorApiService.Get<List<LocalAuthorityDisposalCost>>($"v1/lapcapData/{relativeYear}");

        if (currentCosts?.Count > 0)
        {
            var localAuthorityDataGroupedByCountry = LocalAuthorityDataUtil
                .GetLocalAuthorityData(currentCosts, MaterialTypes.Other)
                .GroupBy(data => data.Country)
                .ToList();

            return View(
                ViewNames.LocalAuthorityDisposalCostsIndex,
                new LocalAuthorityViewModel
                {
                    LastUpdatedBy = currentCosts.FirstOrDefault()?.CreatedBy ?? ErrorMessages.UnknownUser,
                    ByCountry = localAuthorityDataGroupedByCountry
                });
        }

        return View(ViewNames.LocalAuthorityDisposalCostsIndex, new LocalAuthorityViewModel
        {
            LastUpdatedBy = ErrorMessages.UnknownUser,
            ByCountry = new List<IGrouping<string, LocalAuthorityViewModel.LocalAuthorityData>>()
        });
    }
}
