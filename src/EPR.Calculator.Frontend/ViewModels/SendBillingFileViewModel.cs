namespace EPR.Calculator.Frontend.ViewModels
{
    public record SendBillingFileViewModel : ViewModelCommonData
    {
        public required int RunId { get; set; }

        public required string CalcRunName { get; set; }

        public required string SendBillFileHeading { get; set; }

        public required string ConfirmationContent { get; set; }

        public required string WarningContent { get; set; }
    }
}
