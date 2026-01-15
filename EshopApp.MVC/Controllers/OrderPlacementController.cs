using EshopApp.MVC.ControllerUtilities;
using Microsoft.AspNetCore.Mvc;

namespace EshopApp.MVC.Controllers;

public class OrderPlacementController : Controller
{
    private readonly ILogger<OrderPlacementController> _logger;
    private readonly HttpClient httpClient;

    public OrderPlacementController(IHttpClientFactory httpClientFactory, ILogger<OrderPlacementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    public IActionResult CustomerAccountTypeSelection()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
            ViewData["ShouldSynchronizeCart"] = true;
        return View();
    }

    public IActionResult CustomerInformation()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
            ViewData["ShouldSynchronizeCart"] = true;
        return View();
    }

    public IActionResult OrderInformation()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
            ViewData["ShouldSynchronizeCart"] = true;
        return View();
    }

    public IActionResult OrderFinalization()
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (HelperMethods.BasicTokenValidation(Request))
            ViewData["ShouldSynchronizeCart"] = true;
        return View();
    }
}
