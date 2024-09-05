using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers
{
    public class LocalAuthorityDisposalCostsController : Controller
    {
        public IActionResult Index()
        {
            var localAuthorityData = this.GetLocalAuthorityData();
            var localAuthorityDataGroupedByCountry = localAuthorityData?.GroupBy((data) => data.Country).ToList();

            return this.View(ViewNames.LocalAuthorityDisposalCostsIndex, localAuthorityDataGroupedByCountry);
        }

        private List<LocalAuthorityViewModel> GetLocalAuthorityData()
        {
            var localAuthorityDisposalCosts = this.GetLocalAuthorityDisposalCosts();

            var localAuthorityData = new List<LocalAuthorityViewModel>();

            if (localAuthorityDisposalCosts.Count > 0)
            {
                foreach (var la in localAuthorityDisposalCosts)
                {
                    localAuthorityData.Add(new LocalAuthorityViewModel(la));
                }
            }

            return localAuthorityData;
        }

        // TO DO: The below function will be deleted during the integration with GET API
        private List<LocalAuthorityDisposalCost> GetLocalAuthorityDisposalCosts()
        {
            var localAuthorityDisposalCosts = new List<LocalAuthorityDisposalCost>();

            localAuthorityDisposalCosts.AddRange([
                new LocalAuthorityDisposalCost { Id = 1, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-AL", Country = "England", Material = "Aluminium", TotalCost = 2210.45m },
                new LocalAuthorityDisposalCost { Id = 2, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-FC", Country = "England", Material = "Fibre composite", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 3, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-GL", Country = "England", Material = "Glass", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 4, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-PC", Country = "England", Material = "Paper or card", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 5, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-PL", Country = "England", Material = "Plastic", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 6, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-ST", Country = "England", Material = "Steel", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 7, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-WD", Country = "England", Material = "Wood", TotalCost = 2210m },
                new LocalAuthorityDisposalCost { Id = 8, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "ENG-OT", Country = "England", Material = "Other", TotalCost = 0m },
                new LocalAuthorityDisposalCost { Id = 9, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-AL", Country = "NI", Material = "Aluminium", TotalCost = 10m },
                new LocalAuthorityDisposalCost { Id = 10, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-FC", Country = "NI", Material = "Fibre composite", TotalCost = 11m },
                new LocalAuthorityDisposalCost { Id = 11, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-GL", Country = "NI", Material = "Glass", TotalCost = 12m },
                new LocalAuthorityDisposalCost { Id = 12, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-PC", Country = "NI", Material = "Paper or card", TotalCost = 13m },
                new LocalAuthorityDisposalCost { Id = 13, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-PL", Country = "NI", Material = "Plastic", TotalCost = 14m },
                new LocalAuthorityDisposalCost { Id = 14, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-ST", Country = "NI", Material = "Steel", TotalCost = 15m },
                new LocalAuthorityDisposalCost { Id = 15, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-WD", Country = "NI", Material = "Wood", TotalCost = 16m },
                new LocalAuthorityDisposalCost { Id = 16, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "NI-OT", Country = "NI", Material = "Other", TotalCost = 17m },
                new LocalAuthorityDisposalCost { Id = 17, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-AL", Country = "Scotland", Material = "Aluminium", TotalCost = 20.01m },
                new LocalAuthorityDisposalCost { Id = 18, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-FC", Country = "Scotland", Material = "Fibre composite", TotalCost = 21.01m },
                new LocalAuthorityDisposalCost { Id = 19, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-GL", Country = "Scotland", Material = "Glass", TotalCost = 22.01m },
                new LocalAuthorityDisposalCost { Id = 20, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-PC", Country = "Scotland", Material = "Paper or card", TotalCost = 23.01m },
                new LocalAuthorityDisposalCost { Id = 21, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-PL", Country = "Scotland", Material = "Plastic", TotalCost = 24.01m },
                new LocalAuthorityDisposalCost { Id = 22, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-ST", Country = "Scotland", Material = "Steel", TotalCost = 25.01m },
                new LocalAuthorityDisposalCost { Id = 23, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-WD", Country = "Scotland", Material = "Wood", TotalCost = 26.01m },
                new LocalAuthorityDisposalCost { Id = 24, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "SCT-OT", Country = "Scotland", Material = "Other", TotalCost = 27.01m },
                new LocalAuthorityDisposalCost { Id = 25, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-AL", Country = "Wales", Material = "Aluminium", TotalCost = 30.01m },
                new LocalAuthorityDisposalCost { Id = 26, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-FC", Country = "Wales", Material = "Fibre composite", TotalCost = 30.02m },
                new LocalAuthorityDisposalCost { Id = 27, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-GL", Country = "Wales", Material = "Glass", TotalCost = 30.03m },
                new LocalAuthorityDisposalCost { Id = 28, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-PC", Country = "Wales", Material = "Paper or card", TotalCost = 30.04m },
                new LocalAuthorityDisposalCost { Id = 29, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-PL", Country = "Wales", Material = "Plastic", TotalCost = 30.05m },
                new LocalAuthorityDisposalCost { Id = 30, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-ST", Country = "Wales", Material = "Steel", TotalCost = 30.06m },
                new LocalAuthorityDisposalCost { Id = 31, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-WD", Country = "Wales", Material = "Wood", TotalCost = 30.07m },
                new LocalAuthorityDisposalCost { Id = 32, ParameterYear = "2024-25", EffectiveFrom = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), CreatedBy = "Test User", CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc), LapcapDataMasterId = 1, LapcapDataTemplateMasterUniqueRef = "WLS-OT", Country = "Wales", Material = "Other", TotalCost = 30.08m },
            ]);

            return localAuthorityDisposalCosts;
        }
    }
}
