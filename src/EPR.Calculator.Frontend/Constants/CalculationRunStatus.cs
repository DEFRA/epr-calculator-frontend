using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Constants
{
    [ExcludeFromCodeCoverage]
    public static class CalculationRunStatus
    {
        public const string InTheQueue = "IN THE QUEUE";
        public const string Running = "RUNNING";
        public const string Play = "PLAY";
        public const string Unclassified = "UNCLASSIFIED";
        public const string Error = "ERROR";
        public const string Deleted = "DELETED";
    }
}
