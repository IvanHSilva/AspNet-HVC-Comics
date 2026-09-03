namespace HVC_Comics.Models;

public class PaginationResult<T>
{
    public List<T> Items { get; set; } = [];

    public int CurrentPage { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public string DataSource { get; set; } = string.Empty;

    public int TotalPages =>
        (int)Math.Ceiling((double)TotalRecords / PageSize);
}
