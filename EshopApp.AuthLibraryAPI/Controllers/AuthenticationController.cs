using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.UserLogic;
using EshopApp.AuthLibraryAPI.Models.RequestModels;
using EshopApp.AuthLibraryAPI.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Web;

namespace EshopApp.AuthLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationProcedures _authenticationProcedures;
    private readonly IConfiguration _configuration;

    public AuthenticationController(IAuthenticationProcedures authenticationProcedures, IConfiguration configuration)
    {
        _authenticationProcedures = authenticationProcedures;
        _configuration = configuration;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("TryGetCurrentUser")]
    [Authorize]
    public async Task<IActionResult> TryGetCurrentUser()
    {
        try
        {
            // Retrieve the Authorization header from the HTTP request
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByToken(token);
            if (user is null)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });

            return Ok(user);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="signUpModel"></param>
    /// <returns></returns>
    [HttpPost("SignUp")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] ApiSignUpRequestModel signUpModel)
    {
        try
        {
            LibSignUpResponseModel signUpResponseModel = await _authenticationProcedures.SignUpAsync(signUpModel.Email!, signUpModel.PhoneNumber!, signUpModel.Password!);

            if (signUpResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });
            else if (signUpResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return Ok(new ApiSignUpResponseModel(signUpResponseModel.UserId!, signUpResponseModel.Token!));

            //The following needs to be done from the Gateway API
            /*string message = "Click on the following link to confirm your email:";
            string link = $"{_configuration["WebClientOriginUrl"]}/Account/ConfirmEmail?userId={userId}&token={WebUtility.UrlEncode(confirmationToken)}";
            string? confirmationLink = $"{message} {link}";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "receiver", user.Email! },
                { "title", "Email Confirmation" },
                { "message", confirmationLink }
            };

            var response = await httpClient.PostAsJsonAsync("Emails", apiSendEmailModel);
            if (response.StatusCode != HttpStatusCode.OK)
                return Ok(new { Warning = "ConfirmationEmailNotSent" });

            return Ok(new { Warning = "None" });*/
        }
        catch
        {
            return StatusCode(500, "Internal Server");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <param name="redirectUrl"></param>
    /// <returns></returns>
    //redirectUrl contains as a query parameter the returnUrl and thus what is sent is something like the following
    //https://myapp/specificendpointthathandlesredirect?returnUrl=https://myapp/wherethewholethingwasstartedfrom
    [HttpGet("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token, string? redirectUrl = null)
    {
        try
        {
            if (redirectUrl is not null && !CheckIfUrlIsTrusted(redirectUrl))
                return BadRequest(new { ErrorMessage = "InvalidRedirectUrl" });
            
            ReturnCodeAndTokenResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.ConfirmEmailAsync(userId, token);
            if (redirectUrl is not null)
            {
                var uribBuilder = new UriBuilder(redirectUrl);
                var query = HttpUtility.ParseQueryString(uribBuilder.Query);
                if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                    query["errorMessage"] = "UserNotFoundWithGivenUserId";
                else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                    query["errorMessage"] = "UnknownError";
                else
                    query["accessToken"] = returnCodeAndTokenResponseModel.Token;

                uribBuilder.Query = query.ToString();
                return Redirect(uribBuilder.ToString());
            }

            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenUserId" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnkownError" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] ApiSignInRequestModel signInModel)
    {
        try
        {
            ReturnCodeAndTokenResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.SignInAsync(signInModel.Email!, signInModel.Password!, signInModel.RememberMe);
            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenEmail)
                return Unauthorized(new { ErrorMessage = "UserNotFoundWithGivenEmail" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated"});
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InvalidCredentials)
                return Unauthorized(new { ErrorMessage = "InvalidCredentials" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="forgotPasswordModel"></param>
    /// <returns></returns>
    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ApiForgotPasswordRequestModel forgotPasswordModel)
    {
        try
        {
            ReturnCodeAndTokenResponseModel returnCodeAndTokenResponse = await _authenticationProcedures.CreateResetPasswordTokenAsync(forgotPasswordModel.Email!);
            if (returnCodeAndTokenResponse.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenEmail)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenEmail" });

            return Ok(new { PasswordResetToken = returnCodeAndTokenResponse.Token });

            //TODO add this in the gateway API
            /*string message = "Click on the following link to reset your account password:";
            string? link = $"{_configuration["WebClientOriginUrl"]}/Account/ResetPassword?userId={user.Id}&token={WebUtility.UrlEncode(passwordResetToken)}";
            string? confirmationLink = $"{message} {link}";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "title", "Reset Password Confirmation" },
                { "message", confirmationLink },
                { "receiver", user.Email! }
            };

            var response = await httpClient.PostAsJsonAsync("Emails", apiSendEmailModel);
            if (response.StatusCode != HttpStatusCode.OK)
                return Ok(new { Warning = "ResetEmailNotSent" });

            return Ok(new { Warning = "None" });*/
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ApiResetPasswordRequestModel resetPasswordModel)
    {
        try
        {
            ReturnCodeAndTokenResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.ResetPasswordAsync(resetPasswordModel.UserId!, resetPasswordModel.Token!, resetPasswordModel.Password!);

            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new {ErrorMessage = "UserNotFoundWithGivenEmail" });
            else if(returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    //This might need to go to the gateway api in some way, because here it does not make much sense. Considering that GetCurrentUser is literally the same this probably can go.
    /*[HttpGet("EditAccount")]
    [Authorize]
    public async Task<IActionResult> EditAccount()
    {
        try
        {
            // Retrieve the Authorization header from the HTTP request
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByToken(token);

            //this is very unlikely to happen, but someone could craft a valid token without the user existing.
            //This check is for a very edge case and will probably never happen.
            if (user is null)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });

            return Ok(user);
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }*/

    //TODO Skip Tests For This Method, because I am considering changing it
    /*[HttpPost("ChangeBasicAccountSettings")]
    [Authorize]
    public async Task<IActionResult> ChangeBasicAccountSettings([FromBody] ApiAccountBasicSettingsRequestModel accountBasicSettingsViewModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByToken(token);

            //this is very unlikely to happen, but someone could craft a valid token without the user existing.
            //This check is for a very edge case and will probably never happen.
            if (user is null)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });

            user.PhoneNumber = accountBasicSettingsViewModel.PhoneNumber;
            bool result = await _authenticationProcedures.UpdateAccountAsync(user);
            //I am not certain how this can happen, but 
            if (!result)
                return BadRequest(new { ErrorMessage = "BasicInformationChangeError" });

            return Ok();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }*/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="changePasswordModel"></param>
    /// <returns></returns>
    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ApiChangePasswordRequestModel changePasswordModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            LibraryReturnedCodes libraryReturnedCodes = await _authenticationProcedures.ChangePasswordAsync(token, changePasswordModel.OldPassword!, changePasswordModel.NewPassword!);

            if(libraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            if (libraryReturnedCodes == LibraryReturnedCodes.PasswordMissmatch)
                return BadRequest(new { ErrorMessage = "PasswordMismatchError" });
            else if (libraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });
            
            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="changeEmailModel"></param>
    /// <returns></returns>
    [HttpPost("RequestChangeAccountEmail")]
    [Authorize]
    public async Task<IActionResult> RequestChangeAccountEmail([FromBody] ApiChangeEmailRequestModel changeEmailModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnCodeAndTokenResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.CreateChangeEmailTokenAsync(token, changeEmailModel.NewEmail!);

            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if(returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });

            return Ok(new { ChangeEmailToken = returnCodeAndTokenResponseModel.Token });

            //TODO add this to the Gateway API
            /*string message = "Click on the following link to confirm your account's new email:";
            string? link =
                $"{_configuration["WebClientOriginUrl"]}/Account/ConfirmChangeEmail?userId={user.Id}&newEmail={changeEmailModel.NewEmail}&token={WebUtility.UrlEncode(passwordResetToken)}";

            string? confirmationLink = $"{message} {link}";

            var apiSendEmailModel = new Dictionary<string, string>
            {
                { "title", "Email Change Confirmation" },
                { "message", confirmationLink },
                { "receiver", user.Email! }
            };

            var response = await httpClient.PostAsJsonAsync("Emails", apiSendEmailModel);
            if (response.StatusCode != HttpStatusCode.OK)
                return Ok(new { Warning = "EmailNotSent" });

            user.EmailConfirmed = false;
            await _authenticationProcedures.UpdateAccountAsync(user);
            return Ok(new { Warning = "None" });*/
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="apiConfirmChangeEmailRequestModel"></param>
    /// <returns></returns>
    [HttpGet("ConfirmChangeEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string changeEmailToken, string? redirectUrl = null)
    {
        try
        {
            if (redirectUrl is not null && !CheckIfUrlIsTrusted(redirectUrl))
                return BadRequest(new { ErrorMessage = "InvalidRedirectUrl" });

            ReturnCodeAndTokenResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.ChangeEmailAsync(userId, changeEmailToken, newEmail);
            if (redirectUrl is not null)
            {
                var uribBuilder = new UriBuilder(redirectUrl);
                var query = HttpUtility.ParseQueryString(uribBuilder.Query);
                if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                    query["errorMessage"] = "UserNotFoundWithGivenUserId";
                else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                    query["errorMessage"] = "DuplicateEmail";
                else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InvalidEmailAndEmailChangeTokenCombination)
                    query["errorMessage"] = "InvalidEmailAndEmailChangeTokenCombination";
                else
                    query["accessToken"] = returnCodeAndTokenResponseModel.Token;

                uribBuilder.Query = query.ToString();
                return Redirect(uribBuilder.ToString());
            }

            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenId" });
            else if(returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });
            else if(returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InvalidEmailAndEmailChangeTokenCombination)
                return BadRequest(new { ErrorMessage = "InvalidEmailChangeEmailTokenCombination" });
            
            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpDelete("DeleteAccount")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount(string userId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            LibraryReturnedCodes returnedCode = await _authenticationProcedures.DeleteAccountAsync(userId, token);

            if (returnedCode  == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.UserDoesNotOwnGivenAccount)
                return Unauthorized(new { ErrorMessage = "UserDoesNotOwnGivenAccount" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return Unauthorized(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    private bool CheckIfUrlIsTrusted(string redirectUrl)
    {
        List<string> trustedDomains = _configuration["ExcludedCorsOrigins"]!.Split(" ").ToList();
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
