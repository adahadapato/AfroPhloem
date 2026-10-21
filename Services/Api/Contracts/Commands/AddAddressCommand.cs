using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Commands;

/// <summary>Body of <c>POST /api/me/addresses</c>. Same shape as the API's <c>AddAddressCommand</c>.</summary>
public sealed record AddAddressCommand(string Label, string FullAddress, Country Country, bool MakeDefault);
