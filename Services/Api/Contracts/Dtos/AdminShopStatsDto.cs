namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>How many shops are in each status. Same shape as the API's <c>AdminShopStatsDto</c>.</summary>
public sealed record AdminShopStatsDto(int PendingApproval, int Approved, int Rejected, int Blocked, int Total);
