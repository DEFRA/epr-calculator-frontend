using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.ViewModels.CsvUpload;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class DefaultParametersConfirmationController(IConfiguration configuration)
    : BaseController
{
    private readonly int relativeYearStartingMonth = CommonUtil.GetRelativeYearStartingMonth(configuration);

    public IActionResult Index()
    {
        var relativeYear = CommonUtil.GetRelativeYear(HttpContext.Session, relativeYearStartingMonth);

        var viewModel = new CsvUploadSuccessViewModel
        {
            Title = $"Calculator parameters saved for {relativeYear.ToFinancialYear()}",
            RelativeYear = relativeYear,
            ViewLinkUrl = Url.Action("Index", "DefaultParameters")!,
            ViewLinkText = "View default calculator parameters"
        };

        return View("Views/CsvUpload/Success", viewModel);
    }
}
