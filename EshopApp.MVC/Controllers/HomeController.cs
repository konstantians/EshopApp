using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models;
using EshopApp.MVC.Models.AuthModels;
using EshopApp.MVC.Models.DataModels;
using EshopApp.MVC.Models.EmailModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
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

        public async Task<IActionResult> Index()
        {
            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];

            if (HelperMethods.BasicTokenValidation(Request))
                ViewData["ShouldSynchronizeCart"] = true;

            var responseCategory = await httpClient.GetAsync("GatewayCategory/Amount/10000");
            var responseProduct = await httpClient.GetAsync("GatewayProduct/Amount/10000/includeDeactivated/false");

            if (responseCategory.StatusCode == HttpStatusCode.InternalServerError || responseProduct.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (responseCategory.StatusCode == HttpStatusCode.ServiceUnavailable || responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)responseCategory.StatusCode >= 500 || (int)responseProduct.StatusCode >= 500)
                return View("Error");

            var responseBody = await responseProduct.Content.ReadAsStringAsync();
            List<UiProduct> products = JsonSerializer.Deserialize<List<UiProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            foreach (var product in products)
            {
                product.Variants?.RemoveAll(variant => variant.IsDeactivated == true || variant.UnitsInStock <= 0);
                if (product.Variants is null || product.Variants.Count == 0)
                    continue;

                UiVariant? thumbnailVariant = product.Variants.FirstOrDefault(variant => variant.IsThumbnailVariant == true);
                if (thumbnailVariant == null)
                    thumbnailVariant = product.Variants[0];

                thumbnailVariant.VariantImages = thumbnailVariant.VariantImages
                .OrderByDescending(variantImage => variantImage.IsThumbNail)
                .ToList();

                product.Variants = new List<UiVariant> { thumbnailVariant };
            }

            products.RemoveAll(product => product.Variants is null || product.Variants.Count == 0);

            responseBody = await responseCategory.Content.ReadAsStringAsync();
            List<UiCategory> categories = JsonSerializer.Deserialize<List<UiCategory>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            List<UiCategory> remainingCategories = new List<UiCategory>();
            foreach (var category in categories)
            {
                if (category.Products == null)
                    continue;

                List<UiProduct> remainingProducts = new List<UiProduct>();
                foreach (var product in category.Products)
                {
                    product.Variants?.RemoveAll(variant => variant.IsDeactivated == true || variant.UnitsInStock <= 0);
                    if (product.IsDeactivated != true && product.Variants != null && product.Variants.Count > 0)
                        remainingProducts.Add(product);
                }

                if (remainingProducts.Count > 0)
                {
                    category.Products = remainingProducts;
                    remainingCategories.Add(category);
                }
            }

            ViewData["ChosenProducts"] = products?.OrderByDescending(product => product.CreatedAt).Take(4).ToList();
            ViewData["Categories"] = remainingCategories.OrderByDescending(category => category.Products.Count).Take(4).ToList();
            ViewData["RenderFullWidth"] = true;
            return View(new ContactEmailModel());
        }

        [HttpGet]
        public async Task<IActionResult> ViewItems(string? category)
        {
            var responseProduct = await httpClient.GetAsync("GatewayProduct/Amount/10000/includeDeactivated/false");

            if (responseProduct.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)responseProduct.StatusCode >= 500)
                return View("Error");

            string responseBody = await responseProduct.Content.ReadAsStringAsync();
            List<UiProduct> products = JsonSerializer.Deserialize<List<UiProduct>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            foreach (var product in products)
            {
                product.Variants?.RemoveAll(variant => variant.IsDeactivated == true || variant.UnitsInStock <= 0);
                if (product.Variants is null || product.Variants.Count == 0)
                    continue;

                UiVariant? thumbnailVariant = product.Variants.FirstOrDefault(variant => variant.IsThumbnailVariant == true);
                if (thumbnailVariant == null)
                    thumbnailVariant = product.Variants[0];

                thumbnailVariant.VariantImages = thumbnailVariant.VariantImages
                .OrderByDescending(variantImage => variantImage.IsThumbNail)
                .ToList();

                product.Variants = new List<UiVariant> { thumbnailVariant };
            }

            products.RemoveAll(product => product.Variants is null || product.Variants.Count == 0);

            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
            if (HelperMethods.BasicTokenValidation(Request))
                ViewData["ShouldSynchronizeCart"] = true;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> ViewItem(string id)
        {
            var responseProduct = await httpClient.GetAsync($"GatewayProduct/{id}/includeDeactivated/false");

            if (responseProduct.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)responseProduct.StatusCode >= 500)
                return View("Error");
            else if (responseProduct.StatusCode == HttpStatusCode.NotFound)
                return RedirectToAction("ProductNotFound404", "Error");

            string responseBody = await responseProduct.Content.ReadAsStringAsync();
            UiProduct product = JsonSerializer.Deserialize<UiProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            product.Variants.RemoveAll(variant => variant.IsDeactivated == true);
            if (product.Variants.Count == 0)
                return RedirectToAction("ProductNotFound404", "Error");

            UiVariant? thumbnailVariant = product.Variants.FirstOrDefault(variant => variant.IsThumbnailVariant == true);
            if (thumbnailVariant == null)
                thumbnailVariant = product.Variants[0];

            thumbnailVariant.VariantImages = thumbnailVariant.VariantImages
            .OrderByDescending(variantImage => variantImage.IsThumbNail)
            .ToList();

            product.Variants = new List<UiVariant> { thumbnailVariant };

            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
            if (HelperMethods.BasicTokenValidation(Request))
                ViewData["ShouldSynchronizeCart"] = true;
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> CheckVariantAvailability(string productId, string variantId, int requestedUnitsInStock)
        {
            var responseProduct = await httpClient.GetAsync($"GatewayProduct/{productId}/includeDeactivated/false");

            if (responseProduct.StatusCode == HttpStatusCode.InternalServerError)
                return StatusCode(500, new { redirectUrl = "/Error/500" });
            else if (responseProduct.StatusCode == HttpStatusCode.ServiceUnavailable)
                return StatusCode(503, new { redirectUrl = "/Error/503" });
            else if ((int)responseProduct.StatusCode >= 500)
                return StatusCode(500, new { redirectUrl = "/Error/500" });
            else if (responseProduct.StatusCode == HttpStatusCode.NotFound)
                return NotFound(new { redirectUrl = Url.Action("ProductNotFound404", "Error") });

            string responseBody = await responseProduct.Content.ReadAsStringAsync();
            UiProduct product = JsonSerializer.Deserialize<UiProduct>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

            UiVariant? productVariant = product.Variants.FirstOrDefault(variant => variant.Id == variantId);
            if (productVariant is null || productVariant.IsDeactivated == true || productVariant.UnitsInStock <= 0)
                return NotFound(new { redirectUrl = Url.Action("ProductNotFound404", "Error") });

            if (productVariant.UnitsInStock - requestedUnitsInStock < 0)
                return BadRequest(new { errorMessage = "NotEnoughStock" });

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> ViewCart()
        {
            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
            if (string.IsNullOrEmpty(accessToken))
                return View(null);

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

            //this deals with 5xx errors
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)response.StatusCode >= 500)
                return View("Error");

            var responseBody = await response.Content.ReadAsStringAsync();
            UiUser user = JsonSerializer.Deserialize<UiUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            if (user.Cart is null) //there are some users that dont have a cart
                return RedirectToAction("Index", "Home");

            if (HelperMethods.BasicTokenValidation(Request))
                ViewData["ShouldSynchronizeCart"] = true;
            return View(user);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
