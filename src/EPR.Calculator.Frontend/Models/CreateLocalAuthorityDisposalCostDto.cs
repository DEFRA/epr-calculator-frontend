namespace EPR.Calculator.Frontend.Models
{
    public class CreateLocalAuthorityDisposalCostDto
    {
        public string ParameterYear { get; set; }

        public IEnumerable<LocalAuthorityDisposalCostDto> DisposalCosts { get; set; }
    }
}
