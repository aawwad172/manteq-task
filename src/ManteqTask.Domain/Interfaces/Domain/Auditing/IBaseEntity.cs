namespace ManteqTask.Domain.Interfaces.Domain.Auditing;

public interface IBaseEntity : IEntity, ICreationAudit, IModificationAudit, ISoftDelete
{

}
