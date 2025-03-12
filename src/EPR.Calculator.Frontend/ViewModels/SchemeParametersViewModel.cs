using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    public class SchemeParametersViewModel
    {
        public IEnumerable<DefaultSchemeParameters> DefaultSchemeParameters { get; set; }

        public string SchemeParameterName {  get; set; }

        public bool IsDisplayPrefix {  get; set; }
    }
}
