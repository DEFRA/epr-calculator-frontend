using System.ComponentModel;

namespace EPR.Calculator.Frontend.Enums
{
    public enum BillingInstruction
    {
        [Description("No Action")]
        Noaction = 0,

        [Description("Initial")]
        Initial = 1,

        [Description("Delta")]
        Delta = 2,

        [Description("Rebill")]
        Rebill = 3,

        [Description("Cancel Bill")]
        Cancel = 4,
    }
}
