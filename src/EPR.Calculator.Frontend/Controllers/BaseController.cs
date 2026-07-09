using EPR.Calculator.Frontend.Constants;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Controllers;

public class BaseController : Controller
{
    protected string GetBackLink()
    {
        var referrer = Request.Headers.Referer.ToString();

        if (string.IsNullOrEmpty(referrer))
            return string.Empty;

        try
        {
            var uri = new Uri(referrer);
            var absolutePath = uri.AbsolutePath;
            if (absolutePath == "/")
                return ControllerNames.Dashboard;

            var segments = absolutePath.TrimEnd('/').Split('/');

            if (segments.Length >= 2)
                return segments[^2];
        }
        catch (UriFormatException)
        {
            return string.Empty;
        }

        return string.Empty;
    }

    protected RedirectToActionResult RedirectToError()
    {
        return RedirectToAction("Index", "StandardError");
    }
}
