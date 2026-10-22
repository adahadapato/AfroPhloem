namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of "review a document"; a reason is needed to reject. Same as the API's <c>ReviewRequest</c>.</summary>
public sealed record ReviewRequest(bool Accept, string? Reason);
