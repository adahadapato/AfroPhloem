namespace Phloem.Services.Api.Contracts;

/// <summary>One page of a longer list, as the API returns it.</summary>
/// <typeparam name="T">What the list holds.</typeparam>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    /// <summary>True when there are more items after this page.</summary>
    public bool HasNext => Page * PageSize < TotalCount;
}
