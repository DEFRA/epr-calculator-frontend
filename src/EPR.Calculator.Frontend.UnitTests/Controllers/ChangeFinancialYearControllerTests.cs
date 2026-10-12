using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.UnitTests.Mocks;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class ChangeFinancialYearControllerTests
{
    private const int RelativeYearStartingMonth = 4;

    private readonly RelativeYear currentYear =
        CommonUtil.GetDefaultRelativeYear(DateTime.UtcNow, RelativeYearStartingMonth);

    private Mock<IEprCalculatorApiService> apiService = null!;
    private InMemorySession session = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();
        apiService
            .Setup(service => service.FindRelativeYears())
            .ReturnsAsync([
                new RelativeYear(currentYear.Value - 2),
                new RelativeYear(currentYear.Value + 1),
                new RelativeYear(currentYear.Value - 1)
            ]);

        session = new InMemorySession();
    }

    [TestMethod]
    public async Task Index_ListsYearsNewestFirst_ExcludingFutureYearsAndIncludingCurrentYear()
    {
        // Act
        var result = await BuildController().Index() as ViewResult;

        // Assert
        Assert.AreEqual(ViewNames.ChangeFinancialYearIndex, result?.ViewName);
        var model = result?.Model as ChangeFinancialYearViewModel;
        Assert.IsNotNull(model);
        CollectionAssert.AreEqual(
            new[] { currentYear.Value, currentYear.Value - 1, currentYear.Value - 2 },
            model.RelativeYears.Select(year => year.Value).ToArray());
        Assert.IsNull(model.RelativeYear);
    }

    [TestMethod]
    public async Task Index_Post_WhenYearSelected_StoresYearAndRedirectsToDashboard()
    {
        // Arrange
        var selectedYear = currentYear.Value - 1;

        // Act
        var result = await BuildController().Index(new ChangeFinancialYearFormModel { RelativeYear = selectedYear })
            as RedirectToActionResult;

        // Assert
        Assert.AreEqual(ActionNames.Index, result?.ActionName);
        Assert.AreEqual(ControllerNames.Dashboard, result?.ControllerName);
        Assert.AreEqual(selectedYear, session.GetInt32(SessionConstants.RelativeYear));
    }

    [TestMethod]
    public async Task Index_Post_WhenNoYearSelected_ReturnsViewWithError()
    {
        // Arrange
        session.SetInt32(SessionConstants.RelativeYear, currentYear.Value - 2);
        var controller = BuildController();
        controller.ModelState.AddModelError(
            nameof(ChangeFinancialYearFormModel.RelativeYear),
            ErrorMessages.FinancialYearNotSelected);

        // Act
        var result = await controller.Index(new ChangeFinancialYearFormModel()) as ViewResult;

        // Assert
        Assert.AreEqual(ViewNames.ChangeFinancialYearIndex, result?.ViewName);
        Assert.IsInstanceOfType<ChangeFinancialYearViewModel>(result?.Model);
        Assert.AreEqual(currentYear.Value - 2, session.GetInt32(SessionConstants.RelativeYear));
    }

    [TestMethod]
    public async Task Index_Post_WhenYearNotAvailable_ReturnsViewWithError()
    {
        // Arrange
        var controller = BuildController();

        // Act
        var result = await controller.Index(new ChangeFinancialYearFormModel { RelativeYear = currentYear.Value + 1 })
            as ViewResult;

        // Assert
        Assert.AreEqual(ViewNames.ChangeFinancialYearIndex, result?.ViewName);
        Assert.AreEqual(
            ErrorMessages.FinancialYearNotSelected,
            controller.ModelState[nameof(ChangeFinancialYearFormModel.RelativeYear)]?.Errors.Single().ErrorMessage);
        Assert.IsNull(session.GetInt32(SessionConstants.RelativeYear));
    }

    private ChangeFinancialYearController BuildController()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>(
                    CommonConstants.RelativeYearStartingMonth,
                    RelativeYearStartingMonth.ToString())
            ])
            .Build();

        return new ChangeFinancialYearController(configuration, apiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            }
        };
    }
}
