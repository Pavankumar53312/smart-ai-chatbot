using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAIChatbot.Api.Data;
using SmartAIChatbot.Api.Models;
using SmartAIChatbot.Api.Services;
using System.Security.Claims;

namespace SmartAIChatbot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;
    private readonly AppDbContext _db;

    public ChatController(ChatService chatService, AppDbContext db)
    {
        _chatService = chatService;
        _db = db;
    }

    [Authorize]
    [HttpPost("ask")]
    public async Task<ActionResult<AskResponse>> Ask([FromBody] AskRequest req)
    {
        var email = User.Identity?.Name ?? "anonymous";
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "General";

        var resp = await _chatService.GetAnswerAsync(req.Question, role);

        // ⬇️ save history
        _db.ChatHistories.Add(new ChatHistory
        {
            UserEmail = email,
            Role = role,
            Question = req.Question,
            Answer = resp.Answer
        });
        await _db.SaveChangesAsync();

        return Ok(resp);
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        var email = User.Identity?.Name!;
        var history = await _db.ChatHistories
            .Where(h => h.UserEmail == email)
            .OrderByDescending(h => h.Timestamp)
            .Take(50)
            .ToListAsync();

        return Ok(history);
    }


}
