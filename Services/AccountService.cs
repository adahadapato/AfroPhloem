using Phloem.Services.Api;
using Phloem.Services.Api.Contracts.Commands;
using Phloem.Services.Api.Contracts.Dtos;

namespace Phloem.Services;

/// <summary>
/// The signed-in user's own account: <c>/api/me</c>. Profile, addresses and personal details are added here as
/// their screens are wired.
/// </summary>
public sealed class AccountService(ApiClient api)
{
    /// <summary>The signed-in user's profile, addresses and settings.</summary>
    /// <param name="ct">Cancellation.</param>
    /// <returns>The profile; or the API's error (e.g. 401 when not signed in).</returns>
    public Task<ApiResult<ProfileDto>> GetProfileAsync(CancellationToken ct = default) =>
        api.GetAsync<ProfileDto>("api/me", ct);

    /// <summary>Saves a delivery address to the account.</summary>
    /// <param name="command">Label, full address, country and whether it becomes the default.</param>
    /// <param name="ct">Cancellation.</param>
    /// <returns>The new address's Id; or the API's error (e.g. a missing field).</returns>
    public Task<ApiResult<Guid>> AddAddressAsync(AddAddressCommand command, CancellationToken ct = default) =>
        api.PostAsync<Guid>("api/me/addresses", command, ct);
}
