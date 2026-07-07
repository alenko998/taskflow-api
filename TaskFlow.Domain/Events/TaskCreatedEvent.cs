using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Events;

public record TaskCreatedEvent(Guid TaskId, string Title, Guid ProjectId, string? AssigneeId) : IDomainEvent;