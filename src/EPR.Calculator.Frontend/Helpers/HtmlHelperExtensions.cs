using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace EPR.Calculator.Frontend.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static bool HasErrorFor(this ViewDataDictionary viewData, string propertyName)
        {
            return viewData.ModelState[propertyName]?.Errors.Any() == true;
        }
    }
}
