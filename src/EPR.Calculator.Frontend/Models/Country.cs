using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Models
{
    public enum Country
    {
        [EnumMember(Value = "United Kingdom")]
        UnitedKingdom = 0,

        [EnumMember(Value = "England")]
        England = 1,

        [EnumMember(Value = "Wales")]
        Wales = 2,

        [EnumMember(Value = "Scotland")]
        Scotland = 3,

        [EnumMember(Value = "Northern Ireland")]
        NI = 4,
    }
}
