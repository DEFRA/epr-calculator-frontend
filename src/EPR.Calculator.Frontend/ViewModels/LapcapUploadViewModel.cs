namespace EPR.Calculator.Frontend.ViewModels;

public record LapcapUploadViewModel : IFileUploadViewModel
{
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
}
