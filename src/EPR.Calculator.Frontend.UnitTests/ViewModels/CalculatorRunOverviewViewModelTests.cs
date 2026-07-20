using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.ViewModels;

[TestClass]
public class CalculatorRunOverviewViewModelTests
{
    [TestMethod]
    public void BillingRunErrored_SetsErroredFlagAndAllowsRetry()
    {
        // Arrange
        var viewModel = CreateViewModel(BillingRunStatus.Errored);

        // Assert
        Assert.IsTrue(viewModel.IsBillingFileErrored);
        Assert.IsTrue(viewModel.IsNewBillingRunAllowed);
        Assert.IsFalse(viewModel.IsBillingFileRunning);
        Assert.IsFalse(viewModel.IsLatestBillingFile);
        Assert.IsFalse(viewModel.IsSubmitAllowed);
    }

    [DataTestMethod]
    [DataRow(BillingRunStatus.None)]
    [DataRow(BillingRunStatus.Running)]
    [DataRow(BillingRunStatus.Completed)]
    public void OtherBillingRunStatuses_DoNotSetErroredFlag(BillingRunStatus status)
    {
        // Arrange
        var viewModel = CreateViewModel(status);

        // Assert
        Assert.IsFalse(viewModel.IsBillingFileErrored);
    }

    private static CalculatorRunOverviewViewModel CreateViewModel(BillingRunStatus status)
    {
        return new CalculatorRunOverviewViewModel
        {
            Run = new CalculatorRunDto
            {
                RunId = 1,
                RunClassification = RunClassification.INITIAL_RUN,
                RelativeYear = new RelativeYear(2026),
                RunName = "Test run",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "test-user",
                BillingRunStatus = status,
                BillingFile = new CalculatorRunDto.BillingFileDto
                {
                    Id = 1,
                    IsLatest = true,
                    HasBeenSentToFss = false,
                    CsvFileName = "billing.csv",
                    JsonFileName = "billing.json",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "test-user"
                }
            }
        };
    }
}
