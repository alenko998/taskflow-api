using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public record ProjectCreatedEvent(Guid ProjectId, string Name, string CreatedById) : IDomainEvent;