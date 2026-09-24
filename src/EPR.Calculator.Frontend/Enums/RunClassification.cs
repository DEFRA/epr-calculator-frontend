using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace EPR.Calculator.Frontend.Enums;

/// <summary>
///     The classification of a calculation run.
/// </summary>
public enum RunClassification
{
    [Display(Name = "None")] [Description("None")] [EnumMember(Value = "None")]
    None = 0,

    [Display(Name = "Running")] [Description("RUNNING")] [EnumMember(Value = "RUNNING")]
    Running = 2,

    [Display(Name = "Ready for classification")] [Description("READY FOR CLASSIFICATION")] [EnumMember(Value = "UNCLASSIFIED")]
    Unclassified = 3,

    [Display(Name = "Test run")] [Description("TEST RUN")] [EnumMember(Value = "TEST RUN")]
    Test = 4,

    [Display(Name = "Error")] [Description("ERROR")] [EnumMember(Value = "ERROR")]
    Errored = 5,

    [Display(Name = "Deleted")] [Description("DELETED")] [EnumMember(Value = "DELETED")]
    Deleted = 6,

    [Display(Name = "Initial run completed")] [Description("INITIAL RUN COMPLETED")] [EnumMember(Value = "INITIAL RUN COMPLETED")]
    InitialCompleted = 7,

    [Display(Name = "Initial run classified")] [Description("INITIAL RUN CLASSIFIED")] [EnumMember(Value = "INITIAL RUN")]
    Initial = 8,

    [Display(Name = "Re-calculation run classified")] [Description("RECALCULATION RUN CLASSIFIED")] [EnumMember(Value = "RE-CALCULATION RUN")]
    Recalculation = 9,

    [Display(Name = "Re-calculation run completed")] [Description("RE-CALCULATION RUN COMPLETED")] [EnumMember(Value = "RE-CALCULATION RUN COMPLETED")]
    RecalculationCompleted = 12
}

[ExcludeFromCodeCoverage]
public static class RunClassificationHelper
{
    /// <summary>
    ///     Completed classifications are all <see cref="OfficialClassifications">Official</see> classifications that
    ///     have had a Billing File generated and sent to FSS.
    /// </summary>
    public static readonly ImmutableHashSet<RunClassification> CompletedClassifications =
    [
        RunClassification.InitialCompleted,
        RunClassification.RecalculationCompleted
    ];

    /// <summary>
    ///     Official classifications are all non-<see cref="RunClassification.Test">Test</see> classifications that
    ///     may have been designated by a user.
    /// </summary>
    /// <remarks>
    ///     This includes Completed Official classifications.
    /// </remarks>
    public static readonly ImmutableHashSet<RunClassification> OfficialClassifications =
    [
        RunClassification.Initial,
        RunClassification.Recalculation,
        .. CompletedClassifications
    ];

    extension(RunClassification classification)
    {
        /// <summary>
        ///     Has the run been given an <see cref="OfficialClassifications">Official</see> classification?
        /// </summary>
        public bool IsOfficial => OfficialClassifications.Contains(classification);

        /// <summary>
        ///     Has the run been sent to FSS?
        /// </summary>
        public bool IsCompleted => CompletedClassifications.Contains(classification);

        public string DisplayLabel => classification switch
        {
            RunClassification.Test => "Test run",
            RunClassification.Initial => "Initial run",
            RunClassification.InitialCompleted => "Initial run completed",
            RunClassification.Recalculation => "Recalculation run",
            RunClassification.RecalculationCompleted => "Recalculation run completed",
            _ => classification.ToString()
        };

        public string Description => classification switch
        {
            RunClassification.Test =>
                "An unofficial run to view the calculation results without generating a billing file for invoicing.",
            RunClassification.Initial or RunClassification.InitialCompleted =>
                "The first official mandatory run of the financial year, used as the baseline for all future recalculations. " +
                "This run generates an initial billing file for invoicing.",
            RunClassification.Recalculation or RunClassification.RecalculationCompleted =>
                "An official, optional run that can happen any time after the initial run. It can be run multiple times " +
                "and generate a billing file. It can be used to process late or updated producer data.",
            _ => classification.ToString()
        };
    }
}
