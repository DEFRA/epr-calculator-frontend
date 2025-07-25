namespace EPR.Calculator.Frontend.Constants
{
    public static class CommonConstants
    {
        public const string ShowDetailedError = "ShowDetailedError";

        public const string SendBillingFile = "Send the billing file";
        public const string ConfirmationContent = "You've confirmed all billing instructions. To review the billing file before sending it to the FSS, go back to the previous step. You won’t be able to make changes after sending.";
        public const string WarningContent = "You must confirm that you’ve checked the billing file\r\nbefore sending it.";

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

        public const string AcceptViewText = "accept";
        public const string RejectViewText = "reject";

        public const string BillingTableHeader = "Billing instructions";
        public const int DefaultBlockSize = 3;
        public const int DefaultPageSize = 10;
        public const int DefaultPage = 1;
        public static readonly int[] PageSizeOptions = new[] { 10, 25, 50 };
    }
}
