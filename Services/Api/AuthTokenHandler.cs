using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Phloem.Services.Api.Contracts;
using Phloem.Services.Api.Contracts.Dtos;

namespace Phloem.Services.Api;

/// <summary>
/// Sits in front of every API request:
/// <list type="number">
///   <item><description><b>Before:</b> if the access token is about to run out, renews it first; then adds
///   "Authorization: Bearer …".</description></item>
///   <item><description><b>After:</b> on 401, renews the tokens ONCE and sends the request again.</description></item>
///   <item><description><b>Renewal fails</b> (refresh token expired or revoked): signs the user out
///   (<see cref="TokenStore.SessionExpired"/>) and returns the 401.</description></item>
/// </list>
/// </summary>
/// <remarks>Renewal uses a separate HttpClient (<see cref="ApiClient.RefreshClientName"/>) without this handler,
/// so it can never loop.</remarks>
public sealed class AuthTokenHandler(
    TokenStore tokens,
    IHttpClientFactory httpClientFactory,
    ILogger<AuthTokenHandler> logger) : DelegatingHandler
{
    /// <summary>The API's refresh endpoint, relative to its base address.</summary>
    private const string RefreshPath = "api/auth/refresh";

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
                                                                 CancellationToken cancellationToken)
    {
        // 1. Renew first if the token is about to run out (saves a wasted round trip)
        if (tokens.CanRefresh && tokens.AccessTokenExpiresSoon)
            await RenewAsync(tokens.AccessToken, cancellationToken);

        // The body may need sending twice (after a renewal), so keep a copy
        if (request.Content is not null)
            await request.Content.LoadIntoBufferAsync(cancellationToken);

        AddToken(request);
        var response = await base.SendAsync(request, cancellationToken);

        // 2. Rejected: renew once and try again
        if (response.StatusCode != HttpStatusCode.Unauthorized || !tokens.CanRefresh)
            return response;

        var tokenUsed = request.Headers.Authorization?.Parameter;
        if (!await RenewAsync(tokenUsed, cancellationToken))
            return response;

        using var retry = await CloneAsync(request, cancellationToken);
        AddToken(retry);
        response.Dispose();
        return await base.SendAsync(retry, cancellationToken);
    }

    /// <summary>Adds the current access token, if there is one.</summary>
    private void AddToken(HttpRequestMessage request) =>
        request.Headers.Authorization = tokens.AccessToken is null
            ? null
            : new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

    /// <summary>
    /// Swaps the refresh token for new tokens. Only one renewal runs at a time; a request that waited finds the
    /// tokens already renewed and doesn't renew again.
    /// </summary>
    /// <param name="staleAccessToken">The token the caller had; if it changed meanwhile, someone else renewed.</param>
    /// <returns>True when there is a fresh access token to use.</returns>
    private async Task<bool> RenewAsync(string? staleAccessToken, CancellationToken cancellationToken)
    {
        await tokens.RefreshLock.WaitAsync(cancellationToken);
        try
        {
            // Another request renewed while this one waited
            if (tokens.AccessToken is not null && tokens.AccessToken != staleAccessToken && !tokens.AccessTokenExpiresSoon)
                return true;

            if (!tokens.CanRefresh) return false;

            var client = httpClientFactory.CreateClient(ApiClient.RefreshClientName);
            using var response = await client.PostAsJsonAsync(RefreshPath, new RefreshTokenCommand(tokens.RefreshToken!),
                                                              ApiJson.Options, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var renewed = await response.Content.ReadFromJsonAsync<AuthTokensDto>(ApiJson.Options, cancellationToken);
                if (renewed?.AccessToken is not null)
                {
                    await tokens.SaveAsync(renewed, tokens.Remember);
                    return true;
                }
            }

            // The API refused the refresh token (expired, revoked, reused): sign out
            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                logger.LogInformation("Sign-in could not be renewed ({Status}); signing out.", (int)response.StatusCode);
                await tokens.ExpireAsync();
            }

            return false;
        }
        catch (HttpRequestException ex)
        {
            // No connection: keep the tokens, the user is still signed in once the network is back
            logger.LogWarning(ex, "Couldn't reach the API to renew the sign-in.");
            return false;
        }
        finally
        {
            tokens.RefreshLock.Release();
        }
    }

    /// <summary>A copy of a request (a request can't be sent twice).</summary>
    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        foreach (var header in request.Headers.Where(h => h.Key != "Authorization"))
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content is not null)
        {
            var body = await request.Content.ReadAsByteArrayAsync(ct);
            clone.Content = new ByteArrayContent(body);
            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
