namespace EPR.Calculator.Frontend.ViewModels
{
    public record OrganisationSelectionsViewModel
    {
        public List<int> SelectedOrganisationIds { get; init; } = new();
    }
}
