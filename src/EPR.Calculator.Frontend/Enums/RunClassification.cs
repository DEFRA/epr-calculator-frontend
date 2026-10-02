using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.Enums;

/// <summary>
///     Represents the user-assigned classification of a run.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunClassification
{
    /// <summary>
    ///     Indicates that the enum value itself was unknown/invalid.
    /// </summary>
    /// <remarks>
    ///     This shouldn't be encountered in practice; it means the given enum value was not one of the below.
    ///     This typically occurs when the value was corrupted/omitted during (de)serialization across boundaries.
    /// </remarks>
    Unknown = 0,

    /// <summary>
    ///     Run has yet to be classified; classification can only occur once the run has been completed.
    /// </summary>
    None,

    /// <summary>
    ///     Marks a completed run as a non-Official test run. These are essentially ignored, especially from processes that
    ///     aggregate across a year.
    /// </summary>
    Test,

    /// <summary>
    ///     Marks a completed run as Officially classified. It is only applicable to the first run of the year.
    /// </summary>
    Initial,

    /// <summary>
    ///     Marks a completed run as Officially classified. It is only applicable to any additional runs within a year following
    ///     the <see cref="Initial" /> run.
    /// </summary>
    Recalculation,

    /// <summary>
    ///     Marks a completed run as having been deleted.
    /// </summary>
    Deleted
}

[ExcludeFromCodeCoverage]
public static class RunClassificationHelper
{
    extension(RunClassification classification)
    {
        public string DisplayLabel => classification switch
        {
            RunClassification.None => "Unclassified",
            RunClassification.Initial => "Initial run",
            RunClassification.Recalculation => "Re-calculation run",
            _ => $"{classification} run"
        };

        public string Description => classification switch
        {
            RunClassification.Test =>
                "An unofficial run to view the calculation results without generating a billing file for invoicing.",
            RunClassification.Initial =>
                "The first official mandatory run of the financial year, used as the baseline for all future recalculations. " +
                "This run generates an initial billing file for invoicing.",
            RunClassification.Recalculation =>
                "An official, optional run that can happen any time after the initial run. It can be run multiple times " +
                "and generate a billing file. It can be used to process late or updated producer data.",
            _ => classification.ToString()
        };
    }
}
