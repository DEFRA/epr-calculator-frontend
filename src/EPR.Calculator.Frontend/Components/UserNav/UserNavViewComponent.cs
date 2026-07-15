using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.Frontend.Components.UserNav;

public class UserNavViewComponent : ViewComponent
{
    /// <summary>
    ///     Renders the user navigation bar.
    /// </summary>
    /// <param name="backLink">
    ///     Fallback URL the backlink should point to. Also acts as an allowed return target. Used when the referrer
    ///     cannot be resolved, is not a sanctioned origin, or points at the current page.
    /// </param>
    /// <param name="allowedBackLinks">
    ///     Additional sanctioned origins for journeys where the same page can be reached from more than one previous
    ///     page. When the referrer's path matches <paramref name="backLink" /> or one of these, the backlink reflects
    ///     where the user actually came from (preserving any query string).
    /// </param>
    /// <param name="showBackLink">
    ///     <para>Default: <see langword="true" /></para>
    ///     Indicates whether any backlink should be displayed.
    /// </param>
    public IViewComponentResult Invoke(
        string? backLink = null,
        IEnumerable<string>? allowedBackLinks = null,
        bool showBackLink = true)
    {
        var resolvedBackLink = showBackLink ? ResolveBackLinkUrl(backLink, allowedBackLinks) : null;

        var viewModel = new UserNavViewModel
        {
            CurrentUser = CommonUtil.GetUserName(HttpContext),
            BackLinkUrl = resolvedBackLink,
        };

        return View("UserNavView", viewModel);
    }

    private string? ResolveBackLinkUrl(string? backLink, IEnumerable<string>? allowedBackLinks)
    {
        // The explicit fallback plus any additional sanctioned origins.
        var allowed = new[] { backLink }
            .Concat(allowedBackLinks ?? [])
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => url!.Trim())
            .ToList();

        // Prefer the actual previous location when it is a sanctioned, non-self origin,
        // returning the referrer verbatim so filter/pagination state is preserved.
        if (TryGetLocalReferrerPath(out var referrerPath)
            && !IsCurrentPage(referrerPath)
            && allowed.Any(url => PathMatches(url, referrerPath)))
        {
            return referrerPath;
        }

        return allowed.FirstOrDefault();
    }

    private bool TryGetLocalReferrerPath([NotNullWhen(true)] out string? localReferrerPath)
    {
        if (!Uri.TryCreate(Request.Headers.Referer, UriKind.Absolute, out var uri)
            || string.IsNullOrWhiteSpace(uri.PathAndQuery)
            || !HostMatchesCurrentRequest(uri))
        {
            localReferrerPath = null;
            return false;
        }

        localReferrerPath = uri.PathAndQuery.Trim();
        return true;
    }

    private bool HostMatchesCurrentRequest(Uri uri) =>
        !string.IsNullOrWhiteSpace(Request.Host.Host)
        && string.Equals(uri.Host, Request.Host.Host, StringComparison.OrdinalIgnoreCase);

    private bool IsCurrentPage(string referrerPath) =>
        string.Equals(PathOnly(referrerPath), Request.Path.Value, StringComparison.OrdinalIgnoreCase);

    private static bool PathMatches(string candidateUrl, string referrerPath) =>
        string.Equals(PathOnly(candidateUrl), PathOnly(referrerPath), StringComparison.OrdinalIgnoreCase);

    private static string PathOnly(string url) =>
        // NOTE: UriKind.Absolute behaves differently depending on host environment
        // Checking uri.IsAbsoluteUri instead seems consistent.
        Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) && uri.IsAbsoluteUri
            ? uri.AbsolutePath
            : url.Split('?', 2)[0];
}
