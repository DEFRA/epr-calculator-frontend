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
        /// Converts a list of LocalAuthorityDisposalCost objects into a list of LocalAuthorityViewModel objects.
        /// </summary>
        /// <param name="localAuthorityDisposalCosts">A list of LocalAuthorityDisposalCost objects to be converted.</param>
        /// <returns>A list of LocalAuthorityViewModel objects.</returns>
        public static List<LocalAuthorityViewModel>? GetLocalAuthorityData(List<LocalAuthorityDisposalCost> localAuthorityDisposalCosts)
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
                    var otherItem = items.Find(item => item.Material == MaterialTypes.Other);
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
