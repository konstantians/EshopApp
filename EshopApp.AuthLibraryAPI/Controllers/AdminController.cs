using EshopApp.AuthLibrary.AuthLogic;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibraryAPI.Models.RequestModels.AdminModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EshopApp.AuthLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminProcedures _adminProcedures;

    public AdminController(IAdminProcedures adminProcedures)
    {
        _adminProcedures = adminProcedures;
    }

    [HttpGet]
    [Authorize(Policy = "CanManageUsersPolicy")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnUsersAndCodeResponseModel returnUsersAndCodeResponseModel = await _adminProcedures.GetUsersAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanManageUsers") });
            if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            return Ok(returnUsersAndCodeResponseModel.AppUsers);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetUserById/{userId}")]
    [Authorize(Policy = "CanManageUsersPolicy")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnUserAndCodeResponseModel? returnUserAndCodeResponseModel = await _adminProcedures.FindUserByIdAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanManageUsers") }, userId);
            if (returnUserAndCodeResponseModel!.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InsufficientPrivilegesToManageElevatedUser)
                return StatusCode(403, new { ErrorMessage = "InsufficientPrivilegesToManageElevatedUser" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            if (returnUserAndCodeResponseModel.AppUser is null)
                return NotFound();

            return Ok(returnUserAndCodeResponseModel.AppUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetUserByEmail/{email}")]
    [Authorize(Policy = "CanManageUsersPolicy")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnUserAndCodeResponseModel? returnUserAndCodeResponseModel = await _adminProcedures.FindUserByEmailAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanManageUsers") }, email);
            if (returnUserAndCodeResponseModel!.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.InsufficientPrivilegesToManageElevatedUser)
                return StatusCode(403, new { ErrorMessage = "InsufficientPrivilegesToManageElevatedUser" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            if (returnUserAndCodeResponseModel.AppUser is null)
                return NotFound();

            return Ok(returnUserAndCodeResponseModel.AppUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanManageUsersPolicy")]
    public async Task<IActionResult> CreateUserAccount([FromBody] ApiCreateUserRequestModel apiCreateUserRequestModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanManageUsers") };
            ReturnUserAndCodeResponseModel returnUserAndCodeResponseModel = await _adminProcedures.CreateUserAccountAsync(accessToken, expectedClaims, apiCreateUserRequestModel.Email!, apiCreateUserRequestModel.Password!,
                apiCreateUserRequestModel.PhoneNumber!);

            if (returnUserAndCodeResponseModel!.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });
            else if (returnUserAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return CreatedAtAction(nameof(GetUserById), new { userId = returnUserAndCodeResponseModel.AppUser!.Id }, returnUserAndCodeResponseModel.AppUser);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    [Authorize(Policy = "CanManageUsersPolicy")]
    public async Task<IActionResult> UpdateUserAccount([FromBody] ApiUpdateUserRequestModel apiUpdateUserRequestModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanManageUsers") };
            LibraryReturnedCodes returnedCode = await _adminProcedures.UpdateUserAccountAsync(accessToken, expectedClaims, apiUpdateUserRequestModel.AppUser!,
                apiUpdateUserRequestModel.ActivateEmail, apiUpdateUserRequestModel.Password);

            if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.InsufficientPrivilegesToManageElevatedUser)
                return StatusCode(403, new { ErrorMessage = "InsufficientPrivilegesToManageElevatedUser" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnedCode == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "UserNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.DuplicateEmail)
                return BadRequest(new { ErrorMessage = "DuplicateEmail" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{userId}")]
    [Authorize(Policy = "CanManageUsersPolicy")]
    public async Task<IActionResult> DeleteUserAccount(string userId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanManageUsers") };
            LibraryReturnedCodes returnedCode = await _adminProcedures.DeleteUserAccountAsync(accessToken, expectedClaims, userId);

            if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return StatusCode(403, new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.InsufficientPrivilegesToManageElevatedUser)
                return StatusCode(403, new { ErrorMessage = "InsufficientPrivilegesToManageElevatedUser" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnedCode == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "UserNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
