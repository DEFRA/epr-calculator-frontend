using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Enums
{
    public enum RunClassification
    {
        None = 0,

        [EnumMember(Value = "IN THE QUEUE")]
        QUEUE = 1,

        [EnumMember(Value = "RUNNING")]
        RUNNING = 2,

        [EnumMember(Value = "UNCLASSIFIED")]
        UNCLASSIFIED = 3,

        [EnumMember(Value = "PLAY")]
        PLAY = 4,

        [EnumMember(Value = "ERROR")]
        ERROR = 5,

        [EnumMember(Value = "DELETED")]
        DELETED = 6,
    }
}
