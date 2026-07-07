namespace TaskFlow.Application.Common.Models;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items       { get; init; } = new List<T>();
    public int              TotalCount  { get; init; }
    public int              Page        { get; init; }
    public int              PageSize    { get; init; }
    public int              TotalPages  => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool             HasNext     => Page < TotalPages;
    public bool             HasPrevious => Page > 1;
}