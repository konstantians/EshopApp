using EshopApp.AuthLibrary.AuthLogic;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.Models.ResponseModels.RoleManagementModels;
using EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EshopApp.AuthLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class RoleController : ControllerBase
{
    private readonly RoleManagementProcedures _roleManagementProcedures;

    public RoleController(RoleManagementProcedures roleManagementProcedures)
    {
        _roleManagementProcedures = roleManagementProcedures;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    public async Task<IActionResult> GetRoles()
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnRolesAndCodeResponseModel returnRolesAndCodeResponseModel = await _roleManagementProcedures.GetRolesAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanViewUserRoles") });
            if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            return Ok(returnRolesAndCodeResponseModel.AppRoles);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetRoleById/{roleId}")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    public async Task<IActionResult> GetRoleById(string roleId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnRoleAndCodeResponseModel returnRoleAndCodeResponseModel = await _roleManagementProcedures.GetRoleByIdAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanViewUserRoles") }, roleId);
            if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            if(returnRoleAndCodeResponseModel.AppRole is null)
                return NotFound();

            return Ok(returnRoleAndCodeResponseModel.AppRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetRoleByName/{roleName}")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    public async Task<IActionResult> GetRoleByName(string roleName)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnRoleAndCodeResponseModel returnRoleAndCodeResponseModel = await _roleManagementProcedures.GetRoleByIdAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanViewUserRoles") }, roleName);
            if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });

            if (returnRoleAndCodeResponseModel.AppRole is null)
                return NotFound();

            return Ok(returnRoleAndCodeResponseModel.AppRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetRolesOfUser/{userId}")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    public async Task<IActionResult> GetRolesOfUser(string userId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnRolesAndCodeResponseModel returnRolesAndCodeResponseModel = await _roleManagementProcedures.GetRolesAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanViewUserRoles") });
            if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnRolesAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenId" });

            return Ok(returnRolesAndCodeResponseModel.AppRoles);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    [Authorize(Policy = "CanEditUserRolesPolicy")]
    public async Task<IActionResult> CreateRole([FromBody] ApiCreateRoleRequestModel apiCreateRoleRequestModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanViewUserRoles"), new Claim("Permission", "CanEditUserRoles") };
            ReturnRoleAndCodeResponseModel returnRoleAndCodeResponseModel = await _roleManagementProcedures.CreateRoleAsync(accessToken, expectedClaims,
                apiCreateRoleRequestModel.RoleName!, apiCreateRoleRequestModel.Claims);
            if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.DuplicateRole)
                return BadRequest(new { ErrorMessage = "DuplicateRole" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return CreatedAtAction(nameof(GetRoleById), new { userId = returnRoleAndCodeResponseModel.AppRole!.Id }, returnRoleAndCodeResponseModel.AppRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    [Authorize(Policy = "CanEditUserRolesPolicy")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanViewUserRoles"), new Claim("Permission", "CanEditUserRoles") };
            LibraryReturnedCodes returnCode = await _roleManagementProcedures.DeleteRoleAsync(accessToken, expectedClaims, roleId);
            if (returnCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnCode == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnCode == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnCode == LibraryReturnedCodes.RoleNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "RoleNotFoundWithGivenId" });
            else if (returnCode == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetUsersOfRole/{roleId}")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    public async Task<IActionResult> GetUsersOfRole(string roleId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnUsersAndCodeResponseModel returnUsersAndCodeResponseModel = await _roleManagementProcedures.GetUsersOfGivenRoleAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanViewUserRoles") }, roleId);
            if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnUsersAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.RoleNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "RoleNotFoundWithGivenId" });

            return Ok(returnUsersAndCodeResponseModel.AppUsers);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("AddRoleToUser")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    [Authorize(Policy = "CanEditUserRolesPolicy")]
    public async Task<IActionResult> AddRoleToUser([FromBody] ApiAddRoleToUserRequestModel apiAddRoleToUserRequestModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanViewUserRoles"), new Claim("Permission", "CanEditUserRoles") };
            LibraryReturnedCodes returnedCode = await _roleManagementProcedures.AddRoleToUserAsync(accessToken, expectedClaims, 
                apiAddRoleToUserRequestModel.UserId!, apiAddRoleToUserRequestModel.RoleId!);
            
            if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode== LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnedCode == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.RoleNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "RoleNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("ReplaceRoleOfUser")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    [Authorize(Policy = "CanEditUserRolesPolicy")]
    public async Task<IActionResult> ReplaceRoleOfUser([FromBody] ApiReplaceRoleOfUserRequestModel apiReplaceRoleOfUserRequestModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanViewUserRoles"), new Claim("Permission", "CanEditUserRoles") };
            LibraryReturnedCodes returnedCode = await _roleManagementProcedures.ReplaceRoleOfUserAsync(accessToken, expectedClaims,
                apiReplaceRoleOfUserRequestModel.UserId!, apiReplaceRoleOfUserRequestModel.CurrentRoleId!, apiReplaceRoleOfUserRequestModel.NewRoleId!);

            if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnedCode == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.RoleNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "RoleNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("RemoveRoleFromUser/{userId}/role/{roldId}")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    [Authorize(Policy = "CanEditUserRolesPolicy")]
    public async Task<IActionResult> RemoveRoleFromUser(string userId, string roleId)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanViewUserRoles"), new Claim("Permission", "CanEditUserRoles") };
            LibraryReturnedCodes returnedCode = await _roleManagementProcedures.RemoveRoleFromUserAsync(accessToken, expectedClaims, userId, roleId);

            if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnedCode == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnedCode == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnedCode == LibraryReturnedCodes.UserNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "UserNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.RoleNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "RoleNotFoundWithGivenId" });
            else if (returnedCode == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("GetClaims")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    public async Task<IActionResult> GetClaimsInSystem()
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            ReturnClaimsAndCodeResponseModel returnClaimsAndCodeResponseModel = await _roleManagementProcedures.GetAllUniqueClaimsInSystemAsync(accessToken, new List<Claim>() { new Claim("Permission", "CanViewUserRoles") });
            if (returnClaimsAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnClaimsAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnClaimsAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnClaimsAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnClaimsAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });


            return Ok(returnClaimsAndCodeResponseModel.Claims);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("UpdateClaimsOfRole")]
    [Authorize(Policy = "CanViewUserRolesPolicy")]
    [Authorize(Policy = "CanEditUserRolesPolicy")]
    public async Task<IActionResult> UpdateClaimsOfRole([FromBody] ApiUpdateClaimsOfRoleRequestModel apiUpdateClaimsOfRoleRequestModel)
    {
        try
        {
            string authorizationHeader = HttpContext.Request.Headers["Authorization"]!;
            string accessToken = authorizationHeader.Substring("Bearer ".Length).Trim();

            List<Claim> expectedClaims = new List<Claim>() { new Claim("Permission", "CanViewUserRoles"), new Claim("Permission", "CanEditUserRoles") };
            ReturnRoleAndCodeResponseModel returnRoleAndCodeResponseModel = await _roleManagementProcedures.UpdateClaimsOfRoleAsync(accessToken, expectedClaims,
                apiUpdateClaimsOfRoleRequestModel.RoleId!, apiUpdateClaimsOfRoleRequestModel.NewClaims);

            if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButUserNotInRoleInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButUserNotInRoleInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.ValidTokenButClaimNotInSystem)
                return Unauthorized(new { ErrorMessage = "ValidTokenButClaimNotInSystem" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountNotActivated)
                return Unauthorized(new { ErrorMessage = "UserAccountNotActivated" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UserAccountLocked)
                return Unauthorized(new { ErrorMessage = "UserAccountLocked" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.RoleNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "RoleNotFoundWithGivenId" });
            else if (returnRoleAndCodeResponseModel.LibraryReturnedCodes == LibraryReturnedCodes.UnknownError)
                return BadRequest(new { ErrorMessage = "UnknownError" });

            return Ok(returnRoleAndCodeResponseModel.AppRole);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
