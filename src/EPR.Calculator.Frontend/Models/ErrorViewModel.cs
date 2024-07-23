namespace EPR.Calculator.Frontend.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string? DOMElementId { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
