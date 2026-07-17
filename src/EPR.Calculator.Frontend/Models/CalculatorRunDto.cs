using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.Models;

[ExcludeFromCodeCoverage]
public record CalculatorRunDto
{
    public int RunId { get; init; }
    public RunClassification RunClassification { get; init; }
    public RelativeYear RelativeYear { get; init; }
    public string RunName { get; init; } = "";
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = "";
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public BillingRunStatus BillingRunStatus { get; init; }
    public DateTime? BillingRunStartedAt { get; init; }
    public BillingFileDto? BillingFile { get; init; }

    public record BillingFileDto
    {
        public int Id { get ; init; }
        public bool IsLatest { get ; init; }
        public bool HasBeenSentToFss { get; init; }
        public string CsvFileName { get; init; } = null!;
        public string JsonFileName { get; init; } = null!;
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = null!;
        public DateTime? SentAt { get; init; }
        public string? SentBy { get; init; }
    }
}
