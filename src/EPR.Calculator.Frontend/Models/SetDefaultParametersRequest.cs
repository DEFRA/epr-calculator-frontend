namespace EPR.Calculator.Frontend.Models;

public record SetDefaultParametersRequest
{
    public required string Filename { get; init; }
    public required RelativeYear RelativeYear { get; init; }
    public required IReadOnlyList<ParameterValue> Values { get; init; }

    public record ParameterValue
    {
        public required string Id { get; set; }

        public required string Value { get; set; }
    }
}
