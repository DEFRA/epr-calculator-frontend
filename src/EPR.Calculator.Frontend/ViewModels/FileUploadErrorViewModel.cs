namespace EPR.Calculator.Frontend.ViewModels;

public record FileUploadErrorViewModel
{
    public required string InputId { get; init; }
    public required string CallToActionId { get; init; }
    public required string DetailsId { get; init; }
    public bool HasErrors => HasFileErrors || HasContentErrors;
    public bool HasFileErrors => FileErrors.Count > 0;
    public bool HasContentErrors => ContentErrors.Count > 0;
    public required IReadOnlyList<string> FileErrors { get; init; }
    public required IReadOnlyList<string> ContentErrors { get; init; }
    public IReadOnlyList<string> AllErrors => [..FileErrors, ..ContentErrors];
}
