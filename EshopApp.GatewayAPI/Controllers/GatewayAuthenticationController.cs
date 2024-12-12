using EshopApp.GatewayAPI.Models.RequestModels;
using EshopApp.GatewayAPI.Models.ServiceRequestModels;
using EshopApp.GatewayAPI.Models.ServiceResponseModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace EshopApp.GatewayAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayAuthenticationController : ControllerBase
{
    private readonly HttpClient authHttpClient;
    private readonly HttpClient emailHttpClient;
    private readonly HttpClient dataHttpClient;
    private readonly IConfiguration _configuration;

    public GatewayAuthenticationController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        authHttpClient = httpClientFactory.CreateClient("AuthApiClient");
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
        dataHttpClient = httpClientFactory.CreateClient("DataApiClient");
    }

    [HttpGet("GetUserByAccessToken")]
    public async Task<IActionResult> GetUserByAccessToken()
    {
        //request to get the user
        SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
        HttpResponseMessage? response = await authHttpClient.GetAsync("Authentication/GetUserByAccessToken");

        //validate that changing the password has worked
        int retries = 3;
        while ((int)response.StatusCode >= 500)
        {
            if (retries == 0)
                return StatusCode(500, "Internal Server Error");

            response = await authHttpClient.GetAsync("Authentication/GetUserByAccessToken");
            retries--;
        }

        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500) //you can get BadRequest from validationErrors
            return await CommonValidationForRequestClientErrorCodesAsync(response);

        string? responseBody = await response.Content.ReadAsStringAsync();
        JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("AccessToken", out string? accessToken);
        return Ok();
    }

    [HttpPost("SignUp")]
    public async Task<IActionResult> SignUp([FromBody] GatewayApiSignUpRequestModel signUpModel)
    {
        //TODO if the validation fails then maybe do a rollback for the signup???
        try
        {
            //check the redirect URL
            if (!CheckIfUrlIsTrusted(signUpModel.ClientUrl!))
                return BadRequest(new { ErrorMessage = "OriginForRedirectUrlIsNotTrusted" });

            //remove the trailing slash from the client url
            if (signUpModel.ClientUrl!.EndsWith("/"))
                signUpModel.ClientUrl = signUpModel.ClientUrl.Substring(0, signUpModel.ClientUrl.Length - 1);

            //sign up user
            GatewayApiSignUpServiceRequestModel signUpRequestModel = new GatewayApiSignUpServiceRequestModel();
            signUpRequestModel.Email = signUpModel.Email!;
            signUpRequestModel.Password = signUpModel.Password!;
            signUpRequestModel.PhoneNumber = signUpModel.PhoneNumber;

            SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/SignUp", signUpRequestModel);

            //validation that sign up has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/SignUp", signUpRequestModel);
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayApiSignUpServiceResponseModel? signupResponseModel = JsonSerializer.Deserialize<GatewayApiSignUpServiceResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            //create cart for user
            SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            response = await dataHttpClient.PostAsJsonAsync("cart", new { UserId = signupResponseModel!.UserId });

            //validation that cart has been added to user
            retries = 3;
            while (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("cart", new { UserId = signupResponseModel!.UserId });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //send confirmation email
            string message = "Click on the following link to confirm your email: ";
            string link = $"{signUpModel.ClientUrl}?userId={signupResponseModel!.UserId}&token={WebUtility.UrlEncode(signupResponseModel.ConfirmationToken)}";
            string? confirmationLink = $"{message} {link}";
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", signUpModel.Email! },
                { "title", "Email Confirmation" },
                { "message", confirmationLink }
            };
            _ = Task.Run(async () =>
            {
                SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await AttemptToSendEmailAsync(3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server");
        }
    }

    [HttpPost("SignIn")]
    public async Task<IActionResult> SignIn([FromBody] GatewayApiSignInRequestModel signInModel)
    {
        try
        {
            //sign in user
            GatewayApiSignInServiceRequestModel signInRequestModel = new GatewayApiSignInServiceRequestModel();
            signInRequestModel.Email = signInModel.Email;
            signInRequestModel.Password = signInModel.Password;
            signInRequestModel.RememberMe = signInModel.RememberMe;
            SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/SignIn", signInRequestModel);

            //validation that sign in has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/SignIn", signInRequestModel);
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //return access accessToken
            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("AccessToken", out string? accessToken);
            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ExternalSignIn")]
    public async Task<IActionResult> ExternalSignIn([FromBody] GatewayApiExternalSignInRequestModel externalSignInModel)
    {
        try
        {
            //externally start the sign in of the user
            SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ExternalSignIn",
                new { ReturnUrl = externalSignInModel.ReturnUrl, IdentityProviderName = externalSignInModel.IdentityProviderName });

            //validation that sign in has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ExternalSignIn", await authHttpClient.PostAsJsonAsync("Authentication/ExternalSignIn",
                    new { ReturnUrl = externalSignInModel.ReturnUrl, IdentityProviderName = externalSignInModel.IdentityProviderName }));
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //return the challenge result to the client
            string? responseBody = await response.Content.ReadAsStringAsync();
            var challengeDetails = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
            string? identityProviderName = challengeDetails?["IdentityProviderName"];
            string? redirectUri = challengeDetails?["RedirectUri"];

            var authProperties = new AuthenticationProperties
            {
                RedirectUri = redirectUri
            };
            return new ChallengeResult(identityProviderName!, authProperties);
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] GatewayApiForgotPasswordRequestModel forgotPasswordModel)
    {
        try
        {
            //check the redirect URL
            if (!CheckIfUrlIsTrusted(forgotPasswordModel.ClientUrl!))
                return BadRequest(new { ErrorMessage = "OriginForRedirectUrlIsNotTrusted" });

            //remove the trailing slash from the client url
            if (forgotPasswordModel.ClientUrl!.EndsWith("/"))
                forgotPasswordModel.ClientUrl = forgotPasswordModel.ClientUrl.Substring(0, forgotPasswordModel.ClientUrl.Length - 1);

            //request signin in the user
            SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ForgotPassword", new { Email = forgotPasswordModel.Email });

            //validation that requesting forgot password has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ForgotPassword", new { Email = forgotPasswordModel.Email });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayApiForgotPasswordServiceResponseModel? forgotPasswordResponseModel = JsonSerializer.Deserialize<GatewayApiForgotPasswordServiceResponseModel>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            string message = "Click on the following link to change your account's password: ";
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
                SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await AttemptToSendEmailAsync(3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] GatewayApiResetPasswordRequestModel resetPasswordModel)
    {
        try
        {
            //request the reset of the password of the user
            SetDefaultHeadersForClient(false, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ResetPassword",
                new { UserId = resetPasswordModel.UserId, Password = resetPasswordModel.Password, Token = resetPasswordModel.Token });

            //validate that requesting to reset password has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ResetPassword",
                    new { UserId = resetPasswordModel.UserId, Password = resetPasswordModel.Password, Token = resetPasswordModel.Token });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("AccessToken", out string? accessToken);
            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword([FromBody] GatewayApiChangePasswordRequestModel changePasswordModel)
    {
        try
        {
            //request to change the password of the user
            SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/ChangePassword",
                new { CurrentPassword = changePasswordModel.CurrentPassword, NewPassword = changePasswordModel.NewPassword });

            //validate that changing the password has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/ChangePassword", new { CurrentPassword = changePasswordModel.CurrentPassword, NewPassword = changePasswordModel.NewPassword });
                retries--;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("RequestChangeAccountEmail")]
    public async Task<IActionResult> RequestChangeAccountEmail([FromBody] GatewayApiChangeEmailRequestModel changeEmailModel)
    {
        try
        {
            //check the redirect URL
            if (!CheckIfUrlIsTrusted(changeEmailModel.ClientUrl!))
                return BadRequest(new { ErrorMessage = "OriginForRedirectUrlIsNotTrusted" });

            //remove the trailing slash from the client url
            if (changeEmailModel.ClientUrl!.EndsWith("/"))
                changeEmailModel.ClientUrl = changeEmailModel.ClientUrl.Substring(0, changeEmailModel.ClientUrl.Length - 1);

            //request to change the email of the user
            string accessToken = SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!)!;
            HttpResponseMessage? response = await authHttpClient.PostAsJsonAsync("Authentication/RequestChangeAccountEmail", new { NewEmail = changeEmailModel.NewEmail });

            //validate that changing the email has worked
            int retries = 3;
            while ((int)response.StatusCode >= 500)
            {
                if (retries == 0)
                    return StatusCode(500, "Internal Server Error");

                response = await authHttpClient.PostAsJsonAsync("Authentication/RequestChangeAccountEmail", new { NewEmail = changeEmailModel.NewEmail });
                retries--;
            }

            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //after this point the token is certainly valid
            //send the change email link to the user's new email
            string? responseBody = await response.Content.ReadAsStringAsync();
            JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody)!.TryGetValue("ChangeEmailToken", out string? changeEmailToken);

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;

            string message = "Click on the following link to confirm your account's new email: ";
            string? link = $"{changeEmailModel.ClientUrl}?userId={userId}&newEmail={changeEmailModel.NewEmail}&token={WebUtility.UrlEncode(changeEmailToken)}";
            string? confirmationLink = $"{message} {link}";
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", changeEmailModel.NewEmail! },
                { "title", "Email Change Confirmation" },
                { "message", confirmationLink }
            };
            _ = Task.Run(async () =>
            {
                SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await AttemptToSendEmailAsync(3, apiSendEmailModel);
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
            //request to delete the account of the user
            string accessToken = SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!)!;
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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //after this point the token is certainly valid
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            string userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value!;
            string email = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value!;

            //request to delete the user's cart
            SetDefaultHeadersForClient(false, dataHttpClient, _configuration["DataApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //request to remove all the user coupons
            response = await dataHttpClient.DeleteAsync($"Cart/UserId/{userId}");

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
                return await CommonValidationForRequestClientErrorCodesAsync(response);

            //send a request to the user that their account has been deleted
            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", email },
                { "title", "Account Deletion" },
                { "message", "Your account has been deleted. If you have any questions you can contact us at kinnaskonstantinos0@gmail.com ." }
            };
            _ = Task.Run(async () =>
            {
                SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["EmailRateLimitingBypassCode"]!);
                await AttemptToSendEmailAsync(3, apiSendEmailModel);
            });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    private async Task AttemptToSendEmailAsync(int retries, Dictionary<string, string> jsonObject)
    {
        if (retries == 0)
            return;

        HttpResponseMessage response = await emailHttpClient.PostAsJsonAsync("Emails", jsonObject);
        if (response.StatusCode == HttpStatusCode.OK)
            return;

        await Task.Delay(1000);
        retries--;
        await AttemptToSendEmailAsync(retries, jsonObject);
    }

    private string? SetDefaultHeadersForClient(bool includeJWTAuthenticationHeaders, HttpClient httpClient, string apiKey, string rateLimitingBypassCode)
    {
        string? returnedAccessToken = null;
        if (includeJWTAuthenticationHeaders)
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            returnedAccessToken = accessToken;
        }

        httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        httpClient.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", rateLimitingBypassCode);
        return returnedAccessToken;
    }

    private async Task<IActionResult> CommonValidationForRequestClientErrorCodesAsync(HttpResponseMessage response)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        var keyValue = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        keyValue!.TryGetValue("ErrorMessage", out string? errorMessage);
        keyValue!.TryGetValue("Errors", out var errors);

        if (response.StatusCode == HttpStatusCode.Unauthorized && errorMessage is not null)
            return Unauthorized(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Unauthorized();
        else if (response.StatusCode == HttpStatusCode.Forbidden && errorMessage is not null)
            return StatusCode(StatusCodes.Status403Forbidden, new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.Forbidden)
            return StatusCode(StatusCodes.Status403Forbidden);
        else if (response.StatusCode == HttpStatusCode.BadRequest && errorMessage is not null)
            return BadRequest(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.BadRequest && errors is not null) //this is for request validation errors
            return BadRequest(new { Errors = errors });
        else if (response.StatusCode == HttpStatusCode.NotFound && errorMessage is not null)
            return NotFound(new { ErrorMessage = errorMessage });
        else if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();
        else if (response.StatusCode == HttpStatusCode.MethodNotAllowed)
            return StatusCode(StatusCodes.Status405MethodNotAllowed);

        return BadRequest();
    }

    private bool CheckIfUrlIsTrusted(string redirectUrl)
    {
        List<string> trustedDomains = _configuration["TrustedOrigins"]!.Split(" ").ToList();
        var redirectUri = new Uri(redirectUrl);

        foreach (string trustedDomain in trustedDomains)
        {
            var trustedUri = new Uri(trustedDomain);
            if (Uri.Compare(redirectUri, trustedUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0)
                return true;
        }

        return false;
    }
}
