namespace EPR.Calculator.Frontend.Models;

public record SetLapcapDataRequest
{
    public required string Filename { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required IReadOnlyList<LapcapValue> Values { get; init; }

    public record LapcapValue
    {
        public required string Country { get; init; }
        public required string Material { get; init; }
        public required decimal TotalCost { get; init; }
    }
}
