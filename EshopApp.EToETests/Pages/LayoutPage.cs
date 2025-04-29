using Microsoft.Playwright;

namespace EshopApp.EToETests.Pages;
internal class LayoutPage
{
    private readonly IPage _page;
    private readonly ILocator _userAccountSection;
    private readonly ILocator _signOutButton;
    public LayoutPage(IPage page)
    {
        _page = page;
        _userAccountSection = _page.GetByTestId("userAccountSection");
        _signOutButton = _page.GetByTestId("signOutButton");
    }

    public async Task LogUserOut()
    {
        await _userAccountSection.HoverAsync();
        await _signOutButton.ClickAsync();
        await Task.Delay(300);
    }
}
