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
        TEST_RUN = 4,

        [EnumMember(Value = "ERROR")]
        ERROR = 5,

        [EnumMember(Value = "DELETED")]
        DELETED = 6,

        [EnumMember(Value = "INITIAL RUN COMPLETED")]
        INITIAL_RUN_COMPLETED = 7,

        [EnumMember(Value = "INITIAL RUN")]
        INITIAL_RUN = 8,

        [EnumMember(Value = "INTERIM RE-CALCULATION RUN")]
        INTERIM_RECALCULATION_RUN = 9,

        [EnumMember(Value = "FINAL RUN")]
        FINAL_RUN = 10,

        [EnumMember(Value = "FINAL RE-CALCULATION RUN")]
        FINAL_RECALCULATION_RUN = 11,
    }
}
