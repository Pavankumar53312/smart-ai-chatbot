using Microsoft.EntityFrameworkCore;
using SmartAIChatbot.Api.Models;

namespace SmartAIChatbot.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<KnowledgeItem> KnowledgeBase => Set<KnowledgeItem>();
        public DbSet<DocumentEmbedding> Embeddings => Set<DocumentEmbedding>();
        public DbSet<ChatHistory> ChatHistories => Set<ChatHistory>();

        // (optional) OnModelCreating if you need configuration
    }
}
