using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public class LocalAuthorityViewModel
    {
        public LocalAuthorityViewModel(LocalAuthorityDisposalCost localAuthorityDisposalCost)
        {
            Country = localAuthorityDisposalCost.Country;
            Material = localAuthorityDisposalCost.Material;
            TotalCost = localAuthorityDisposalCost.TotalCost;
            CreatedBy = localAuthorityDisposalCost.CreatedBy;
            CreatedAt = GetFormattedCreatedAt(localAuthorityDisposalCost.CreatedAt);
        }

        public string Country { get; set; }

        public string Material { get; set; }

        public decimal TotalCost { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedAt { get; set; }

        private string GetFormattedCreatedAt(DateTime createdAt)
        {
            return createdAt.ToString("dd MMM yyyy ' at 'H:mm", new System.Globalization.CultureInfo("en-GB"));
        }
    }
}
