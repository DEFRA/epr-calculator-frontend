using System.ComponentModel;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Enums
{
    public enum RunClassification
    {
        /// <summary>
        /// In the Queue.
        /// </summary>
        [Description("IN THE QUEUE")]
        INTHEQUEUE = 1,

        /// <summary>
        /// Running.
        /// </summary>
        [Description("RUNNING")]
        RUNNING = 2,

        /// <summary>
        /// Unclassified.
        /// </summary>
        [Description("UNCLASSIFIED")]
        UNCLASSIFIED = 3,

        [EnumMember(Value = "TEST RUN")]
        TEST_RUN = 4,

        /// <summary>
        /// Error.
        /// </summary>
        [Description("ERROR")]
        ERROR = 5,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("DELETED")]
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