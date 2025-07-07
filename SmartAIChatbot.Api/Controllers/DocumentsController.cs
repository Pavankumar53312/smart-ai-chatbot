using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAIChatbot.Api.Data;
using SmartAIChatbot.Api.Helper;
using SmartAIChatbot.Api.Models;
using SmartAIChatbot.Api.Services;
using System.Security.Claims;
using System.Text.Json;

namespace SmartAIChatbot.Api.Controllers;

[ApiController]
[Route("api/documents")]
public class DocumentController : ControllerBase
{
    private readonly FormRecognizerService _formRecognizer;
    private readonly AzureOpenAIService _openAI;
    private readonly BlobContainerClient _container;
    private readonly AppDbContext _db;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(
        FormRecognizerService formRecognizer,
        AzureOpenAIService openAI,
        BlobContainerClient container,
        AppDbContext db,
        ILogger<DocumentController> logger)
    {
        _formRecognizer = formRecognizer;
        _openAI = openAI;
        _container = container;
        _db = db;
        _logger = logger;
    }

    /* ---------- Admin Upload ---------- */
    [Authorize(Roles = "Admin")]
    [HttpPost("upload-to-blob")]
    public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            /* 1️⃣  Upload to Blob ------------------------------------------------ */
            var blobClient = _container.GetBlobClient(file.FileName);
            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            /* 2️⃣  Extract text -------------------------------------------------- */
            stream.Position = 0;                      // rewind stream for reading

            string text;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (ext == ".txt" || ext == ".md" || ext == ".csv")
            {
                // already plain text ― read directly
                using var reader = new StreamReader(stream);
                text = await reader.ReadToEndAsync();
            }
            else
            {
                // OCR / layout extraction via Form Recognizer
                text = await _formRecognizer.ExtractTextAsync(stream);
            }

            /* 3️⃣  Chunk + Embed ------------------------------------------------- */
            // Get intended role from header (default = general)
            var roleHeader = Request.Headers["x-document-role"].ToString().ToLower();
            var role = string.IsNullOrWhiteSpace(roleHeader) ? "general" : roleHeader;
            var chunks = EmbeddingHelper.SplitIntoChunks(text, 500);

            foreach (var chunk in chunks)
            {
                var vec = await _openAI.GetEmbeddingAsync(chunk);
                _db.Embeddings.Add(new DocumentEmbedding
                {
                    FileName = file.FileName,
                    Role = role,
                    Chunk = chunk,
                    EmbeddingJson = JsonSerializer.Serialize(vec)
                });
            }
            await _db.SaveChangesAsync();

            return Ok(new { message = "Uploaded & indexed", file = file.FileName });
        }
        catch (Azure.RequestFailedException ex)
        {
            _logger.LogError(ex, "Azure service error");
            return StatusCode(503, new { message = "Azure service unavailable. Please try again later." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected upload error");
            return StatusCode(500, new { message = ex.Message });
        }
    }


    /* ---------- List docs (handle SQL/offline) ---------------- */
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        try
        {
            var docs = await _db.Embeddings
                .GroupBy(e => e.FileName)
                .Select(g => new {
                    FileName = g.Key,
                    Chunks = g.Count(),
                    Role = g.Min(e => e.Role)
                })
                .ToListAsync();

            return Ok(docs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch document list");
            return StatusCode(503, new { message = "Document list unavailable." });
        }
    }


    // DELETE api/documents/{fileName}
    [Authorize(Roles = "Admin")]
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> Delete(string fileName)
    {
        var chunks = _db.Embeddings.Where(e => e.FileName == fileName);
        _db.Embeddings.RemoveRange(chunks);
        await _db.SaveChangesAsync();

        // If you stored the physical file locally, also delete it:
        // System.IO.File.Delete(Path.Combine("Uploads", fileName));

        return NoContent();
    }

}
