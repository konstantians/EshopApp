using EshopApp.MVC.ControllerUtilities;
using EshopApp.MVC.Models;
using EshopApp.MVC.Models.DataModels;
using EshopApp.MVC.ViewModels.CartViewModels;
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

        public IActionResult Index()
        {
            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
            if (HelperMethods.BasicTokenValidation(Request))
                ViewData["ShouldSynchronizeCart"] = true;
            return View();
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

            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
            if (HelperMethods.BasicTokenValidation(Request))
                ViewData["ShouldSynchronizeCart"] = true;
            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> ViewCart()
        {
            string? accessToken = Request.Cookies["EshopAppAuthenticationCookie"];
            if (!HelperMethods.BasicTokenValidation(Request))
            {
                Response.Cookies.Delete("EshopAppAuthenticationCookie");
                return View(null);
            }

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await httpClient.GetAsync("GatewayAuthentication/GetUserByAccessToken?includeCart=true");

            //this deals with 5xx errors
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return View("Error500");
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                return View("Error503");
            else if ((int)response.StatusCode >= 500)
                return View("Error");

            else if ((int)response.StatusCode >= 400)
            {
                Response.Cookies.Delete("EshopAppAuthenticationCookie");
                return RedirectToAction("SignInAndSignUp", "Account");
            }

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

        [HttpPost]
        public IActionResult AddItemToCart(AddItemToCartViewModel addItemToCartViewModel)
        {
            //this will just add the product to the cart. Every time the user goes to a page his cart will simply be loaded, but when I add a product to the cart I don't need to do that immediatelly, because I am handling it from the front end
            //so here the only thing that needs to happen is adding the item to the cart and then return failure or success
            return Json(new { success = true });
        }
    }
}
