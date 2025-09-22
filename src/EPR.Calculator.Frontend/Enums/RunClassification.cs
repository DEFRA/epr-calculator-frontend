using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Enums
{
    /// <summary>
    /// The classification of a calculation run.
    /// </summary>
    public enum RunClassification
    {
        [Display(Name = "None")]
        [Description("None")]
        [EnumMember(Value = "None")]
        None = 0,

        /// <summary>
        /// IN THE QUEUE.
        /// </summary>
        [Display(Name = "In the queue")]
        [Description("In the Queue")]
        [EnumMember(Value = "IN THE QUEUE")]
        QUEUE = 1,

        /// <summary>
        /// RUNNING.
        /// </summary>
        [Display(Name = "Running")]
        [Description("RUNNING")]
        [EnumMember(Value = "RUNNING")]
        RUNNING = 2,

        /// <summary>
        /// READY FOR CLASSIFICATION.
        /// </summary>
        [Display(Name = "Ready for classification")]
        [Description("READY FOR CLASSIFICATION")]
        [EnumMember(Value = "UNCLASSIFIED")]
        UNCLASSIFIED = 3,

        /// <summary>
        /// TEST RUN.
        /// </summary>
        [Display(Name = "Test run")]
        [Description("TEST RUN")]
        [EnumMember(Value = "TEST RUN")]
        TEST_RUN = 4,

        /// <summary>
        /// ERROR.
        /// </summary>
        [Display(Name = "Error")]
        [Description("ERROR")]
        [EnumMember(Value = "ERROR")]
        ERROR = 5,

        /// <summary>
        /// DELETED.
        /// </summary>
        [Display(Name = "Deleted")]
        [Description("DELETED")]
        [EnumMember(Value = "DELETED")]
        DELETED = 6,

        /// <summary>
        /// INITIAL RUN COMPLETED.
        /// </summary>
        [Display(Name = "Initial run completed")]
        [Description("INITIAL RUN COMPLETED")]
        [EnumMember(Value = "INITIAL RUN COMPLETED")]
        INITIAL_RUN_COMPLETED = 7,

        /// <summary>
        /// INITIAL RUN CLASSIFIED.
        /// </summary>
        [Display(Name = "Initial run classified")]
        [Description("INITIAL RUN CLASSIFIED")]
        [EnumMember(Value = "INITIAL RUN")]
        INITIAL_RUN = 8,

        /// <summary>
        /// INTERIM RECALCULATION RUN CLASSIFIED.
        /// </summary>
        [Display(Name = "Interim re-calculation run classified")]
        [Description("INTERIM RECALCULATION RUN CLASSIFIED")]
        [EnumMember(Value = "INTERIM RE-CALCULATION RUN")]
        INTERIM_RECALCULATION_RUN = 9,

        /// <summary>
        /// FINAL RUN CLASSIFIED.
        /// </summary>
        [Display(Name = "Final run classified")]
        [Description("FINAL RUN CLASSIFIED")]
        [EnumMember(Value = "FINAL RUN")]
        FINAL_RUN = 10,

        /// <summary>
        /// FINAL RECALCULATION RUN CLASSIFIED.
        /// </summary>
        [Display(Name = "Final re-calculation run classified")]
        [Description("FINAL RECALCULATION RUN CLASSIFIED")]
        [EnumMember(Value = "FINAL RE-CALCULATION RUN")]
        FINAL_RECALCULATION_RUN = 11,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN COMPLETED.
        /// </summary>
        [Display(Name = "Interim re-calculation run completed")]
        [Description("INTERIM RE-CALCULATION RUN COMPLETED")]
        [EnumMember(Value = "INTERIM RE-CALCULATION RUN COMPLETED")]
        INTERIM_RECALCULATION_RUN_COMPLETED = 12,

        /// <summary>
        /// FINAL RE-CALCULATION RUN COMPLETED.
        /// </summary>
        [Display(Name = "Final re-calculation run completed")]
        [Description("FINAL RE-CALCULATION RUN COMPLETED")]
        [EnumMember(Value = "FINAL RE-CALCULATION RUN COMPLETED")]
        FINAL_RECALCULATION_RUN_COMPLETED = 13,

        /// <summary>
        /// FINAL RUN COMPLETED.
        /// </summary>
        [Display(Name = "Final run completed")]
        [Description("FINAL RUN COMPLETED")]
        [EnumMember(Value = "FINAL RUN COMPLETED")]
        FINAL_RUN_COMPLETED = 14,
    }
}