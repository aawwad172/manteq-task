namespace ManteqTask.Domain.Entities;

public class PaginationResult<T>
{
    public int TotalDisplayRecords { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public IEnumerable<T>? Page { get; set; }
}
