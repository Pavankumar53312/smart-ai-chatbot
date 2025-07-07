namespace SmartAIChatbot.Api.Services;

public class UserService
{
    private readonly List<(string Email, string Password, string Role)> _users = new()
    {
        ("hr@company.com", "password123", "HR"),
        ("it@company.com", "password123", "IT"),
        ("finance@company.com", "password123", "FINANCE"),
        ("admin@company.com", "adminpass", "Admin"),
    };

    public (bool IsValid, string Role) ValidateUser(string email, string password)
    {
        var user = _users.FirstOrDefault(u => u.Email == email && u.Password == password);
        return user == default ? (false, null) : (true, user.Role);
    }
}
