using TaskFlow.Domain.Common;
using TaskStatus = TaskFlow.Domain.Enums.TaskStatus;

namespace TaskFlow.Domain.Events;

public record TaskStatusChangedEvent(Guid TaskId, TaskStatus OldStatus, TaskStatus NewStatus, string UserId) : IDomainEvent;