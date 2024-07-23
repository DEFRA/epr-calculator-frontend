using EPR.Calculator.Frontend.Contents;

namespace EPR.Calculator.Frontend.Models
{
    public class UploadCSVErrorViewModel
    {
        public string RouteName = UploadCSVError.BackRoute;

        public List<ErrorViewModel> errorViewModels { get; set; }
    }
}
