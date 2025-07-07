namespace SmartAIChatbot.Api.Models;

public class ChatHistory
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = "";
    public string Role { get; set; } = "";
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
