namespace FCG.Users.Application.Auth.UseCases.Queries.LoginUserQuery;
public class LoginUserQueryOutput
{
    public string Token { get; set; } = default!;
    public int UserId { get; set; }
    public string Role { get; set; } = default!;
}
