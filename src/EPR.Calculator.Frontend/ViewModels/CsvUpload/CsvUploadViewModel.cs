namespace EPR.Calculator.Frontend.ViewModels.CsvUpload;

public static class CsvUploadDomElements
{
    public const string InputId = "file-upload-input";
    public const string RequirementsId = "file-upload-requirements";
    public const string ErrorCallToActionId = "file-upload-error-call-to-action";
    public const string ErrorDetailsId = "file-upload-error-details";
}

public record CsvUploadViewModel
{
    public required string Title { get; init; }
    public required string BackLinkUrl { get; init; }
    public required string UploadUrl { get; init; }
    public TemplateDownloadViewModel? DownloadTemplate { get; init; }
    public bool HasDownloadableTemplate => DownloadTemplate != null;
    public bool HasErrors => Errors?.HasErrors ?? false;
    public ErrorsViewModel? Errors { get; init; }

    public string InputDescribedBy => HasErrors
        ? $"{CsvUploadDomElements.RequirementsId} {CsvUploadDomElements.ErrorCallToActionId} {CsvUploadDomElements.ErrorDetailsId}"
        : CsvUploadDomElements.RequirementsId;

    public record TemplateDownloadViewModel
    {
        public required string Url { get; init; }
        public required string LinkText { get; init; }
    }

    public record ErrorsViewModel
    {
        public bool HasErrors => HasFileErrors || HasContentErrors;
        public bool HasFileErrors => FileErrors.Count > 0;
        public bool HasContentErrors => ContentErrors.Count > 0;
        public bool HasMultipleErrors => AllErrors.Count > 1;
        public required IReadOnlyList<string> FileErrors { get; init; }
        public required IReadOnlyList<string> ContentErrors { get; init; }
        public IReadOnlyList<string> AllErrors => [..FileErrors, ..ContentErrors];
    }
}
