using Phloem.Services.Api.Contracts.Enums;

namespace Phloem.Services.Api.Contracts.Dtos;

/// <summary>One saved delivery address, as the API returns it.</summary>
public sealed record AddressDto(Guid Id, string Label, string FullAddress, Country Country, bool IsDefault);
