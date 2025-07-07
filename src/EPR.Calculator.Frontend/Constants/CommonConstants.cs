namespace EPR.Calculator.Frontend.Constants
{
    public static class CommonConstants
    {
        public const string ShowDetailedError = "ShowDetailedError";

        public const string SendBillingFile = "Send the billing file";
        public const string ConfirmationContent = "You have accepted all billing instructions. Send the billing file to the FSS.";
        public const string WarningContent = "Once sent, you will not be able to amend the billing file.";

        public const string TimeZone = "GMT Standard Time";
        public const string DateFormat = "dd MMM yyyy";
        public const string TimeFormat = "H:mm";
        public const string RunDetailError = "The calculation was unsuccessful";
        public const string InitialRunDescription = "The first official mandatory run of the financial year, used as the baseline for all future recalculations. This run generates an initial billing file for invoicing.";
        public const string TestRunDescription = "An unofficial run to view the calculation results without generating a billing file for invoicing.";

        public const bool IsDraftFileTrue = true;
        public const bool IsDraftFileFalse = false;

        public const bool IsBillingFileTrue = true;
        public const bool IsBillingFileFalse = false;

        public const int DefaultBlockSize = 3;
        public static readonly int[] PageSizeOptions = new[] { 10, 25, 50 };
    }
}
