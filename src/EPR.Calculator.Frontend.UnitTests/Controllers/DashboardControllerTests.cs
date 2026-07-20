using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Controllers;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.Services;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace EPR.Calculator.Frontend.UnitTests.Controllers;

[TestClass]
public class DashboardControllerTests
{
    private const int RelativeYearStartingMonth = 4;
    private static readonly int[] ExpectedActiveCalculationIds = [1, 2];

    private static readonly KeyValuePair<string, string?>[] RelativeYearStartingMonthConfiguration =
    [
        new(
            CommonConstants.RelativeYearStartingMonth,
            RelativeYearStartingMonth.ToString())
    ];

    private Mock<IEprCalculatorApiService> apiService = null!;
    private IConfiguration configuration = null!;
    private InMemorySession session = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        apiService = new Mock<IEprCalculatorApiService>();

        apiService
            .Setup(service => service.FindRelativeYears())
            .ReturnsAsync([(RelativeYear) 2026]);

        apiService
            .Setup(service => service.FindCalculatorRuns(It.IsAny<RelativeYear>()))
            .ReturnsAsync([]);

        session = new InMemorySession();
        configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(RelativeYearStartingMonthConfiguration)
            .Build();
    }

    [TestMethod]
    public async Task GetCalculations_WhenRunsContainDeletedAndQueue_FiltersThemOut()
    {
        // Arrange
        apiService
            .Setup(service => service.FindCalculatorRuns(It.IsAny<RelativeYear>()))
            .ReturnsAsync([
                BuildRun(1, RunClassification.RUNNING),
                BuildRun(2, RunClassification.TEST_RUN),
                BuildRun(3, RunClassification.DELETED),
                BuildRun(4, RunClassification.QUEUE)
            ]);

        var controller = BuildController();

        // Act
        var result = await controller.GetCalculations(2026) as PartialViewResult;

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("_CalculationRunsPartial", result.ViewName);

        var calculations = result.Model as IEnumerable<CalculationRunViewModel>;
        Assert.IsNotNull(calculations);

        var calculationIds = calculations.Select(calculation => calculation.RunId).ToList();
        CollectionAssert.AreEquivalent(ExpectedActiveCalculationIds, calculationIds);
        Assert.AreEqual(2026, session.GetInt32(SessionConstants.RelativeYear));
    }

    [TestMethod]
    public async Task Index_WhenRunIsError_SetsShowErrorLink()
    {
        apiService
            .Setup(service => service.FindCalculatorRuns(It.IsAny<RelativeYear>()))
            .ReturnsAsync([
                BuildRun(5, RunClassification.ERROR)
            ]);

        var controller = BuildController();

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as DashboardViewModel;
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Calculations);

        var calculation = model.Calculations.Single();
        Assert.AreEqual(RunClassification.ERROR, calculation.RunClassification);
        Assert.IsTrue(calculation.ShowErrorLink);
    }

    [TestMethod]
    public async Task GetCalculations_WhenRunIsCompleted_ReturnsCompletedRunLink()
    {
        // Arrange
        var runId = 10;

        apiService
            .Setup(service => service.FindCalculatorRuns(It.IsAny<RelativeYear>()))
            .ReturnsAsync([
                BuildRun(runId, RunClassification.INITIAL_RUN_COMPLETED)
            ]);

        var controller = BuildController();

        // Act
        var result = await controller.GetCalculations(2026) as PartialViewResult;

        // Assert
        Assert.IsNotNull(result);
        var calculations = result.Model as IEnumerable<CalculationRunViewModel>;
        Assert.IsNotNull(calculations);

        var calculation = calculations.Single();
        Assert.AreEqual(
            string.Format(ActionNames.CompletedRun, runId),
            calculation.RunDetailLink);
    }

    [TestMethod]
    public async Task Index_BuildsRelativeYearList_WithCurrentYearFirstAndSelected()
    {
        // Arrange
        var currentRelativeYear = CommonUtil.GetDefaultRelativeYear(
            DateTime.UtcNow,
            RelativeYearStartingMonth).Value;

        apiService
            .Setup(service => service.FindRelativeYears())
            .ReturnsAsync([new RelativeYear(currentRelativeYear - 1), new RelativeYear(currentRelativeYear - 2), new RelativeYear(3000)]);

        apiService
            .Setup(service => service.FindCalculatorRuns(It.IsAny<RelativeYear>()))
            .ReturnsAsync([
                BuildRun(21, RunClassification.TEST_RUN)
            ]);

        var controller = BuildController();

        // Act
        var result = await controller.Index() as ViewResult;

        // Assert
        Assert.IsNotNull(result);
        var model = result.Model as DashboardViewModel;
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.RelativeYearSelectList);

        Assert.IsFalse(model.RelativeYearSelectList.Any(item => item.Value == "3000"));
        Assert.AreEqual(currentRelativeYear.ToString(), model.RelativeYearSelectList[0].Value);
        Assert.IsTrue(model.RelativeYearSelectList[0].Selected);

        var selectedItem = model.RelativeYearSelectList.Single(item => item.Selected);
        Assert.AreEqual(model.RelativeYear.Value.ToString(), selectedItem.Value);
    }

    private DashboardController BuildController()
    {
        return new DashboardController(
            configuration,
            apiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = session
                }
            }
        };
    }

    private static CalculatorRunDto BuildRun(int runId, RunClassification classification)
    {
        return new CalculatorRunDto
        {
            RunId = runId,
            RunClassification = classification,
            RunName = $"Run {runId}",
            CreatedAt = new DateTime(2025, 6, 30, 10, 0, 0, DateTimeKind.Utc),
            CreatedBy = "Test User",
            RelativeYear = new RelativeYear(2024),
            BillingRunStatus = BillingRunStatus.None
        };
    }

    private sealed class InMemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> store = new();

        public IEnumerable<string> Keys => store.Keys;

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable => true;

        public void Clear()
        {
            store.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            store.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            store[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            return store.TryGetValue(key, out value!);
        }
    }
}
