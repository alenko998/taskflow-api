namespace TaskFlow.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string firstName, string userId, string token);
    Task SendPasswordResetAsync(string email, string firstName, string userId, string token);
    Task SendWorkspaceInvitationAsync(string email, string inviterName, string workspaceName, string token);
    Task SendTaskAssignedAsync(string email, string firstName, string taskTitle, string projectName);
    Task SendTaskStatusChangedAsync(string email, string firstName, string taskTitle, string newStatus);
}