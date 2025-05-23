using System.ComponentModel;

namespace EPR.Calculator.Frontend.Enums
{
    public enum BillingStatus
    {
        [Description("No Action")]
        Noaction = 0,

        [Description("Accepted")]
        Accepted = 1,

        [Description("Rejected")]
        Rejected = 2,

        [Description("Pending")]
        Pending = 3,
    }
}
