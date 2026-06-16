using Microsoft.AspNetCore.Mvc;

namespace EshopApp.MVC.Controllers;

public class ErrorController : Controller
{
    [Route("Error/{id}")]
    public IActionResult Error(int id)
    {
        return id switch
        {
            401 => View("Unauthorized401"),
            403 => View("Forbidden403"),
            404 => View("PageNotFound404"),
            500 => View("Error500"),
            503 => View("ServiceUnavailable503"),
            _ => View("PageNotFound404")
        };
    }

    [Route("Error/Unauthorized401")]
    public IActionResult Unauthorized401()
    {
        return View();
    }

    [Route("Error/Forbidden403")]
    public IActionResult Forbidden403()
    {
        return View();
    }

    [Route("Error/ProductNotFound404")]
    public IActionResult ProductNotFound404()
    {
        return View();
    }

    [Route("Error/PageNotFound404")]
    public IActionResult PageNotFound404()
    {
        return View();
    }
}
