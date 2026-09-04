namespace EPR.Calculator.Frontend.ViewModels.CsvUpload;

public record CsvUploadProcessingViewModel
{
    public required string ProcessingUrl { get; init; }
    public required string SuccessUrl { get; init; }
    public required string ErrorUrl { get; init; }
    public required string JsonPayload { get; init; }
}
