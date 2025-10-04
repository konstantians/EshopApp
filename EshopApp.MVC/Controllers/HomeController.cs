using EshopApp.MVC.Models;
using EshopApp.MVC.Models.DataModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace EshopApp.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient httpClient;

        public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger)
        {
            httpClient = httpClientFactory.CreateClient("GatewayApiClient");
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ViewItems(string? category)
        {
            var responseProduct = await httpClient.GetAsync("GatewayProduct/Amount/10000/includeDeactivated/false");

            if (responseProduct.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)responseProduct.StatusCode >= 500)
                return View("Error");

            if ((int)responseProduct.StatusCode >= 400)
            {
                Response.Cookies.Delete("EshopAppAuthenticationCookie");
                return RedirectToAction("SignInAndSignUp", "Account");
            }

            string responseBody = await responseProduct.Content.ReadAsStringAsync();
            List<UiProduct> products = JsonSerializer.Deserialize<List<UiProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            foreach (var product in products)
            {
                if (product.Variants is null || product.Variants.Count == 0)
                    continue;

                UiVariant? thumbnailVariant = product.Variants.FirstOrDefault(variant => variant.IsThumbnailVariant == true);
                if (thumbnailVariant == null)
                {
                    thumbnailVariant = product.Variants[0];
                    continue;
                }

                thumbnailVariant.VariantImages = thumbnailVariant.VariantImages
                .OrderByDescending(variantImage => variantImage.IsThumbNail)
                .ToList();

                product.Variants = new List<UiVariant> { thumbnailVariant };
            }

            return View(products);
        }

        public async Task<IActionResult> ViewItem(string id)
        {
            var responseProduct = await httpClient.GetAsync($"GatewayProduct/{id}/includeDeactivated/false");

            if (responseProduct.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)responseProduct.StatusCode >= 500)
                return View("Error");

            if ((int)responseProduct.StatusCode >= 400)
            {
                Response.Cookies.Delete("EshopAppAuthenticationCookie");
                return RedirectToAction("SignInAndSignUp", "Account");
            }

            string responseBody = await responseProduct.Content.ReadAsStringAsync();
            UiProduct product = JsonSerializer.Deserialize<UiProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            UiVariant? thumbnailVariant = product.Variants.FirstOrDefault(variant => variant.IsThumbnailVariant == true);
            if (thumbnailVariant == null)
                thumbnailVariant = product.Variants[0];

            thumbnailVariant.VariantImages = thumbnailVariant.VariantImages
            .OrderByDescending(variantImage => variantImage.IsThumbNail)
            .ToList();

            product.Variants = new List<UiVariant> { thumbnailVariant };

            return View(product);
        }

        public IActionResult ViewCart()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
