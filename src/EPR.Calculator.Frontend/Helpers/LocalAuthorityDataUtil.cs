using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Helpers
{
    /// <summary>
    /// Utility class for handling Local Authority data transformations.
    /// </summary>
    public static class LocalAuthorityDataUtil
    {
        /// <summary>
        /// Retrieves and formats local authority data based on disposal costs and material type.
        /// </summary>
        /// <param name="localAuthorityDisposalCosts">A list of disposal costs for local authorities.</param>
        /// <param name="matertialType">The material type to be prioritized in the formatted data.</param>
        /// <returns>
        /// A list of <see cref="LocalAuthorityViewModel"/> objects, grouped by country and with the specified material type prioritized.
        /// Returns null if the input list is null.
        /// </returns>
        public static List<LocalAuthorityViewModel>? GetLocalAuthorityData(List<LocalAuthorityDisposalCost> localAuthorityDisposalCosts, string matertialType)
        {
            if (localAuthorityDisposalCosts == null)
            {
                return null;
            }

            var localAuthorityData = localAuthorityDisposalCosts
                .Select(la => new LocalAuthorityViewModel(la))
                .ToList();

            var formattedLocalAuthorityData = localAuthorityData
                .GroupBy(la => la.Country)
                .SelectMany(group =>
                {
                    var items = group.ToList();
                    var otherItem = items.Find(item => item.Material == matertialType);
                    if (otherItem != null)
                    {
                        items.Remove(otherItem);
                        items.Add(otherItem);
                    }

                    return items;
                })
                .ToList();

            return formattedLocalAuthorityData;
        }
    }
}
