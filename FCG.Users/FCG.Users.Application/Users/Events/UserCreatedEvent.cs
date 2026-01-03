namespace FCG.Users.Application.Users.Events;
public class UserCreatedEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string NickName { get; set; } = default!;
    public string Role { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
