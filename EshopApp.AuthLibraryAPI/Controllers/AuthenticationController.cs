using EshopApp.AuthLibrary.AuthLogic;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationModels;
using EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationProceduresModels;
using EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;
using EshopApp.AuthLibraryAPI.Models.ResponseModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Web;

namespace EshopApp.AuthLibraryAPI.Controllers;

/// <summary>
/// 
/// </summary>
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
    /// This endpoint is the most fundemental endpoint and simply retrieves the a corresponding user of the access confirmEmailToken. The validation of the confirmEmailToken occurs in the middlewares of the api, 
    /// but this method also checks that the access confirmEmailToken, even if is valid, it actually belongs to a user in the system/database. 
    /// </summary>
    /// <returns>An isntance of appuser that corresponds to the access confirmEmailToken with their fields that exist in the auth database filled, or it returns Unauthorized, BadRequest or other error codes depending
    /// on the error case.</returns>
    [HttpGet("GetUserByAccessToken")]
    [Authorize]
    public async Task<IActionResult> GetUserByAccessToken()
    {
        try
        {
            // Retrieve the Authorization header from the HTTP request
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnUserAndCodeResponseModel returnCodeAndUserResponseModel = await _authenticationProcedures.GetCurrentUserByTokenAsync(token);
            if (returnCodeAndUserResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnCodeAndUserResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCodeAndUserResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            return Ok(returnCodeAndUserResponseModel.AppUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint create an account for the user in the authentication and authorization database, using the signupModel, which it validates. An important thing to keep in mind
    /// is that this endpoint does not activate the account and just returns the necessary confirmEmailToken to activate that account, which can be done using the ConfirmEmail endpoint. Another 
    /// important thing is that the username and the email are the same for this application. 
    /// </summary>
    /// <param name="signUpModel">this parameter consists of the email and password fields that are required and must be valid and optionally a valid phone number.</param>
    /// <returns>The userId of the newly created account and the confirm email confirmEmailToken that can be used in the ConfirmEmail endpoint to activate the account, or it returns BadRequest, InternalServerError or other 
    /// error codes depending on the error case.</returns>
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
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used after the SignUp endpoint has returned the confirm email confirmEmailToken to activate the newly created account and optionally redirects the user to a specific returnUrl. 
    /// This endpoint can be used with an api client without passing in a redirectUrl parameter since api clients in general do not need a redirect link.
    /// The redirectUrl parameter can be used for the typical flow of the application in which way the redirectUrl will be needed to redirect the user correctly to the front end. 
    /// If the latter is the case the front end needs to handle the response, which means it should correctly read the potential errormessage or access confirmEmailToken it will receive from this endpoint.
    /// Another important point is that this endpoint is also one of the few endpoints that can be used without the need of an API key.
    /// </summary>
    /// <param name="userId">Necessary parameter that specifies the userId of the account that needs to be activated.</param>
    /// <param name="confirmEmailToken">Necessary parameter that specifies the confirm email confirmEmailToken that is needed to activate the account. This confirmEmailToken can be produced using the SignUp endpoint and
    /// must be sent to the ConfirmEmail endpoint as a url encoded query parameter</param>
    /// <param name="redirectUrl">Optional parameter that specifies the redirectUrl that the endpoint will redirect the user to with its given response.</param>
    /// <returns>It returns multiple status codes that indicate whether or not the operation was successful or not. If the redirectUrl parameter is not null it will return a Redirect result
    /// which will redirect the user to the given url with the appropriate arguements, otherwise it will return Ok status code with the newly created access confirmEmailToken</returns>
    //redirectUrl can contain as a query parameter the returnUrl(if the client wishes for it) and thus what is sent is something like the following
    //https://myapp/endpointThatHandlesRedirect?returnUrl=https://myapp/endpointTheClientWishesToRedirectAfterConfirmingEmailHasBeenDone
    [HttpGet("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string confirmEmailToken, string? redirectUrl = null)
    {
        try
        {
            if (redirectUrl is not null && !CheckIfUrlIsTrusted(redirectUrl))
                return BadRequest(new { ErrorMessage = "InvalidRedirectUrl" });

            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.ConfirmEmailAsync(userId, confirmEmailToken);
            //this appends in the end of the redirectUrl either an errorMesage or an accessToken depending on whether or not the confirming of the email was completed successfully
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
            if (redirectUrl is not null)
            {
                var uribBuilder = new UriBuilder(redirectUrl);
                var query = HttpUtility.ParseQueryString(uribBuilder.Query);

                query["errorMessage"] = "InternalServerError";
                uribBuilder.Query = query.ToString();
                return Redirect(uribBuilder.ToString());
            }

            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used to sign in a user using the email and password fields that were given as input and optionally the rememberMe field. An important thing to keep in mind is that if 
    /// the account is not activated, even if the credentials are correct the sign in will return an error, so the ConfirmEmail endpoint should be used first before a sign in can succeed.
    /// </summary>
    /// <param name="signInModel">This parameter consists of 2 necessary and valid parameters, which are the email and the password of the account and the optional boolean parameter RememberMe, 
    /// which dictates the duration of the validity of the access confirmEmailToken(if false 1 day, if true 30 days as it stands).</param>
    /// <returns>it returns Ok status code with the newly created access confirmEmailToken, or Unauthorized, Internal server status codes depending on the error case.</returns>
    [HttpPost("SignIn")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] ApiSignInRequestModel signInModel)
    {
        try
        {
            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.SignInAsync(signInModel.Email!, signInModel.Password!, signInModel.RememberMe);
            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenEmail)
                return Unauthorized(new { ErrorMessage = "UserNotFoundWithGivenEmail" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InvalidCredentials)
                return Unauthorized(new { ErrorMessage = "InvalidCredentials" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint simply returns the registered external identity providers and their properties that are registered for this api. 
    /// </summary>
    /// <returns>Either Ok status code with the external identity providers and their properties or an internal server error status code</returns>
    [HttpGet("GetRegisteredExternalIdentityProviders")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRegisteredExternalIdentityProviders()
    {
        try
        {
            IEnumerable<AuthenticationScheme> externalIdentityProviders = await _authenticationProcedures.GetExternalIdentityProvidersAsync();
            List<string> externalIdentityProvidersNames = new List<string>();
            foreach (AuthenticationScheme scheme in externalIdentityProviders)
                externalIdentityProvidersNames.Add(scheme.Name);

            return Ok(externalIdentityProvidersNames);
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used to initiate the OAuth2 flow with an external identity provider. The main point to keep in mind is that the return Url needs to be valid and fully authorized by the app
    /// or else the whole process will not work. Unlike other endpoints that contain redirects the returnUrl is necessary for the OAuth2 flow. If the operation is successful and the user correctly
    /// authenticates with their external identity provider then the ExternalSignInCallback endpoint will be called to finish the OAuth2 flow.
    /// </summary>
    /// <param name="apiExternalSignInRequestModel">This model contains 2 properties. The IdentityProvider name and the ReturnUrl property that must be valid for the OAuth2 flow to work. The return Url
    /// domain part needs to also be registered in the API itself.</param>
    /// <returns>If successful it returns a Challenge Result(302) that initiate the OAuth2 flow with the external Identity provider. If not successful it returns BadRequest or other error codes 
    /// depending on the error case.</returns>
    [HttpPost("ExternalSignIn")]
    [AllowAnonymous]
    public IActionResult ExternalSignIn([FromBody] ApiExternalSignInRequestModel apiExternalSignInRequestModel)
    {
        try
        {
            if (!CheckIfUrlIsTrusted(apiExternalSignInRequestModel.ReturnUrl!))
                return BadRequest(new { ErrorMessage = "InvalidReturnUrl" });

            string redirectUrl = Url.Action("ExternalSignInCallback", "Authentication", new { ReturnUrl = apiExternalSignInRequestModel.ReturnUrl })!;
            AuthenticationProperties identityProviderConfiguration = _authenticationProcedures.GetExternalIdentityProvidersProperties(apiExternalSignInRequestModel.IdentityProviderName!, redirectUrl);

            return new ChallengeResult(apiExternalSignInRequestModel.IdentityProviderName!, identityProviderConfiguration);
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is the endpoint that the user is redirected in the end of the OAuth2 flow that is initiated by the ExternalSignIn endpoint and will authenticate and authorize the user. 
    /// This endpoint should never be called directly. If somehow the returnUrl is invalid even after this point then the process will fail from the beggining, so make sure that the returnUrl
    /// arguement in the ExternalSignIn domain is valid. Unlike other endpoints the returnUrl parameter in this endpoint is not optional.
    /// </summary>
    /// <param name="returnUrl">Necessary parameter that corresponds to the returnUrl to which the endpoint will redirect the user after the endpoint is done processing. The domain of the return
    /// url need to be registered in the api itself for it to be valid.</param>
    /// <param name="remoteError">Optional parameter that corresponds to potentional errors that might occur in the external providers end.</param>
    /// <returns>If successful it redirects the user to the returnUrl with the newly created AccessToken as a query parameter. If not successful it redirects the user to the returnUrl with an errorMessage 
    /// query parameter and the appropriate error status code. There is an edge case in which the endpoint will not redirect the user, but it will only return a BadRequest and that will only happen
    /// if the returnUrl is not valid and registered in the api.</returns>
    [HttpGet("ExternalSignInCallback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalSignInCallback(string returnUrl, string? remoteError = null)
    {
        try
        {
            //Edge case check, for even more security, because who knows, what someone might be trying to do with some api client.
            if (!CheckIfUrlIsTrusted(returnUrl))
                return BadRequest(new { ErrorMessage = "InvalidReturnUrl" });

            if (remoteError is not null)
                return Redirect($"{returnUrl}?errorMessage=RemoteError");

            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.HandleExternalSignInCallbackAsync();
            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.LoginInfoNotReceivedFromIdentityProvider)
                return Redirect($"{returnUrl}?errorMessage=LoginInfoNotReceivedFromIdentityProvider");
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.EmailClaimNotReceivedFromIdentityProvider)
                return Redirect($"{returnUrl}?errorMessage=EmailClaimNotReceivedFromIdentityProvider");
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Redirect($"{returnUrl}?errorMessage=UserAccountNotActivated");
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Redirect($"{returnUrl}?errorMessage=UserAccountLocked");
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return Redirect($"{returnUrl}?errorMessage=UnknownError");

            return Redirect($"{returnUrl}?accessToken={returnCodeAndTokenResponseModel.Token}");
        }
        catch
        {
            return Redirect($"{returnUrl}?errorMessage=InternalServerError");
        }
    }

    /// <summary>
    /// This endpoint is used to create the reset password confirmEmailToken that can be used to reset the password of an activated account. If the account is not activated the response would return
    /// an appropriate error message. An important part to keep in mind is that this endpoint does not reset the password of the account, but only creates the necessary confirmEmailToken that can then be 
    /// used as an arguement with the ResetPassword endpoint to reset the password of the user account.
    /// </summary>
    /// <param name="forgotPasswordModel">This model consists of the required property email. The email property needs to be correctly formated and valid.</param>
    /// <returns>The newly created reset password confirmEmailToken, which can be used in the ResetPassword endpoint of the api to allow the user to change the account of the password, or it returns BadRequest or other
    /// error status codes based on the error case.</returns>
    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ApiForgotPasswordRequestModel forgotPasswordModel)
    {
        try
        {
            ReturnTokenUserIdAndCodeResponseModel returnTokenUserIdAndCodeResponseModel = await _authenticationProcedures.CreateResetPasswordTokenAsync(forgotPasswordModel.Email!);
            if (returnTokenUserIdAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenEmail)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenEmail" });
            else if (returnTokenUserIdAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return BadRequest(new { ErrorMessage = "UserAccountNotActivated" });

            return Ok(new ApiForgotPasswordResponseModel(returnTokenUserIdAndCodeResponseModel.Token!, returnTokenUserIdAndCodeResponseModel.UserId!));
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used after the forgot password endpoint has produced the necessary reset password confirmEmailToken to change the password of the account and return the newly created access confirmEmailToken to the user.
    /// As was the case with the forgotpassword endpoint if the account is not activated an appropriate error message will be returned and the process will fail.
    /// </summary>
    /// <param name="resetPasswordModel">This model consists of the 3 required properties. The userId, the password reset confirmEmailToken and the password property that contains the new password.</param>
    /// <returns>It returns the newly created access confirmEmailToken to the user or it returns BadRequest, Unauthorized or other error status codes based on the error case.</returns>
    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ApiResetPasswordRequestModel resetPasswordModel)
    {
        try
        {
            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.ResetPasswordAsync(resetPasswordModel.UserId!, resetPasswordModel.Token!, resetPasswordModel.Password!);

            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenEmail" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return BadRequest(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500);
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
            string confirmEmailToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByTokenAsync(confirmEmailToken);

            //this is very unlikely to happen, but someone could craft a valid confirmEmailToken without the user existing.
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
            string confirmEmailToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            AppUser? user = await _authenticationProcedures.GetCurrentUserByTokenAsync(confirmEmailToken);

            //this is very unlikely to happen, but someone could craft a valid confirmEmailToken without the user existing.
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
    /// This endpoint allows a user with a valid access token to change their current password. Appropriate status codes will be returned in case of errors.
    /// </summary>
    /// <param name="changePasswordModel">This models contains 2 required properties. These properties are the old password and the new password, which need to be valid.</param>
    /// <returns>NoContent status code if it succeeds, alternatively it returns BadRequest, Unauthorized or other error status codes based on the error case.</returns>
    [HttpPost("ChangePassword")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ApiChangePasswordRequestModel changePasswordModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            LibraryReturnedCodes libraryReturnedCodes = await _authenticationProcedures.ChangePasswordAsync(token, changePasswordModel.CurrentPassword!, changePasswordModel.NewPassword!);

            if (libraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (libraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (libraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (libraryReturnedCodes == LibraryReturnedCodes.PasswordMissmatch)
                return BadRequest(new { ErrorMessage = "PasswordMismatchError" });
            else if (libraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used to create the necessary email tokens that can be used in a subsequent request at the ConfirmChangeEmail endpoint. It is important to keep in mind that this endpoint only creates the tokens
    /// and deactivates the current account. Thus until the RequestChangeAccountEmail endpoint is successfully called the user can no longer use their account.
    /// </summary>
    /// <param name="changeEmailModel">This model consists of the required property NewEmail, which must be correctly formated.</param>
    /// <returns>If successful it returns ok status code and the necessary confirmEmailToken, which can be used to change the user email using the RequestChangeAccountEmail endpoint, or it returns BadRequest, 
    /// Unauthorized or other error codes depending on the error case.</returns>
    [HttpPost("RequestChangeAccountEmail")]
    [Authorize]
    public async Task<IActionResult> RequestChangeAccountEmail([FromBody] ApiChangeEmailRequestModel changeEmailModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.CreateChangeEmailTokenAsync(token, changeEmailModel.NewEmail!);

            if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });

            return Ok(new { ChangeEmailToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used to activate the new email of the user account using the change email confirmEmailToken that was produced by the RequestChangeAccountEmail endpoint in a previous request.  
    /// This endpoint can be used with an api client without passing in a redirectUrl parameter since api clients in general do not need a redirect link.
    /// The redirectUrl parameter can be used for the typical flow of the application in which way the redirectUrl will be needed to redirect the user correctly to the front end. 
    /// If the latter is the case the front end needs to handle the response, which means it should correctly read the potential errormessage or access confirmEmailToken it will receive from this endpoint.
    /// Another important point is that this endpoint is also one of the few endpoints that can be used without the need of an API key.
    /// </summary>
    /// <param name="userId">Necessary parameter that correspond to the id of the user account</param>
    /// <param name="newEmail">Necessary parameter that must correctly formated and corresponds to the email that will be the new email of the account</param>
    /// <param name="changeEmailToken">Necessary parameter that corresponds to the confirmEmailToken that will be used to change the email of the account. This confirmEmailToken can be produced using the RequestChangeAccountEmail and
    /// must be sent to the ConfirmChangeEmail endpoint as a url encoded query parameter.</param>
    /// <param name="redirectUrl">Optional parameter that specifies the redirectUrl that the endpoint will redirect the user to, with its given response.</param>
    /// <returns>It returns multiple status codes that indicate whether or not the operation was successful or not. If the redirectUrl parameter is not null it will return a Redirect result
    /// which will redirect the user to the given url with the appropriate arguements, otherwise it will return Ok status code with the newly created access confirmEmailToken</returns>
    [HttpGet("ConfirmChangeEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmChangeEmail(string userId, string newEmail, string changeEmailToken, string? redirectUrl = null)
    {
        try
        {
            if (redirectUrl is not null && !CheckIfUrlIsTrusted(redirectUrl))
                return BadRequest(new { ErrorMessage = "InvalidRedirectUrl" });

            ReturnTokenAndCodeResponseModel returnCodeAndTokenResponseModel = await _authenticationProcedures.ChangeEmailAsync(userId, changeEmailToken, newEmail);
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
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });
            else if (returnCodeAndTokenResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InvalidEmailAndEmailChangeTokenCombination)
                return BadRequest(new { ErrorMessage = "InvalidEmailChangeEmailTokenCombination" });

            return Ok(new { AccessToken = returnCodeAndTokenResponseModel.Token });
        }
        catch
        {
            if (redirectUrl is not null)
            {
                var uribBuilder = new UriBuilder(redirectUrl);
                var query = HttpUtility.ParseQueryString(uribBuilder.Query);

                query["errorMessage"] = "InternalServerError";
                uribBuilder.Query = query.ToString();
                return Redirect(uribBuilder.ToString());
            }

            return StatusCode(500);
        }
    }

    /// <summary>
    /// This endpoint is used to delete a user account based on the access confirmEmailToken, which is added in the authorization header when making the request.
    /// </summary>
    /// <returns>It returns No Content status code if operation is successful or BadRequest, Unauthorized or other error status codes depending on the error cases.</returns>
    [HttpDelete("DeleteAccount")]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string token = authorizationHeader.Substring("Bearer ".Length).Trim();

            LibraryReturnedCodes returnedCode = await _authenticationProcedures.DeleteAccountAsync(token);

            if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return Unauthorized(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch
        {
            return StatusCode(500);
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
