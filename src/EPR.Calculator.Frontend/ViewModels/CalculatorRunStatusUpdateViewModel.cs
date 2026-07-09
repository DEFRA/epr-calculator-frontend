using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record CalculatorRunStatusUpdateViewModel : ViewModelCommonData
{
    public required CalculatorRunStatusUpdateDto Data { get; init; }
    public ErrorViewModel Errors { get; set; } = null!;
}
