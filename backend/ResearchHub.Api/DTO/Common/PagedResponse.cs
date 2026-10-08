namespace ResearchHub.Api.DTO.Common;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public static PagedResponse<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var list = source.ToList();
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch { < 1 => 20, > 100 => 100, _ => pageSize };
        var total = list.Count;
        var items = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        return new PagedResponse<T>(items, page, pageSize, total, totalPages);
    }
}
