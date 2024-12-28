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
    public async Task<IActionResult> GetUserByAccessToken()
    {
        //check that an access token has been supplied, this check is made to avoid unnecessary requests
        if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
            !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

        //request to get the user
        _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
        HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetUserByAccessToken");

        //validate that getting the user has worked
        int retries = 3;
        while ((int)response.StatusCode >= 500)
        {
            if (retries == 0)
                return StatusCode(500, "Internal Server Error");

            response = await authHttpClient.GetAsync("Authentication/GetUserByAccessToken");
            retries--;
        }

        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

        string? responseBody = await response.Content.ReadAsStringAsync();
        GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        //get the user coupons
        _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
        response = await dataHttpClient.GetAsync($"Coupon/userId/{appUser!.Id}/includeDeactivated/true");

        //validate that getting the user coupons has worked
        retries = 3;
        while ((int)response.StatusCode >= 500)
        {
            if (retries == 0)
                return StatusCode(500, "Internal Server Error");

            response = await dataHttpClient.GetAsync($"Coupon/userId/{appUser.Id}/includeDeactivated/true");
            retries--;
        }

        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

        responseBody = await response.Content.ReadAsStringAsync();
        List<GatewayUserCoupon>? userCoupons = JsonSerializer.Deserialize<List<GatewayUserCoupon>>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        appUser.UserCoupons = userCoupons!;

        //get the user cart
        response = await dataHttpClient.GetAsync($"Cart/UserId/{appUser.Id}");

        //validate that getting the user cart has worked
        retries = 3;
        while ((int)response.StatusCode >= 500)
        {
            if (retries == 0)
                return StatusCode(500, "Internal Server Error");

            response = await dataHttpClient.GetAsync($"Cart/UserId/{appUser.Id}");
            retries--;
        }

        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

        responseBody = await response.Content.ReadAsStringAsync();
        GatewayCart? userCart = JsonSerializer.Deserialize<GatewayCart>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        appUser.Cart = userCart!;

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
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/SignUp", new { signUpModel.Email, signUpModel.Password, signUpModel.PhoneNumber });

            //validation that sign up has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/SignUp", new { signUpModel.Email, signUpModel.Password, signUpModel.PhoneNumber });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewaySignUpServiceResponseModel? signupResponseModel = JsonSerializer.Deserialize<GatewaySignUpServiceResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //create cart for user
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PostAsJsonAsync("cart", new { signupResponseModel!.UserId });

            //validation that cart has been added to user
            retries = 3;
            while (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("cart", new { signupResponseModel!.UserId });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/SignIn", new { signInModel.Email, signInModel.Password, signInModel.RememberMe });

            //validation that sign in has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/SignIn", new { signInModel.Email, signInModel.Password, signInModel.RememberMe });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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

            //request signin in the user
            _utilityMethods.SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ForgotPassword", new { forgotPasswordModel.Email });

            //validation that requesting forgot password has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ForgotPassword", new { forgotPasswordModel.Email });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ResetPassword",
                new { resetPasswordModel.UserId, resetPasswordModel.Password, resetPasswordModel.Token });

            //validate that requesting to reset password has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ResetPassword",
                    new { resetPasswordModel.UserId, resetPasswordModel.Password, resetPasswordModel.Token });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ChangePassword",
                new { changePasswordModel.CurrentPassword, changePasswordModel.NewPassword });

            //validate that changing the password has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ChangePassword", new { changePasswordModel.CurrentPassword, changePasswordModel.NewPassword });
                retries--;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/RequestChangeAccountEmail", new { changeEmailModel.NewEmail });

            //validate that changing the email has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/RequestChangeAccountEmail", new { changeEmailModel.NewEmail });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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
            HttpResponseMessage? response = await authHttpClient.DeleteAsync("Authentication/DeleteAccount");

            //validate that deleting the user account has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.DeleteAsync("Authentication/DeleteAccount");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //after this point the token is certainly valid
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;

            //request to delete the user's cart
            _utilityMethods.SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.DeleteAsync($"Cart/UserId/{userId}");

            //validate the deletion of the user's cart
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.DeleteAsync($"Cart/UserId/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

            //request to remove all the user coupons
            response = await dataHttpClient.DeleteAsync($"Coupon/RemoveAllUserCoupons/userId/{userId}");

            //validate the deletion of the usercoupons
            retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await dataHttpClient.DeleteAsync($"Coupon/RemoveAllUserCoupons/userId/{userId}");
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await _utilityMethods.CommonValidationForRequestClientErrorCodesAsync(response);

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
}
