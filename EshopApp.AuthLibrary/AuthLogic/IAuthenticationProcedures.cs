﻿using EshopApp.AuthLibrary.Models;
using EshopApp.AuthLibrary.Models.ResponseModels;
using EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationModels;
using EshopApp.AuthLibrary.Models.ResponseModels.AuthenticationProceduresModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.AuthLogic;

public interface IAuthenticationProcedures
{
    Task<ReturnTokenAndCodeResponseModel> ChangeEmailAsync(string userId, string changeEmailToken, string newEmail);
    Task<LibraryReturnedCodes> ChangePasswordAsync(string accessToken, string currentPassword, string newPassword);
    Task<ReturnTokenAndCodeResponseModel> ConfirmEmailAsync(string userId, string confirmationToken);
    Task<ReturnTokenAndCodeResponseModel> CreateChangeEmailTokenAsync(string accessToken, string newEmail);
    Task<ReturnTokenUserIdAndCodeResponseModel> CreateResetPasswordTokenAsync(string email);
    Task<LibraryReturnedCodes> DeleteAccountAsync(string accessToken);
    Task<ReturnUserAndCodeResponseModel> GetCurrentUserByTokenAsync(string token);
    Task<LibSignUpResponseModel> SignUpAsync(string email, string phoneNumber, string password);
    Task<ReturnTokenAndCodeResponseModel> ResetPasswordAsync(string userId, string resetPasswordToken, string newPassword);
    Task<ReturnTokenAndCodeResponseModel> SignInAsync(string username, string password, bool isPersistent);
    //Task<bool> UpdateAccountAsync(AppUser appUser);
    AuthenticationProperties GetExternalIdentityProvidersProperties(string identityProviderName, string redirectUrl);
    Task<ReturnTokenAndCodeResponseModel> HandleExternalSignInCallbackAsync();
    Task<IEnumerable<AuthenticationScheme>> GetExternalIdentityProvidersAsync();
    Task<ReturnUserAndCodeResponseModel> GetCurrentUserWithValidatedClaimsByTokenAsync(string accessToken, List<Claim> expectedClaims);
    Task<ReturnUserAndCodeResponseModel> CheckResetPasswordEligibilityForGivenUserId(string userId, string resetPasswordToken);
    Task<LibraryReturnedCodes> UpdateAccountAsync(string accessToken, AppUser appUser);
}