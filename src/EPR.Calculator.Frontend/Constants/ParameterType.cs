using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.Constants
{
    public enum ParameterType
    {
        [Display(Name = "Communication costs by material")]
        CommunicationCostsByMaterial,

        [Display(Name = "Communication costs by country")]
        CommunicationCostsByCountry,

        [Display(Name = "Scheme administrator operating costs")]
        SchemeAdministratorOperatingCosts,

        [Display(Name = "Local authority data preparation costs")]
        LocalAuthorityDataPreparationCosts,

        [Display(Name = "Scheme setup costs")]
        SchemeSetupCosts,

        [Display(Name = "Late reporting tonnage")]
        LateReportingTonnage,

        [Display(Name = "Bad debt provision")]
        BadDebtProvision,

        [Display(Name = "Materiality threshold")]
        MaterialityThreshold,

        [Display(Name = "Tonnage change threshold")]
        TonnageChangeThreshold,
    }
}
