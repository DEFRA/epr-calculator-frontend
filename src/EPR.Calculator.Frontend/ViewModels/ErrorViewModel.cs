namespace EPR.Calculator.Frontend.ViewModels;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    public string? DOMElementId { get; set; }
    public string? ErrorMessage { get; set; }
}
