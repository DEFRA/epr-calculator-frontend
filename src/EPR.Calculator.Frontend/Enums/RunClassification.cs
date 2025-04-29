using System.ComponentModel;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Enums
{
    public enum RunClassification
    {
        None = 0,

        [Description("In the Queue")]
        [EnumMember(Value = "IN THE QUEUE")]
        QUEUE = 1,

        [Description("RUNNING")]
        [EnumMember(Value = "RUNNING")]
        RUNNING = 2,

        [Description("READY FOR CLASSIFICATION")]
        [EnumMember(Value = "UNCLASSIFIED")]
        UNCLASSIFIED = 3,

        [Description("TEST RUN")]
        [EnumMember(Value = "TEST RUN")]
        TEST_RUN = 4,

        [Description("ERROR")]
        [EnumMember(Value = "ERROR")]
        ERROR = 5,

        [Description("DELETED")]
        [EnumMember(Value = "DELETED")]
        DELETED = 6,

        [Description("INITIAL RUN COMPLETED")]
        [EnumMember(Value = "INITIAL RUN COMPLETED")]
        INITIAL_RUN_COMPLETED = 7,

        [Description("INITIAL RUN CLASSIFIED")]
        [EnumMember(Value = "INITIAL RUN")]
        INITIAL_RUN = 8,

        [Description("INTERIM RECALCULATION RUN CLASSIFIED")]
        [EnumMember(Value = "INTERIM RE-CALCULATION RUN")]
        INTERIM_RECALCULATION_RUN = 9,

        [Description("FINAL RUN CLASSIFIED")]
        [EnumMember(Value = "FINAL RUN")]
        FINAL_RUN = 10,

        [Description("FINAL RECALCULATION RUN CLASSIFIED")]
        [EnumMember(Value = "FINAL RE-CALCULATION RUN")]
        FINAL_RECALCULATION_RUN = 11,
    }
}