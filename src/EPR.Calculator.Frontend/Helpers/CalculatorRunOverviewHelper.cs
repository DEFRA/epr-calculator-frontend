using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Helpers;

public static class CalculatorRunOverviewHelper
{
    public static async Task<CalculatorRunOverviewViewModel> CreateViewModel(CalculatorRunDetailsViewModel runDetails, string backLink, HttpContext httpContext)
    {
        var currentUser = CommonUtil.GetUserName(httpContext);
        var viewModel = new CalculatorRunOverviewViewModel()
        {
            CurrentUser = currentUser,
            CalculatorRunDetails = new CalculatorRunDetailsViewModel(),
            BackLinkViewModel = new BackLinkViewModel
            {
                BackLink = string.Empty,
                CurrentUser = currentUser,
                HideBackLink = backLink != ControllerNames.Dashboard,
            },
        };

        if (runDetails != null && runDetails!.RunId > 0)
        {
            viewModel.CalculatorRunDetails = runDetails;
        }

        return viewModel;
    }
}
