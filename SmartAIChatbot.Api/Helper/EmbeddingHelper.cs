using System.Text.RegularExpressions;
using System.Text;

namespace SmartAIChatbot.Api.Helper
{
    public class EmbeddingHelper
    {
        public static List<string> SplitIntoChunks(string text, int maxTokens = 500)
        {
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
            var chunks = new List<string>();
            var current = new StringBuilder();

            foreach (var sentence in sentences)
            {
                if (current.Length + sentence.Length > maxTokens * 4) // approx 4 chars per token
                {
                    chunks.Add(current.ToString());
                    current.Clear();
                }
                current.Append(sentence + " ");
            }

            if (current.Length > 0)
                chunks.Add(current.ToString());

            return chunks;
        }

    }
}
