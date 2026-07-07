namespace TaskFlow.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId      { get; }
    string? WorkspaceId { get; }
    string? Role        { get; }
    bool    IsAuthenticated { get; }
}