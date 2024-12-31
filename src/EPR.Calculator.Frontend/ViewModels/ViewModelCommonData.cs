using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    /// <summary>
    /// Fields that need to be reused by multiple view models.
    /// </summary>
    public record ViewModelCommonData
    {
        /// <summary>
        /// Gets the name of the currently logged in user.
        /// </summary>
        public required string CurrentUser { get; init; }
    }
}
