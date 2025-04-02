namespace EPR.Calculator.Frontend.ViewModels
{
    public record SendBillingFileViewModel : ViewModelCommonData
    {
        public string CalcRunName { get; set; }

        public string SendBillFileHeading { get; set; }

        public string ConfirmationContent { get; set; }

        public string WarningContent { get; set; }

        public string BackLink { get; set; }
    }
}
