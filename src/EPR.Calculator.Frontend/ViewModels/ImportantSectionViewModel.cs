using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record ImportantSectionViewModel
{
    public required RelativeYear RelativeYear { get ; init ; }
    public NotificationType Notification { get; init; }
    public int? IncompleteOfficialRunId { get ; set ; }
    public DateTime? RecalculationCompletedAt { get ; set ; }
    public DateTime? InitialRunCompletedAt { get ; set ; }

    public enum NotificationType
    {
        None,
        TestOnlyIncompleted,
        TestOnlyOutdated,
        AlreadyCompleted
    }
}
