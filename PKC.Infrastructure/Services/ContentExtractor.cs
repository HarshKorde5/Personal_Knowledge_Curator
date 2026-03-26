using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace PKC.Infrastructure.Services;

public class ContentExtractor
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContentExtractor> _logger;

    public ContentExtractor(HttpClient httpClient, ILogger<ContentExtractor> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> ExtractFromUrlAsync(string url)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            //  Important: mimic browser (many sites block default requests)
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64)");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch URL: {Url}", url);
                return string.Empty;
            }

            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove unwanted elements
            var removeNodes = doc.DocumentNode.SelectNodes("//script|//style|//nav|//footer|//header|//aside");
            if (removeNodes != null)
            {
                foreach (var node in removeNodes)
                    node.Remove();
            }

            var body = doc.DocumentNode.SelectSingleNode("//body");

            if (body == null)
                return string.Empty;

            var text = body.InnerText;

            // Decode HTML entities
            text = HtmlEntity.DeEntitize(text);

            // Clean whitespace
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting content from {Url}", url);
            return string.Empty;
        }
    }
}