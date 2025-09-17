using EPR.Calculator.Frontend.Constants;
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
        public string CurrentUser { get; init; }

        /// <summary>
        /// Gets or sets the Back Link Partial View Model.
        /// </summary>
        public BackLinkViewModel? BackLinkViewModel { get; set; }
    }
}