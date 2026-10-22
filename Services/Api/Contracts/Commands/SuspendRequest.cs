namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>
/// Body of <c>POST /api/admin/users/{id}/suspend</c>; <see cref="UntilUtc"/> null = until further notice.
/// Same as the API's <c>SuspendRequest</c>.
/// </summary>
public sealed record SuspendRequest(string Reason, DateTime? UntilUtc);
