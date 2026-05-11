using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class OpenAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OpenAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async IAsyncEnumerable<string> StreamText(List<OpenAiMessage> messages)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        var request = new
        {
            model = "gpt-4.1-mini",
            stream = true,
            messages = messages.Select(m => new
            {
                role = m.Role,
                content = m.Content
            })
        };

        var json = JsonSerializer.Serialize(request);

        using var requestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.openai.com/v1/chat/completions");

        requestMessage.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            requestMessage,
            HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();

        using var reader = new StreamReader(stream);

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (!line.StartsWith("data: "))
                continue;

            var jsonData = line["data: ".Length..];

            if (jsonData == "[DONE]")
                yield break;

            string? text = null;

            try
            {
                using var doc = JsonDocument.Parse(jsonData);

                var delta = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("delta");

                if (delta.TryGetProperty("content", out var contentElement))
                {
                    text = contentElement.GetString();
                }
            }
            catch
            {
                text = null;
            }

            if (!string.IsNullOrEmpty(text))
            {
                yield return text;
            }
        }
    }
}