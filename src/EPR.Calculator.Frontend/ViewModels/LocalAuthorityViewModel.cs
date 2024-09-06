﻿using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model to display the local authority disposal costs.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LocalAuthorityViewModel
    {
        public LocalAuthorityViewModel(LocalAuthorityDisposalCost localAuthorityDisposalCost)
        {
            this.Country = GetCountryDescription(localAuthorityDisposalCost.Country);
            this.Material = localAuthorityDisposalCost.Material;
            this.TotalCost = localAuthorityDisposalCost.TotalCost;
            this.CreatedBy = localAuthorityDisposalCost.CreatedBy;
            this.CreatedAt = GetFormattedCreatedAt(localAuthorityDisposalCost.CreatedAt);
            this.EffectiveFrom = localAuthorityDisposalCost.EffectiveFrom;
        }

        public string Country { get; set; }

        public string Material { get; set; }

        public decimal TotalCost { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedAt { get; set; }

        public DateTime EffectiveFrom { get; set; }

        private static string GetCountryDescription(string country)
        {
            return typeof(Country).GetTypeInfo().DeclaredMembers.SingleOrDefault(x => x.Name == country)?.GetCustomAttribute<EnumMemberAttribute>(false)?.Value;
        }

        private static string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new System.Globalization.CultureInfo("en-GB"));
        }
    }
}
