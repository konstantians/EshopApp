using EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.RequestModels;
using EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication.Models.ServiceResponseModels;
using EshopApp.GatewayAPI.AuthMicroService.Models;
using EshopApp.GatewayAPI.DataMicroService.SharedModels;
using EshopApp.GatewayAPI.HelperMethods;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAuthentication;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayAuthenticationController : ControllerBase
{
    //the general idea with redirectUrl is that it can contain a returnUrl that the frontend handler(the endpoint that is specified as the redirectUrl) can use to redirect the user too after its own processing
    private readonly HttpClient authHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;

    public GatewayAuthenticationController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpGet("GetUserByAccessToken")]
    public async Task<IActionResult> GetUserByAccessToken(bool? includeCart, bool? includeCoupons, bool? includeOrders)
    {
        //check that an access token has been supplied, this check is made to avoid unnecessary requests
        if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
            !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

        //request to get the user
        _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
        HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync("Authentication/GetUserByAccessToken")); //this contains retry logic

        if ((int)response.StatusCode >= 400)
            return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

        string? responseBody = await response.Content.ReadAsStringAsync();
        GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);

        if (includeCart.HasValue && includeCart.Value)
        {
            //get the user cart
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Cart/UserId/{appUser!.Id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            GatewayCart? userCart = JsonSerializer.Deserialize<GatewayCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            appUser!.Cart = userCart!;
        }

        if (includeCoupons.HasValue && includeCoupons.Value)
        {
            //get the user coupons
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Coupon/userId/{appUser!.Id}/includeDeactivated/true"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayUserCoupon>? userCoupons = JsonSerializer.Deserialize<List<GatewayUserCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            appUser!.UserCoupons = userCoupons!;
        }

        if (includeOrders.HasValue && includeOrders.Value)
        {
            //get the user orders
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.GetAsync($"Order/Amount/{int.MaxValue}/UserId/{appUser!.Id}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            responseBody = await response.Content.ReadAsStringAsync();
            List<GatewayOrder>? userOrders = JsonSerializer.Deserialize<List<GatewayOrder>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            appUser!.Orders = userOrders!;
        }

        return Ok(appUser);
    }

    //The client Url might other than the handler endpoint might contain as a query parameter something like returnUrl, which can be used
    //by the handler endpoint to redirect back to the whole signup started from. So if the signup started from signup then redirect to home
    //if it started from some part inside the application(due to need authentication) then redirect there instead of the default, which is home
    //I suppose I will leave that for the front end implementation to consider. The only thing that is needed 100% is the domain:port/endpoint to be sent
    //but if the frontend wants it can be domain:port/endpoint?returnUrl=returnUrl
    [HttpPost("SignUp")]
    public async Task<IActionResult> SignUp([FromBody] GatewaySignUpRequestModel signUpModel)
    {
        //TODO if the validation fails then maybe do a rollback for the signup???
        try
        {
            //check the redirect URL
            if (!_utilityMethods.CheckIfUrlIsTrusted(signUpModel.ClientUrl!, _configuration))
                return BadRequest(new { ErrorMessage = "OriginForRedirectUrlIsNotTrusted" });

            //remove the trailing slash from the client url
            if (signUpModel.ClientUrl!.EndsWith("/"))
                signUpModel.ClientUrl = signUpModel.ClientUrl.Substring(0, signUpModel.ClientUrl.Length - 1);

            //start by doing healthchecks for the endpoints this is calling
            if (!await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //sign up user
            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.PostAsJsonAsync("Authentication/SignUp", new { signUpModel.Email, signUpModel.Password, signUpModel.PhoneNumber })); //this contains retry logic

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewaySignUpServiceResponseModel? signupResponseModel = JsonSerializer.Deserialize<GatewaySignUpServiceResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //create cart for user
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.PostAsJsonAsync("cart", new { signupResponseModel!.UserId })); //this contains retry logic

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //send confirmation email
            string message = "Click on the following link to confirm your email:";
            string link = $"{_configuration["AuthApiBaseUrl"]}Authentication/ConfirmEmail?" +
                $"userId={signupResponseModel!.UserId}&confirmEmailToken={WebUtility.UrlEncode(signupResponseModel.ConfirmationToken)}&redirectUrl={WebUtility.UrlEncode(signUpModel.ClientUrl)}";
            string? confirmationLink = $"{message} {link}";
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", signUpModel.Email! },
                { "title", "Email Confirmation" },
                { "message", confirmationLink }
            };
            _ = Task.Run(async () =>
            {
                _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server");
        }
    }

    [HttpPost("SignIn")]
    public async Task<IActionResult> SignIn([FromBody] GatewaySignInRequestModel signInModel)
    {
        try
        {
            //sign in user
            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
                authHttpClient.PostAsJsonAsync("Authentication/SignIn", new { signInModel.Email, signInModel.Password, signInModel.RememberMe }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //return access accessToken
            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("accessToken", out string? accessToken);
            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    //the clientUrl here contains just the front end handler since the reset password endpoint that is need to be called after this requires more information from the front end
    //so the clientUrl is something like this: domain:port/endpoint with this endpoint appending userId and password reset token(so the front end handler needs to include those)
    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] GatewayForgotPasswordRequestModel forgotPasswordModel)
    {
        try
        {
            //check the redirect URL
            if (!_utilityMethods.CheckIfUrlIsTrusted(forgotPasswordModel.ClientUrl!, _configuration))
                return BadRequest(new { ErrorMessage = "OriginForRedirectUrlIsNotTrusted" });

            //remove the trailing slash from the client url
            if (forgotPasswordModel.ClientUrl!.EndsWith("/"))
                forgotPasswordModel.ClientUrl = forgotPasswordModel.ClientUrl.Substring(0, forgotPasswordModel.ClientUrl.Length - 1);

            //start by doing healthchecks for the endpoints this is calling
            if (!await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.PostAsJsonAsync("Authentication/ForgotPassword", new { forgotPasswordModel.Email }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayForgotPasswordServiceResponseModel? forgotPasswordResponseModel = JsonSerializer.Deserialize<GatewayForgotPasswordServiceResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string message = "Click on the following link to change your account's password:";
            string link = $"{forgotPasswordModel.ClientUrl}?userId={forgotPasswordResponseModel!.UserId}&token={WebUtility.UrlEncode(forgotPasswordResponseModel.Token)}";
            string? confirmationLink = $"{message} {link}";
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", forgotPasswordModel.Email! },
                { "title", "Reset Password Confirmation" },
                { "message", confirmationLink }
            };
            _ = Task.Run(async () =>
            {
                _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] GatewayResetPasswordRequestModel resetPasswordModel)
    {
        try
        {
            //request the reset of the password of the user
            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.PostAsJsonAsync("Authentication/ResetPassword",
                new { resetPasswordModel.UserId, resetPasswordModel.Password, resetPasswordModel.Token }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("accessToken", out string? accessToken);
            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] GatewayChangePasswordRequestModel changePasswordModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //request to change the password of the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.PostAsJsonAsync("Authentication/ChangePassword",
                new { changePasswordModel.CurrentPassword, changePasswordModel.NewPassword }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    //the clientUrl in this works the same way as in the signup, which means that the format is something along those lines: domain:port/endpoint now if the front end wants it can be
    //domain:port/endpoint?returnUrl=returnUrl with the front end handler using the returnUrl to redirect the user after everything is done to where this whole process started from
    //(in this case it is probably pointless since it should return them to home no matter what, but who knows)
    [HttpPost("RequestChangeAccountEmail")]
    public async Task<IActionResult> RequestChangeAccountEmail([FromBody] GatewayChangeEmailRequestModel changeEmailModel)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //check the redirect URL
            if (!_utilityMethods.CheckIfUrlIsTrusted(changeEmailModel.ClientUrl!, _configuration))
                return BadRequest(new { ErrorMessage = "OriginForRedirectUrlIsNotTrusted" });

            //remove the trailing slash from the client url
            if (changeEmailModel.ClientUrl!.EndsWith("/"))
                changeEmailModel.ClientUrl = changeEmailModel.ClientUrl.Substring(0, changeEmailModel.ClientUrl.Length - 1);

            //start by doing healthchecks for the endpoints this is calling
            if (!await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //request to change the email of the user
            string accessToken = _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request)!;
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.PostAsJsonAsync("Authentication/RequestChangeAccountEmail", new { changeEmailModel.NewEmail }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //after this point the token is certainly valid
            //send the change email link to the user's new email
            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("changeEmailToken", out string? changeEmailToken);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;

            string message = "Click on the following link to confirm your account's new email:";
            string link = $"{_configuration["AuthApiBaseUrl"]}Authentication/ConfirmChangeEmail" +
                $"?userId={userId}&newEmail={changeEmailModel.NewEmail}&changeEmailToken={WebUtility.UrlEncode(changeEmailToken)}&redirectUrl={WebUtility.UrlEncode(changeEmailModel.ClientUrl)}";
            string? confirmationLink = $"{message} {link}";
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", changeEmailModel.NewEmail! },
                { "title", "Email Change Confirmation" },
                { "message", confirmationLink }
            };
            _ = Task.Run(async () =>
            {
                _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpDelete("DeleteAccount")]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //start by doing healthchecks for the endpoints this is calling
            if (!await _utilityMethods.CheckIfMicroservicesFullyOnlineAsync(new List<HttpClient>() { authHttpClient, dataHttpClient, emailHttpClient }))
                return StatusCode(503, new { ErrorMessage = "OneOrMoreMicroservicesAreUnavailable" });

            //request to delete the account of the user
            string accessToken = _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request)!;
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.DeleteAsync("Authentication/DeleteAccount"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //after this point the token is certainly valid
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;

            //request to delete the user's cart
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.DeleteAsync($"Cart/UserId/{userId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //request to remove all the user coupons
            response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => dataHttpClient.DeleteAsync($"Coupon/RemoveAllUserCoupons/userId/{userId}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            //send an email to the user to notify them that their account has been deleted
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", email },
                { "title", "Account Deletion" },
                { "message", "Your account has been deleted. If you have any questions you can contact us at kinnaskonstantinos0@gmail.com ." }
            };
            _ = Task.Run(async () =>
            {
                _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await _utilityMethods.AttemptToSendEmailAsync(emailHttpClient, 3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("CheckResetPasswordEligibility")]
    public async Task<IActionResult> CheckResetPasswordEligibility(string userId, string resetPasswordToken)
    {
        try
        {
            //request the reset of the password of the user
            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
            authHttpClient.GetAsync($"Authentication/CheckResetPasswordEligibility?userId={userId}&resetPasswordToken={WebUtility.UrlEncode(resetPasswordToken)}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Ok(appUser);
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    //also make sure that error messages and correct messages in edit account are displayed correctly
    //once this is done refactor the errorhandling in the AccountController
    [HttpPut("UpdateAccount")]
    public async Task<IActionResult> UpdateAccount([FromBody] GatewayUpdateAccountRequestModel updateUserRequestModel)
    {
        try
        {
            GatewayAppUser gatewayAppUser = new GatewayAppUser();
            gatewayAppUser.FirstName = updateUserRequestModel.FirstName;
            gatewayAppUser.LastName = updateUserRequestModel.LastName;
            gatewayAppUser.PhoneNumber = updateUserRequestModel.PhoneNumber;

            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            //request the reset of the password of the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() =>
            authHttpClient.PutAsJsonAsync("Authentication/UpdateAccount", gatewayAppUser));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
}
