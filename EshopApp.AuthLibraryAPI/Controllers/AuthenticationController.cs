using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.UserLogic;
using EshopApp.AuthLibraryAPI.Models.RequestModels;
using EshopApp.AuthLibraryAPI.Models.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace EshopApp.AuthLibraryAPI.Controllers;

[ApiController]
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
            return Ok(user);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server");
        }
    }

    [HttpPost("SignUp")]
    [AllowAnonymous]
    public async Task<IActionResult> SignUp([FromBody] ApiSignUpRequestModel signUpModel)
    {
        try
        {
            AppUser? user = await _authenticationProcedures.FindByEmailAsync(signUpModel.Username!);
            if (user is not null)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });

            user = new AppUser();
            user.UserName = signUpModel.Username!;
            user.PhoneNumber = signUpModel.PhoneNumber!;
            user.Email = signUpModel.Username; //Email and username are the same for this app

            (string userId, string confirmationToken) = await _authenticationProcedures.SignUpUserAsync(user, signUpModel.Password!, false);
            
            return Ok(new ApiSignUpResponseModel(userId, confirmationToken));

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

    [HttpGet("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        try
        {
            string accessToken = await _authenticationProcedures.ConfirmEmailAsync(userId, token);
            if (accessToken is null)
                return BadRequest();

            return Ok(new { AccessToken = accessToken });
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
            string accessToken = await _authenticationProcedures.
                SignInUserAsync(signInModel.Username!, signInModel.Password!, signInModel.RememberMe);

            if (accessToken is null)
                return Unauthorized();

            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ApiForgotPasswordRequestModel forgotPasswordModel)
    {
        try
        {
            AppUser? user;
            user = await _authenticationProcedures.FindByEmailAsync(forgotPasswordModel.Email!);

            if (user is null)
                return BadRequest(new {ErrorMessage = "UnkownEmail"});

            string passwordResetToken = await _authenticationProcedures.CreateResetPasswordTokenAsync(user);
            return Ok(new { PasswordResetToken = passwordResetToken });

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

    [HttpGet("ResetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string userId)
    {
        try
        {
            AppUser? user = await _authenticationProcedures.FindByUserIdAsync(userId);
            if (user is null)
                return BadRequest();

            return Ok(new { Username = user.UserName });
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
            string accessToken = await _authenticationProcedures.ResetPasswordAsync(
                    resetPasswordModel.UserId!, resetPasswordModel.Token!, resetPasswordModel.Password!);

            if (accessToken == null)
                return BadRequest(new {ErrorMessage = "InvalidResetTokenForUserId"});

            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("EditAccount")]
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
    }

    //TODO Skip Tests For This Method, because I am considering changing it
    [HttpPost("ChangeBasicAccountSettings")]
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
            bool result = await _authenticationProcedures.UpdateUserAccountAsync(user);
            //I am not certain how this can happen, but 
            if (!result)
                return BadRequest(new { ErrorMessage = "BasicInformationChangeError" });

            return Ok();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ApiChangePasswordRequestModel changePasswordModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByToken(token);
            if (user is null)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });

            (bool result, string errorCode) = await _authenticationProcedures.ChangePasswordAsync(
                user, changePasswordModel.OldPassword!, changePasswordModel.NewPassword!);

            if (!result && errorCode == "passwordMismatch")
                return BadRequest(new { ErrorMessage = "PasswordMismatchError" });
            else if (!result)
                return BadRequest(new { ErrorMessage = "PasswordChangeError" });

            return Ok();
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("RequestChangeAccountEmail")]
    [Authorize]
    public async Task<IActionResult> RequestChangeAccountEmail([FromBody] ApiChangeEmailRequestModel changeEmailModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByToken(token);
            if (user is null)
                return BadRequest(new { ErrorMessage = "ValidTokenButUserNotInSystem" });

            AppUser? otherUser = await _authenticationProcedures.FindByEmailAsync(changeEmailModel.NewEmail!);
            if (otherUser is not null)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });

            string changeEmailToken = await _authenticationProcedures.CreateChangeEmailTokenAsync(user, changeEmailModel.NewEmail!);

            return Ok(new { ChangeEmailToken = changeEmailToken });

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
            await _authenticationProcedures.UpdateUserAccountAsync(user);
            return Ok(new { Warning = "None" });*/
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost("ConfirmChangeEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmChangeEmail([FromBody] ApiConfirmChangeEmailRequestModel apiConfirmChangeEmailRequestModel)
    {
        try
        {
            string accessToken = await _authenticationProcedures.ChangeEmailAsync(apiConfirmChangeEmailRequestModel.UserId!, 
                apiConfirmChangeEmailRequestModel.ChangeEmailToken!, apiConfirmChangeEmailRequestModel.NewEmail!);
            if (accessToken is null)
                return BadRequest();
            return Ok(new { AccessToken = accessToken });
        }
        catch
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

}
