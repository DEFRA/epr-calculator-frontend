namespace EPR.Calculator.Frontend.ViewModels.CsvUpload;

public record CsvUploadViewModel : IFileUploadViewModel
{
    public required string Title { get; init; }
    public required string BackLinkUrl { get; init; }
    public TemplateDownload? Template { get; init; }
    public bool HasDownloadableTemplate => Template != null;
    public bool HasErrors => ErrorsViewModel?.HasErrors ?? false;
    public FileUploadErrorViewModel? ErrorsViewModel { get; init; }
    public string InputId => DomElements.InputId;
    public string RequirementsId => DomElements.RequirementsId;
    public string ErrorCallToActionId => HasErrors ? ErrorsViewModel!.CallToActionId : string.Empty;
    public string ErrorDetailsId => HasErrors ? ErrorsViewModel!.DetailsId : string.Empty;

    public string InputDescribedBy => HasErrors
        ? $"{ErrorCallToActionId} {ErrorDetailsId}"
        : RequirementsId;

    public static class DomElements
    {
        public const string InputId = "file-upload-input";
        public const string RequirementsId = "file-upload-requirements";
        public const string ErrorDetailsId = "file-upload-error-details";
        public const string ErrorCallToActionId = "file-upload-error-action";
    }

    public record TemplateDownload
    {
        public required string Url { get; init; }
        public required string LinkText { get; init; }
    }
}
