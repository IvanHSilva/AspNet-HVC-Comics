namespace HVC_Comics.Models;

public class PaginationResult<T>
{
    public List<T> Items { get; set; } = new();

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalRecords / PageSize);
}
