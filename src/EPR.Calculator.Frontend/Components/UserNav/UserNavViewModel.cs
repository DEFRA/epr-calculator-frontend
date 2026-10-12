using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.Components.UserNav;

public class UserNavViewModel
{
    public required string CurrentUser { get; init; }
    public string? BackLinkUrl { get; init; }
    public required string BackLinkText { get; init; }
    public RelativeYear? RelativeYear { get; init; }
}
