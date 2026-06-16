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

        appUser!.HasPassword = appUser.PasswordHash is not null; //can happen if user signed in with external sign in provider
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
            string confirmLink = $"{_configuration["AuthApiBaseUrl"]}Authentication/ConfirmEmail?" +
                $"userId={signupResponseModel!.UserId}&confirmEmailToken={WebUtility.UrlEncode(signupResponseModel.ConfirmationToken)}&redirectUrl={WebUtility.UrlEncode(signUpModel.ClientUrl)}";

            var emailHtml = @"
                <!doctype html>
                <html lang=""el"">
                <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Επιβεβαίωση Email</title>
                </head>
                <body style=""margin:0;padding:32px 16px;background:#f2f2f2;font-family:Arial,sans-serif;color:#1a1a1a;"">

                <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 8px 32px rgba(0,0,0,0.12);"">

                  <div style=""background:#ffffff;padding:36px 28px 28px;text-align:center;border-bottom:1px solid #f0f0f0;"">
                    <div style=""width:56px;height:56px;background:#fff5ef;border:1px solid #ffd8c2;border-radius:14px;margin:0 auto 16px;display:flex;align-items:center;justify-content:center;"">
                      <svg width=""26"" height=""26"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""1.8"" stroke-linecap=""round"" stroke-linejoin=""round"">
                        <path d=""M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z""/>
                        <polyline points=""22,6 12,13 2,6""/>
                      </svg>
                    </div>
                    <div style=""display:inline-block;font-size:10px;font-weight:800;letter-spacing:1.2px;padding:5px 12px;border-radius:999px;background:#fff0e8;color:#ff5e00;margin-bottom:14px;text-transform:uppercase;"">
                      Επιβεβαίωση Λογαριασμού
                    </div>
                    <h1 style=""margin:0 0 8px;font-size:24px;font-weight:800;color:#1a1a1a;letter-spacing:-0.5px;"">Καλωσήρθατε στο Eshopapp!</h1>
                    <p style=""margin:0;font-size:13px;color:#888;line-height:1.6;"">Επιβεβαιώστε το email σας για να ολοκληρώσετε την εγγραφή σας</p>
                  </div>

                  <div style=""padding:28px 24px;background:#eeeeee;"">

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:20px;margin-bottom:16px;"">
                      <div style=""display:flex;align-items:center;gap:10px;margin-bottom:16px;"">
                        <svg width=""16"" height=""16"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" style=""flex-shrink:0;"">
                          <circle cx=""12"" cy=""12"" r=""10""/><path d=""M12 8v4M12 16h.01""/>
                        </svg>
                        <span style=""font-size:11px;font-weight:800;color:#888;text-transform:uppercase;letter-spacing:0.8px;"">Στοιχεία Λογαριασμού</span>
                      </div>
                      <div style=""display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid #f5f5f5;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Email</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">" + signUpModel.Email + @"</span>
                      </div>
                      <div style=""display:flex;justify-content:space-between;padding:10px 0;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Ισχύς συνδέσμου</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">24 ώρες</span>
                      </div>
                    </div>

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:24px;text-align:center;margin-bottom:16px;"">
                      <p style=""margin:0 0 6px;font-size:13px;color:#555;line-height:1.6;"">Πατήστε το κουμπί για να επιβεβαιώσετε τη διεύθυνση email σας</p>
                      <p style=""margin:0 0 20px;font-size:12px;color:#aaa;"">Ο σύνδεσμος λήγει μετά από 24 ώρες</p>
                      <a href=""" + confirmLink + @""" style=""display:inline-block;background:#ff5e00;color:#fff;text-decoration:none;font-size:15px;font-weight:800;padding:14px 36px;border-radius:10px;letter-spacing:0.2px;"">Επιβεβαίωση Email</a>
                    </div>

                    <div style=""background:#fff;border-left:4px solid #ff5e00;border-radius:0 6px 6px 0;padding:14px 16px;font-size:13px;color:#555;line-height:1.65;"">
                      <strong style=""color:#1a1a1a;display:block;margin-bottom:4px;"">Δεν δημιουργήσατε λογαριασμό;</strong>
                      Μπορείτε να αγνοήσετε αυτό το email με ασφάλεια. Δεν θα γίνει καμία αλλαγή στον λογαριασμό σας.
                    </div>

                  </div>

                  <div style=""text-align:center;padding:16px;font-size:11px;color:#cc4a00;background:rgba(255,94,0,0.08);border-top:1px solid rgba(255,94,0,0.15);"">
                    © " + DateTime.UtcNow.Year + @" Eshopapp — Αυτόματη ειδοποίηση εγγραφής
                  </div>

                </div>

                </body>
            </html>";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", signUpModel.Email! },
                { "title", "Email Confirmation" },
                { "message", emailHtml }
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

            string resetLink = $"{forgotPasswordModel.ClientUrl}?userId={forgotPasswordResponseModel!.UserId}&token={WebUtility.UrlEncode(forgotPasswordResponseModel.Token)}";

            var emailHtml = @"
                <!doctype html>
                <html lang=""el"">
                <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Επαναφορά Κωδικού</title>
                </head>
                <body style=""margin:0;padding:32px 16px;background:#f2f2f2;font-family:Arial,sans-serif;color:#1a1a1a;"">

                <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 8px 32px rgba(0,0,0,0.12);"">

                  <div style=""background:#ffffff;padding:36px 28px 28px;text-align:center;border-bottom:1px solid #f0f0f0;"">
                    <div style=""width:56px;height:56px;background:#fff5ef;border:1px solid #ffd8c2;border-radius:14px;margin:0 auto 16px;display:flex;align-items:center;justify-content:center;"">
                      <svg width=""26"" height=""26"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""1.8"" stroke-linecap=""round"" stroke-linejoin=""round"">
                        <rect x=""3"" y=""11"" width=""18"" height=""11"" rx=""2"" ry=""2""/>
                        <path d=""M7 11V7a5 5 0 0 1 10 0v4""/>
                      </svg>
                    </div>
                    <div style=""display:inline-block;font-size:10px;font-weight:800;letter-spacing:1.2px;padding:5px 12px;border-radius:999px;background:#fff0e8;color:#ff5e00;margin-bottom:14px;text-transform:uppercase;"">
                      Επαναφορά Κωδικού
                    </div>
                    <h1 style=""margin:0 0 8px;font-size:24px;font-weight:800;color:#1a1a1a;letter-spacing:-0.5px;"">Αίτημα αλλαγής κωδικού</h1>
                    <p style=""margin:0;font-size:13px;color:#888;line-height:1.6;"">Λάβαμε ένα αίτημα επαναφοράς κωδικού για τον λογαριασμό σας</p>
                  </div>

                  <div style=""padding:28px 24px;background:#eeeeee;"">

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:20px;margin-bottom:16px;"">
                      <div style=""display:flex;align-items:center;gap:10px;margin-bottom:16px;"">
                        <svg width=""16"" height=""16"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" style=""flex-shrink:0;"">
                          <circle cx=""12"" cy=""12"" r=""10""/><path d=""M12 8v4M12 16h.01""/>
                        </svg>
                        <span style=""font-size:11px;font-weight:800;color:#888;text-transform:uppercase;letter-spacing:0.8px;"">Σημαντικές πληροφορίες</span>
                      </div>
                      <div style=""display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid #f5f5f5;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Λογαριασμός</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">" + forgotPasswordModel.Email + @"</span>
                      </div>
                      <div style=""display:flex;justify-content:space-between;padding:10px 0;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Ισχύς συνδέσμου</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">24 ώρες</span>
                      </div>
                    </div>

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:24px;text-align:center;margin-bottom:16px;"">
                      <p style=""margin:0 0 6px;font-size:13px;color:#555;line-height:1.6;"">Πατήστε το κουμπί για να ορίσετε νέο κωδικό πρόσβασης</p>
                      <p style=""margin:0 0 20px;font-size:12px;color:#aaa;"">Ο σύνδεσμος λήγει μετά από 24 ώρες</p>
                      <a href=""" + resetLink + @""" style=""display:inline-block;background:#ff5e00;color:#fff;text-decoration:none;font-size:15px;font-weight:800;padding:14px 36px;border-radius:10px;letter-spacing:0.2px;"">Επαναφορά Κωδικού</a>
                    </div>

                    <div style=""background:#fff;border-left:4px solid #ff5e00;border-radius:0 6px 6px 0;padding:14px 16px;font-size:13px;color:#555;line-height:1.65;"">
                      <strong style=""color:#1a1a1a;display:block;margin-bottom:4px;"">Δεν ζητήσατε αλλαγή κωδικού;</strong>
                      Μπορείτε να αγνοήσετε αυτό το email με ασφάλεια. Ο λογαριασμός σας παραμένει προστατευμένος.
                    </div>

                  </div>

                  <div style=""text-align:center;padding:16px;font-size:11px;color:#cc4a00;background:rgba(255,94,0,0.08);border-top:1px solid rgba(255,94,0,0.15);"">
                    © " + DateTime.UtcNow.Year + @" Eshopapp — Αυτόματη ειδοποίηση ασφαλείας
                  </div>

                </div>

                </body>
            </html>";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", forgotPasswordModel.Email! },
                { "title", "Reset Password Confirmation" },
                { "message", emailHtml }
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

            string confirmLink = $"{_configuration["AuthApiBaseUrl"]}Authentication/ConfirmChangeEmail" +
                $"?userId={userId}&newEmail={changeEmailModel.NewEmail}&changeEmailToken={WebUtility.UrlEncode(changeEmailToken)}&redirectUrl={WebUtility.UrlEncode(changeEmailModel.ClientUrl)}";

            var emailHtml = @"
            <!doctype html>
                <html lang=""el"">
                <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Αλλαγή Email</title>
                </head>
                <body style=""margin:0;padding:32px 16px;background:#f2f2f2;font-family:Arial,sans-serif;color:#1a1a1a;"">

                <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 8px 32px rgba(0,0,0,0.12);"">

                    <div style=""background:#ffffff;padding:36px 28px 28px;text-align:center;border-bottom:1px solid #f0f0f0;"">
                    <div style=""width:56px;height:56px;background:#fff5ef;border:1px solid #ffd8c2;border-radius:14px;margin:0 auto 16px;display:flex;align-items:center;justify-content:center;"">
                        <svg width=""26"" height=""26"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""1.8"" stroke-linecap=""round"" stroke-linejoin=""round"">
                        <path d=""M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z""/>
                        <polyline points=""22,6 12,13 2,6""/>
                        <line x1=""5"" y1=""12"" x2=""9"" y2=""12""/>
                        <line x1=""7"" y1=""10"" x2=""7"" y2=""14""/>
                        </svg>
                    </div>
                    <div style=""display:inline-block;font-size:10px;font-weight:800;letter-spacing:1.2px;padding:5px 12px;border-radius:999px;background:#fff0e8;color:#ff5e00;margin-bottom:14px;text-transform:uppercase;"">
                        Αλλαγή Email
                    </div>
                    <h1 style=""margin:0 0 8px;font-size:24px;font-weight:800;color:#1a1a1a;letter-spacing:-0.5px;"">Επιβεβαίωση νέου email</h1>
                    <p style=""margin:0;font-size:13px;color:#888;line-height:1.6;"">Λάβαμε ένα αίτημα αλλαγής της διεύθυνσης email του λογαριασμού σας</p>
                    </div>

                    <div style=""padding:28px 24px;background:#eeeeee;"">

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:20px;margin-bottom:16px;"">
                        <div style=""display:flex;align-items:center;gap:10px;margin-bottom:16px;"">
                        <svg width=""16"" height=""16"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" style=""flex-shrink:0;"">
                            <circle cx=""12"" cy=""12"" r=""10""/><path d=""M12 8v4M12 16h.01""/>
                        </svg>
                        <span style=""font-size:11px;font-weight:800;color:#888;text-transform:uppercase;letter-spacing:0.8px;"">Στοιχεία Αλλαγής</span>
                        </div>
                        <div style=""display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid #f5f5f5;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Νέο Email</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">" + changeEmailModel.NewEmail + @"</span>
                        </div>
                        <div style=""display:flex;justify-content:space-between;padding:10px 0;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Ισχύς συνδέσμου</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">24 ώρες</span>
                        </div>
                    </div>

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:24px;text-align:center;margin-bottom:16px;"">
                        <p style=""margin:0 0 6px;font-size:13px;color:#555;line-height:1.6;"">Πατήστε το κουμπί για να επιβεβαιώσετε τη νέα διεύθυνση email σας</p>
                        <p style=""margin:0 0 20px;font-size:12px;color:#aaa;"">Ο σύνδεσμος λήγει μετά από 24 ώρες</p>
                        <a href=""" + confirmLink + @""" style=""display:inline-block;background:#ff5e00;color:#fff;text-decoration:none;font-size:15px;font-weight:800;padding:14px 36px;border-radius:10px;letter-spacing:0.2px;"">Επιβεβαίωση Αλλαγής</a>
                    </div>

                    <div style=""background:#fff;border-left:4px solid #ff5e00;border-radius:0 6px 6px 0;padding:14px 16px;font-size:13px;color:#555;line-height:1.65;"">
                        <strong style=""color:#1a1a1a;display:block;margin-bottom:4px;"">Δεν ζητήσατε αλλαγή email;</strong>
                        Μπορείτε να αγνοήσετε αυτό το email με ασφάλεια. Η διεύθυνση email σας δεν θα αλλάξει.
                    </div>

                    </div>

                    <div style=""text-align:center;padding:16px;font-size:11px;color:#cc4a00;background:rgba(255,94,0,0.08);border-top:1px solid rgba(255,94,0,0.15);"">
                    © " + DateTime.UtcNow.Year + @" Eshopapp — Αυτόματη ειδοποίηση ασφαλείας
                    </div>

                </div>

                </body>
            </html>";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", changeEmailModel.NewEmail! },
                { "title", "Email Change Confirmation" },
                { "message", emailHtml }
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
            var emailHtml = @"
            <!doctype html>
            <html lang=""el"">
                <head>
                <meta charset=""UTF-8"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                <title>Διαγραφή Λογαριασμού</title>
                </head>
                <body style=""margin:0;padding:32px 16px;background:#f2f2f2;font-family:Arial,sans-serif;color:#1a1a1a;"">

                <div style=""max-width:600px;margin:0 auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 8px 32px rgba(0,0,0,0.12);"">

                    <div style=""background:#ffffff;padding:36px 28px 28px;text-align:center;border-bottom:1px solid #f0f0f0;"">
                    <div style=""width:56px;height:56px;background:#fff5ef;border:1px solid #ffd8c2;border-radius:14px;margin:0 auto 16px;display:flex;align-items:center;justify-content:center;"">
                        <svg width=""26"" height=""26"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""1.8"" stroke-linecap=""round"" stroke-linejoin=""round"">
                        <polyline points=""3 6 5 6 21 6""/><path d=""M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6""/><path d=""M10 11v6M14 11v6""/><path d=""M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2""/>
                        </svg>
                    </div>
                    <div style=""display:inline-block;font-size:10px;font-weight:800;letter-spacing:1.2px;padding:5px 12px;border-radius:999px;background:#fff0e8;color:#ff5e00;margin-bottom:14px;text-transform:uppercase;"">
                        Διαγραφή Λογαριασμού
                    </div>
                    <h1 style=""margin:0 0 8px;font-size:24px;font-weight:800;color:#1a1a1a;letter-spacing:-0.5px;"">Ο λογαριασμός σας διαγράφηκε</h1>
                    <p style=""margin:0;font-size:13px;color:#888;line-height:1.6;"">Σας ενημερώνουμε ότι ο λογαριασμός σας στο Eshopapp έχει διαγραφεί οριστικά</p>
                    </div>

                    <div style=""padding:28px 24px;background:#eeeeee;"">

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:20px;margin-bottom:16px;"">
                        <div style=""display:flex;align-items:center;gap:10px;margin-bottom:16px;"">
                        <svg width=""16"" height=""16"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""2"" stroke-linecap=""round"" stroke-linejoin=""round"" style=""flex-shrink:0;"">
                            <circle cx=""12"" cy=""12"" r=""10""/><path d=""M12 8v4M12 16h.01""/>
                        </svg>
                        <span style=""font-size:11px;font-weight:800;color:#888;text-transform:uppercase;letter-spacing:0.8px;"">Στοιχεία Διαγραφής</span>
                        </div>
                        <div style=""display:flex;justify-content:space-between;padding:10px 0;border-bottom:1px solid #f5f5f5;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Λογαριασμός</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">" + email + @"</span>
                        </div>
                        <div style=""display:flex;justify-content:space-between;padding:10px 0;font-size:14px;"">
                        <span style=""color:#888;font-weight:600;"">Ημερομηνία διαγραφής</span>
                        <span style=""font-weight:700;color:#1a1a1a;"">" + DateTime.UtcNow.ToString("dd/MM/yyyy") + @"</span>
                        </div>
                    </div>

                    <div style=""background:#fff;border:1px solid #d8d8d8;border-radius:12px;padding:20px;margin-bottom:16px;text-align:center;"">
                        <p style=""margin:0 0 4px;font-size:13px;color:#555;line-height:1.7;"">Εάν έχετε απορίες ή χρειάζεστε βοήθεια, επικοινωνήστε μαζί μας:</p>
                        <a href=""mailto:kinnaskonstantinos0@gmail.com"" style=""font-size:14px;font-weight:800;color:#ff5e00;text-decoration:none;"">kinnaskonstantinos0@gmail.com</a>
                    </div>

                    <div style=""background:#fff;border-left:4px solid #ff5e00;border-radius:0 6px 6px 0;padding:14px 16px;font-size:13px;color:#555;line-height:1.65;"">
                        <strong style=""color:#1a1a1a;display:block;margin-bottom:4px;"">Δεν ζητήσατε τη διαγραφή του λογαριασμού σας;</strong>
                        Επικοινωνήστε μαζί μας άμεσα στο παραπάνω email ώστε να διερευνήσουμε το θέμα.
                    </div>

                    </div>

                    <div style=""text-align:center;padding:16px;font-size:11px;color:#cc4a00;background:rgba(255,94,0,0.08);border-top:1px solid rgba(255,94,0,0.15);"">
                    © " + DateTime.UtcNow.Year + @" Eshopapp — Αυτόματη ειδοποίηση λογαριασμού
                    </div>

                </div>

                </body>
            </html>";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", email },
                { "title", "Account Deletion" },
                { "message", emailHtml }
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

    // ******* The below endpoints were added later and have not been thoroughly tested *******

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
            gatewayAppUser.Address = updateUserRequestModel.Address;

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


    [HttpGet("GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/ClaimType/{claimType}/ClaimValue/{claimValue}")]
    [HttpGet("GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/ClaimType/{claimType}/ClaimValue/{claimValue}/SecondClaimType/{secondClaimType}/SecondClaimValue/{secondClaimValue}")]
    public async Task<IActionResult> GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken(string claimType, string claimValue, string? secondClaimType, string? secondClaimValue)
    {
        try
        {
            //check that an access token has been supplied, this check is made to avoid unnecessary requests
            if (HttpContext?.Request == null || !HttpContext.Request.Headers.ContainsKey("Authorization") || string.IsNullOrEmpty(HttpContext.Request.Headers["Authorization"]) ||
                !HttpContext.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized(new { ErrorMessage = "NoValidAccessTokenWasProvided" });

            string? endpoint;
            if (string.IsNullOrEmpty(secondClaimType) || string.IsNullOrEmpty(secondClaimValue))
                endpoint = $"ClaimType/{claimType}/ClaimValue/{claimValue}";
            else
                endpoint = $"ClaimType/{claimType}/ClaimValue/{claimValue}/SecondClaimType/{secondClaimType}/SecondClaimValue/{secondClaimValue}";

            //request to get the user
            _utilityMethods.SetDefaultHeadersForClient(true, authHttpClient, _configuration["AuthApiKey"]!, _configuration["AuthRateLimitingBypassCode"]!, HttpContext.Request);
            HttpResponseMessage response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => authHttpClient.GetAsync($"Authentication/GetCurrentUserAndValidateThatTheyHaveGivenClaimsByToken/{endpoint}"));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            string? responseBody = await response.Content.ReadAsStringAsync();
            GatewayAppUser? appUser = JsonSerializer.Deserialize<GatewayAppUser>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            appUser!.HasPassword = appUser.PasswordHash is not null; //can happen if user signed in with external sign in provider

            return Ok(appUser);
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
}
