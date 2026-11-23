using System.Text;
using System.Text.Json;

namespace AISmartReply.Services
{
    public class AiService
    {
        private readonly HttpClient _httpClient;

        public AiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateReplyAsync(
            string customerMessage,string tone,string replyLength)
        {
            var prompt = $"""
    You are a customer support assistant for a small business.

    Customer message: {customerMessage}
    Tone: {tone}
    Reply length: {replyLength}

    Instructions:
    - Short means maximum 2 sentences.
    - Medium means maximum 4 sentences.
    - Detailed means maximum 6 sentences.
    - Use simple, natural language.
    - Never invent prices, discounts, offers or policies.
    - Never promise anything on behalf of the business.
    - Do not suggest pricing changes or discounts.
    - Ask a question only when necessary.
    - Return only the reply text.
    """;

            var requestData = new
            {
                model = "llama3.2:1b",
                prompt = prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestData);

            var content = new StringContent(
                json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "http://localhost:11434/api/generate", content);

            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseText);

            //return document.RootElement
            //    .GetProperty("response").GetString() ?? "";
            var reply = document.RootElement
                .GetProperty("response").GetString() ?? "";

            return reply.Trim().Trim('"');

        }
    }
}
