
using System.Text;
using System.Text.Json;

namespace AISmartReply.Services
{
    public class AiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> GenerateReplyAsync(
            string customerMessage, string tone, string replyLength)
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

            var provider = _configuration["AIProvider"] ?? "Ollama";

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                return await GenerateGeminiReplyAsync(prompt);
            }

            return await GenerateOllamaReplyAsync(prompt);
        }

        private async Task<string> GenerateOllamaReplyAsync(string prompt)
        {
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

            return document.RootElement
                .GetProperty("response").GetString()?.Trim().Trim('"') ?? "";
        }

        private async Task<string> GenerateGeminiReplyAsync(string prompt)
        {
            var apiKey = _configuration["GeminiApiKey"];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(
                    "Gemini API key is not configured.");
            }

            var model = _configuration["GeminiModel"] ?? "gemini-2.5-flash";

            var requestData = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestData);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent");

            request.Headers.Add("x-goog-api-key", apiKey);

            request.Content = new StringContent(
                json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(responseText);

            return document.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()?.Trim().Trim('"') ?? "";
        }
    }
}
