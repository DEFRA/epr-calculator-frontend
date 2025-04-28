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

        [EnumMember(Value = "TEST RUN")]
        TESTRUN = 4,

        [EnumMember(Value = "ERROR")]
        ERROR = 5,

        [EnumMember(Value = "DELETED")]
        DELETED = 6,

        [EnumMember(Value = "INITIAL RUN COMPLETED")]
        INITIALRUNCOMPLETED = 7,

        [EnumMember(Value = "INITIAL RUN")]
        INITIALRUN = 8,

        [EnumMember(Value = "INTERIM RE-CALCULATION RUN")]
        INTERIMRECALCULATIONRUN = 9,

        [EnumMember(Value = "FINAL RUN")]
        FINALRUN = 10,

        [EnumMember(Value = "FINAL RE-CALCULATION RUN")]
        FINALRECALCULATIONRUN = 11,
    }
}
