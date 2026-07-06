namespace ManteqTask.Domain.Entities;

public class PaginationResult<T>
{
    public int TotalDisplayRecords { get; set; }
    public int TotalRecords { get; set; }
    public IEnumerable<T>? Page { get; set; }
}
