using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record LapcapProcessingViewModel
{
    public required string Filename { get; init; }
    public required IReadOnlyList<CreateLapcapDataRequest.LapcapValue> Values { get; init; }
}
