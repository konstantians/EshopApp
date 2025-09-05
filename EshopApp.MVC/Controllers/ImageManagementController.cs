using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EshopApp.MVC.Controllers;

public class ImageManagementController : Controller
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageManagementController> _logger;
    private readonly HttpClient httpClient;

    public ImageManagementController(IWebHostEnvironment env, IHttpClientFactory httpClientFactory, ILogger<ImageManagementController> logger)
    {
        httpClient = httpClientFactory.CreateClient("GatewayApiClient");
        _env = env;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile)
    {
        string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
        if (!HelperMethods.BasicTokenValidation(Request))
        {
            Response.Cookies.Delete("EshopAppAuthenticationCookie");
            return RedirectToAction("SignInAndSignUp", "Account");
        }

        if (imageFile == null || imageFile.Length == 0)
            return BadRequest("No file selected");

        // Decide where to save
        var uploadsFolder = Path.Combine(_env.WebRootPath, "DynamicImages");
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        // Generate unique filename
        var random = Path.GetRandomFileName().Replace(".", ""); // 11 chars
        var uniqueFileName = $"{random}_{Path.GetFileName(imageFile.FileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("gatewayImage", new { Name = Path.GetFileNameWithoutExtension(imageFile.FileName), ImagePath = uniqueFileName });
        string? responseBody = await response.Content.ReadAsStringAsync();
        UiImage? image = JsonSerializer.Deserialize<UiImage>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var validationResult = await HelperMethods.CommonErrorValidation(this, _logger, response, null, "CreateProduct", "productManagement", shouldRedirect: false);
        if (validationResult is not null)
        {
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            return validationResult;
        }

        return Ok(new { ImageId = image!.Id });
    }
}
