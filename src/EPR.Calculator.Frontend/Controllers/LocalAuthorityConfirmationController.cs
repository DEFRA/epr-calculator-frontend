using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class LocalAuthorityConfirmationController(IConfiguration configuration)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    public IActionResult Index()
    {
        var relativeYear = CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth);

        var viewModel = new CsvUploadSuccessViewModel
        {
            Title = $"Calculator local authority disposal costs saved for {relativeYear.ToFinancialYear()}",
            RelativeYear = relativeYear,
            ViewLinkUrl = Url.Action("Index", "LocalAuthorityDisposalCosts")!,
            ViewLinkText = "View Local authority disposal costs"
        };

        return View("Views/CsvUpload/Success", viewModel);
    }
}
