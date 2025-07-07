namespace SmartAIChatbot.Api.Models;

public class AskResponse
{
    public string Answer { get; set; }
    public string Source { get; set; } // SQL, Blob, or Both
}
