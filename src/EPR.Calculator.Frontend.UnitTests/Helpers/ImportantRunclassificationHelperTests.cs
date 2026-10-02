using System.Collections.Immutable;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Helpers;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.UnitTests.Helpers;

[TestClass]
public class ImportantRunClassificationHelperTests
{
    private static readonly ImmutableList<RunClassification> DefaultAvailableClassifications =
        [RunClassification.Initial, RunClassification.Recalculation, RunClassification.Test];

    [DataTestMethod]
    [DataRow(RunClassification.Initial)]
    [DataRow(RunClassification.Recalculation)]
    public void CreateNotificationViewModel_OnlyTestAvailableWithIncompleteOfficialRun_ReturnsTestOnlyIncompleted(
        RunClassification incompleteClassification)
    {
        // Arrange
        var incompleteRun = CreateRun(incompleteClassification, runId: 101, updatedAt: new DateTime(2025, 6, 1));
        ImmutableList<CalculatorRunDto> classifiedRuns = [incompleteRun];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns, availableClassifications: [RunClassification.Test]);

        // Assert
        Assert.AreEqual(ImportantSectionViewModel.NotificationType.TestOnlyIncompleted, result.Notification);
        Assert.AreEqual(incompleteRun.RunId, result.IncompleteOfficialRunId);
    }

    [TestMethod]
    public void CreateNotificationViewModel_OnlyTestAvailableWithNoOfficialRuns_ReturnsTestOnlyOutdated()
    {
        // Arrange
        var relativeYear = new RelativeYear(2025);
        ImmutableList<CalculatorRunDto> classifiedRuns =
        [
            CreateRun(RunClassification.Test, runId: 1),
            CreateRun(RunClassification.Initial, runId: 2, sharedAt: DateTime.UtcNow.AddDays(-1))
        ];

        // Act
        var result = CreateNotificationViewModel(
            classifiedRuns,
            availableClassifications: [RunClassification.Test],
            relativeYear: relativeYear);

        // Assert
        Assert.AreEqual(ImportantSectionViewModel.NotificationType.TestOnlyOutdated, result.Notification);
        Assert.IsNull(result.IncompleteOfficialRunId);
        Assert.AreEqual(relativeYear, result.RelativeYear);
    }

    [DataTestMethod]
    [DataRow(RunClassification.Initial)]
    [DataRow(RunClassification.Recalculation)]
    public void CreateNotificationViewModel_OfficialRunCompleted_ReturnsAlreadyCompletedWithSentDate(
        RunClassification completedClassification)
    {
        // Arrange
        var sentAt = new DateTime(2025, 5, 1);
        ImmutableList<CalculatorRunDto> classifiedRuns = [CreateRun(completedClassification, sharedAt: sentAt)];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns);

        // Assert
        Assert.AreEqual(ImportantSectionViewModel.NotificationType.AlreadyCompleted, result.Notification);

        var completedAt = completedClassification == RunClassification.Initial
            ? result.InitialRunCompletedAt
            : result.RecalculationCompletedAt;

        Assert.AreEqual(sentAt, completedAt);
    }

    [TestMethod]
    public void CreateNotificationViewModel_BothOfficialRunTypesCompleted_SetsBothCompletionDatesIndependently()
    {
        // Arrange
        var initialSentAt = new DateTime(2025, 4, 1);
        var recalculationSentAt = new DateTime(2025, 5, 1);
        ImmutableList<CalculatorRunDto> classifiedRuns =
        [
            CreateRun(RunClassification.Initial, CalculationRunStatus.Completed, runId: 1, sharedAt: initialSentAt),
            CreateRun(RunClassification.Recalculation, CalculationRunStatus.Completed, runId: 2, sharedAt: recalculationSentAt)
        ];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns);

        // Assert
        Assert.AreEqual(ImportantSectionViewModel.NotificationType.AlreadyCompleted, result.Notification);
        Assert.AreEqual(initialSentAt, result.InitialRunCompletedAt);
        Assert.AreEqual(recalculationSentAt, result.RecalculationCompletedAt);
    }

    [TestMethod]
    public void CreateNotificationViewModel_MultipleCompletedRunsOfSameType_UsesMostRecentlySentBillingFile()
    {
        // Arrange
        var earlierSentButLaterUpdated = CreateRun(
            RunClassification.Recalculation,
            CalculationRunStatus.Completed,
            runId: 1,
            updatedAt: new DateTime(2025, 6, 1),
            sharedAt: new DateTime(2025, 1, 1));

        var laterSentButEarlierUpdated = CreateRun(
            RunClassification.Recalculation,
            CalculationRunStatus.Completed,
            runId: 2,
            updatedAt: new DateTime(2025, 1, 1),
            sharedAt: new DateTime(2025, 3, 1));

        ImmutableList<CalculatorRunDto> classifiedRuns = [earlierSentButLaterUpdated, laterSentButEarlierUpdated];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns);

        // Assert
        Assert.AreEqual(new DateTime(2025, 3, 1), result.RecalculationCompletedAt);
    }

    [TestMethod]
    public void CreateNotificationViewModel_IncompleteOfficialRunBlocksNewOfficialClassification_TakesPriorityOverAlreadyCompleted()
    {
        // Arrange
        var initialSentAt = new DateTime(2025, 1, 1);
        var completedInitialRun = CreateRun(RunClassification.Initial, CalculationRunStatus.Completed, runId: 1, updatedAt: initialSentAt, sharedAt: initialSentAt);
        var incompleteRecalculationRun = CreateRun(RunClassification.Recalculation, runId: 2, updatedAt: new DateTime(2025, 6, 1));
        ImmutableList<CalculatorRunDto> classifiedRuns = [completedInitialRun, incompleteRecalculationRun];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns, availableClassifications: [RunClassification.Test]);

        // Assert
        Assert.AreEqual(ImportantSectionViewModel.NotificationType.TestOnlyIncompleted, result.Notification);
        Assert.AreEqual(incompleteRecalculationRun.RunId, result.IncompleteOfficialRunId);
        Assert.AreEqual(initialSentAt, result.InitialRunCompletedAt);
    }

    [TestMethod]
    public void CreateNotificationViewModel_MostRecentlyUpdatedOfficialRunIrrespectiveOfListOrder_IsUsedAsIncompleteOfficialRunId()
    {
        // Arrange
        var oldest = CreateRun(RunClassification.Initial, runId: 1, updatedAt: new DateTime(2025, 1, 1));
        var mostRecent = CreateRun(RunClassification.Recalculation, runId: 2, updatedAt: new DateTime(2025, 3, 1));
        var middle = CreateRun(RunClassification.Initial, runId: 3, updatedAt: new DateTime(2025, 2, 1));
        ImmutableList<CalculatorRunDto> classifiedRuns = [oldest, mostRecent, middle];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns);

        // Assert
        Assert.AreEqual(mostRecent.RunId, result.IncompleteOfficialRunId);
    }

    [TestMethod]
    public void CreateNotificationViewModel_NoOfficialRunsAndMultipleClassificationsAvailable_ReturnsNoNotification()
    {
        // Arrange
        var relativeYear = new RelativeYear(2026);
        ImmutableList<CalculatorRunDto> classifiedRuns = [CreateRun(RunClassification.Test, runId: 1)];

        // Act
        var result = CreateNotificationViewModel(classifiedRuns, relativeYear: relativeYear);

        // Assert
        Assert.AreEqual(ImportantSectionViewModel.NotificationType.None, result.Notification);
        Assert.IsNull(result.IncompleteOfficialRunId);
        Assert.IsNull(result.InitialRunCompletedAt);
        Assert.IsNull(result.RecalculationCompletedAt);
        Assert.AreEqual(relativeYear, result.RelativeYear);
    }

    private static ImportantSectionViewModel CreateNotificationViewModel(
        ImmutableList<CalculatorRunDto> classifiedRuns,
        ImmutableList<RunClassification>? availableClassifications = null,
        RelativeYear? relativeYear = null) =>
        ImportantRunClassificationHelper.CreateNotificationViewModel(
            availableClassifications ?? DefaultAvailableClassifications,
            classifiedRuns,
            relativeYear ?? new RelativeYear(2025));

    private static CalculatorRunDto CreateRun(
        RunClassification classification,
        CalculationRunStatus calculationRunStatus = CalculationRunStatus.None,
        int runId = 1,
        DateTime? updatedAt = null,
        DateTime? sharedAt = null)
    {
        var effectiveUpdatedAt = updatedAt ?? sharedAt ?? new DateTime(2025, 1, 1);

        return new CalculatorRunDto
        {
            RunId = runId,
            RunName = $"Run {runId}",
            RunClassification = classification,
            CalculationRunStatus = calculationRunStatus,
            CreatedAt = effectiveUpdatedAt.AddDays(-1),
            UpdatedAt = effectiveUpdatedAt,
            BillingFile = sharedAt is null ? null : new CalculatorRunDto.BillingFileDto
            {
                IsShared = true,
                SharedAt = sharedAt
            },
        };
    }
}
