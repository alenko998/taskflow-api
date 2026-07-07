using TaskFlow.Domain.Common;

namespace TaskFlow.Domain.Entities;

public class Comment : BaseEntity
{
    public string  Content    { get; private set; } = string.Empty;
    public Guid    TaskItemId { get; private set; }
    public string  AuthorId   { get; private set; } = string.Empty;

    public TaskItem Task   { get; private set; } = null!;
    public User     Author { get; private set; } = null!;

    private Comment() { }

    public static Comment Create(string content, Guid taskItemId, string authorId)
    {
        return new Comment
        {
            Content    = content,
            TaskItemId = taskItemId,
            AuthorId   = authorId,
        };
    }

    public void Update(string content)
    {
        Content = content;
        SetUpdatedAt();
    }
}