using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using TaskFlow.Application.Common.Interfaces;

namespace TaskFlow.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly string _host;
    private readonly int    _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _fromName;

    public EmailService(IConfiguration config)
    {
        _config   = config;
        _host     = config["EmailSettings:Host"]!;
        _port     = int.Parse(config["EmailSettings:Port"]!);
        _username = config["EmailSettings:Username"]!;
        _password = config["EmailSettings:Password"]!;
        _fromName = config["EmailSettings:FromName"]!;
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _username));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(_host, _port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_username, _password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendEmailVerificationAsync(string email, string firstName, string userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"http://localhost:5173/verify-email?userId={userId}&token={encodedToken}";
        var html = $@"
            <h2>Welcome to TaskFlow, {firstName}!</h2>
            <p>Please verify your email by clicking below:</p>
            <a href='{link}' style='background:#6C63FF;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;display:inline-block;'>
                Verify Email
            </a>
            <p>This link expires in 24 hours.</p>";
        await SendAsync(email, firstName, "Verify your email — TaskFlow", html);
    }

    public async Task SendPasswordResetAsync(string email, string firstName, string userId, string token)
    {
        var encodedToken = Uri.EscapeDataString(token);
        var link = $"http://localhost:5173/reset-password?userId={userId}&token={encodedToken}";
        var html = $@"
            <h2>Hi {firstName}, reset your password</h2>
            <a href='{link}' style='background:#6C63FF;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;display:inline-block;'>
                Reset Password
            </a>
            <p>This link expires in 1 hour.</p>";
        await SendAsync(email, firstName, "Reset your password — TaskFlow", html);
    }

    public async Task SendWorkspaceInvitationAsync(string email, string inviterName, string workspaceName, string token)
    {
        var link = $"http://localhost:5173/accept-invitation?token={token}";
        var html = $@"
            <h2>You've been invited to {workspaceName}!</h2>
            <p>{inviterName} has invited you to join their workspace on TaskFlow.</p>
            <a href='{link}' style='background:#6C63FF;color:white;padding:12px 24px;text-decoration:none;border-radius:4px;display:inline-block;'>
                Accept Invitation
            </a>
            <p>This link expires in 7 days.</p>";
        await SendAsync(email, email, $"You're invited to {workspaceName} — TaskFlow", html);
    }

    public async Task SendTaskAssignedAsync(string email, string firstName, string taskTitle, string projectName)
    {
        var html = $@"
            <h2>Hi {firstName}, you have a new task!</h2>
            <p><strong>Task:</strong> {taskTitle}</p>
            <p><strong>Project:</strong> {projectName}</p>
            <p>Log in to TaskFlow to view the details.</p>";
        await SendAsync(email, firstName, $"New task assigned — {taskTitle}", html);
    }

    public async Task SendTaskStatusChangedAsync(string email, string firstName, string taskTitle, string newStatus)
    {
        var html = $@"
            <h2>Hi {firstName}, task update!</h2>
            <p><strong>Task:</strong> {taskTitle}</p>
            <p><strong>New Status:</strong> {newStatus}</p>";
        await SendAsync(email, firstName, $"Task updated — {taskTitle}", html);
    }
}