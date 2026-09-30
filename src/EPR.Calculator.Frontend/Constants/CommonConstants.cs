namespace EPR.Calculator.Frontend.Constants
{
    public static class CommonConstants
    {
        public const string RelativeYearStartingMonth = "RelativeYearStartingMonth";

        public const string SendBillingFile = "Send the billing file";
        public const string ConfirmationContent = "You've confirmed all billing instructions. To review the billing file before sending it to the FSS, go back to the previous step. You won’t be able to make changes after sending.";
        public const string WarningContent = "You must confirm that you’ve checked the billing file\r\nbefore sending it.";


        public const string TestRunDescription = "An unofficial run to view the calculation results without generating a billing file for invoicing.";
        public const string RecalculationRunDescription = "An official, optional run that can happen any time after the initial run. It can be run multiple times and generate a billing file. It can be used to process late or updated producer data.";

        public const string AcceptViewText = "accept";
        public const string RejectViewText = "reject";

        public const int DefaultBlockSize = 3;
        public const int DefaultPageSize = 10;
        public const int DefaultPage = 1;
        public static readonly int[] PageSizeOptions = new[] { 10, 25, 50 };
    }
}
