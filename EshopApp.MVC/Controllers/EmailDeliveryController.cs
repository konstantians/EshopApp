using EshopApp.MVC.Models.EmailModels;
using Microsoft.AspNetCore.Mvc;

namespace EshopApp.MVC.Controllers;

public class EmailDeliveryController : Controller
{
    private readonly HttpClient httpClient;
    private readonly ILogger<RoleManagementController> _logger;

    public EmailDeliveryController(IHttpClientFactory httpClientFactory, ILogger<RoleManagementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendContactEmail([FromBody] ContactEmailModel contactEmailModel)
    {
        var response = await httpClient.PostAsJsonAsync("GatewayEmail/ContactEmail", contactEmailModel);
        var statusCode = (int)response.StatusCode;

        if (statusCode >= 500)
            return StatusCode(statusCode, new
            {
                result = statusCode switch
                {
                    500 => "serverError",
                    503 => "serviceError",
                    _ => "unknownServerError"
                }
            });

        if (statusCode != 204)
            return BadRequest(new { result = "contactEmailNotSend" });

        return Ok(new { result = "noError" });
    }
}
