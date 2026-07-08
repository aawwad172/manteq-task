namespace ManteqTask.Domain.Interfaces.Domain.Auditing;

public interface ISoftDelete
{
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
