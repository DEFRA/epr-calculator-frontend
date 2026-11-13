using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels.CsvUpload;

public record CsvUploadSuccessViewModel
{
    public required RelativeYear RelativeYear { get; init; }
    public required string Title { get; init; }
    public required string ViewLinkUrl { get ; set ; }
    public required string ViewLinkText { get ; set ; }
}
