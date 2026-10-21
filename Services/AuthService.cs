using Microsoft.Extensions.Logging;
using Phloem.Services.Api;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Commands;
using Phloem.Services.Api.Contracts.Dtos;

namespace Phloem.Services;

/// <summary>
/// Signing in and out: <c>/api/auth</c>. After a successful sign-in it keeps the tokens (<see cref="TokenStore"/>),
/// loads the profile (<see cref="AccountService"/>) and fills <see cref="SessionService"/>, so every screen knows
/// who is signed in.
/// </summary>
public sealed class AuthService(
    ApiClient api,
    TokenStore tokens,
    AccountService account,
    SessionService session,
    ILogger<AuthService> logger)
{
    // ───────────────────────────── 1. Sign in ─────────────────────────────

    /// <summary>Email + password.</summary>
    /// <param name="email">The account's email.</param>
    /// <param name="password">The password.</param>
    /// <param name="remember">Stay signed in after the app closes.</param>
    /// <returns>
    /// The API's answer. When <see cref="AuthTokensDto.RequiresTwoFactor"/> is true the user is NOT signed in yet:
    /// ask for the code and call <see cref="CompleteTwoFactorAsync"/>.
    /// </returns>
    public async Task<ApiResult<AuthTokensDto>> LoginAsync(string email, string password, bool remember)
    {
        var login = await api.PostAsync<AuthTokensDto>("api/auth/login", new LoginCommand(email.Trim(), password));
        if (login.IsFailure || login.Value!.RequiresTwoFactor) return login;

        var signedIn = await SignInAsync(login.Value, remember);
        return signedIn.IsSuccess ? login : signedIn.Error!;
    }

    /// <summary>Second step of a two-factor sign-in.</summary>
    /// <param name="challengeToken">From the login answer.</param>
    /// <param name="code">The 6-digit code, or a recovery code.</param>
    /// <param name="isRecoveryCode">True when a recovery code was typed.</param>
    /// <param name="remember">Stay signed in after the app closes.</param>
    public async Task<ApiResult> CompleteTwoFactorAsync(string challengeToken, string code, bool isRecoveryCode,
                                                        bool remember)
    {
        var result = await api.PostAsync<AuthTokensDto>("api/auth/login/two-factor",
            new CompleteTwoFactorLoginCommand(challengeToken, code.Trim(), isRecoveryCode));
        return result.IsFailure ? result.Error! : await SignInAsync(result.Value!, remember);
    }

    /// <summary>Creates the account and signs straight in.</summary>
    /// <param name="command">The sign-up form.</param>
    public async Task<ApiResult> RegisterAsync(RegisterCommand command)
    {
        var result = await api.PostAsync<AuthTokensDto>("api/auth/register", command);
        return result.IsFailure ? result.Error! : await SignInAsync(result.Value!, remember: true);
    }

    // ───────────────────────────── 2. Session ─────────────────────────────

    /// <summary>
    /// App start: if the user ticked "Remember me" last time, renews the sign-in and loads the profile.
    /// </summary>
    /// <returns>True when the user is signed in again.</returns>
    public async Task<bool> RestoreSessionAsync()
    {
        if (!await tokens.LoadAsync()) return false;

        // The token handler renews the tokens on the way if they ran out
        var profile = await account.GetProfileAsync();
        if (profile.IsSuccess)
        {
            session.SignIn(profile.Value!, tokens.Roles);
            return true;
        }

        // Rejected = the sign-in is over; no connection = keep the tokens and try again next start
        if (profile.Error!.IsUnauthorized) await tokens.ClearAsync();
        logger.LogInformation("Couldn't restore the sign-in: {Code}.", profile.Error.Code);
        return false;
    }

    /// <summary>Reloads the profile (e.g. after opening a shop or changing details).</summary>
    public async Task<ApiResult> RefreshProfileAsync()
    {
        var profile = await account.GetProfileAsync();
        if (profile.IsFailure) return profile.Error!;

        session.SignIn(profile.Value!, tokens.Roles);
        return ApiResult.Success();
    }

    /// <summary>Signs out on this device and cancels the refresh token on the server.</summary>
    public async Task LogoutAsync()
    {
        if (tokens.RefreshToken is { } refreshToken)
        {
            // Best effort: the user is signed out here even if the server can't be reached
            var revoked = await api.PostAsync("api/auth/logout", new RevokeTokenCommand(refreshToken));
            if (revoked.IsFailure) logger.LogInformation("Logout not confirmed by the API: {Code}.", revoked.Error!.Code);
        }

        await tokens.ClearAsync();
        session.SignOut();
    }

    // ───────────────────────────── 3. Password ─────────────────────────────

    /// <summary>Emails a reset link (the API answers the same whether or not the account exists).</summary>
    /// <param name="email">The account's email.</param>
    public Task<ApiResult> ForgotPasswordAsync(string email) =>
        api.PostAsync("api/auth/forgot-password", new ForgotPasswordCommand(email.Trim()));

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>Keeps the tokens, then loads the profile into the session.</summary>
    private async Task<ApiResult> SignInAsync(AuthTokensDto tokensDto, bool remember)
    {
        await tokens.SaveAsync(tokensDto, remember);

        var profile = await account.GetProfileAsync();
        if (profile.IsFailure)
        {
            await tokens.ClearAsync();
            return profile.Error!;
        }

        session.SignIn(profile.Value!, tokens.Roles);
        return ApiResult.Success();
    }
}
