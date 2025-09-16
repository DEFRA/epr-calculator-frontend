using EPR.Calculator.Frontend.Constants;
using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record SendBillingFileViewModel : ViewModelCommonData
    {
        public required int RunId { get; set; }

        public required string CalcRunName { get; set; }

        public string SendBillFileHeading { get; set; } = CommonConstants.SendBillingFile;

        public string ConfirmationContent { get; set; } = CommonConstants.ConfirmationContent;

        public string WarningContent { get; set; } = CommonConstants.WarningContent;

        [Required(ErrorMessage = "You must confirm that you’ve checked the billing file before sending it.")]
        [Display(Name = "ConfirmSend")]
        public bool? ConfirmSend { get; set; }

        public bool IsBillingFileLatest { get; set; } = true;
    }
}
