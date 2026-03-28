using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

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

    // -------------------------------------------------------------------------
    // URL EXTRACTION
    // -------------------------------------------------------------------------
    public async Task<string> ExtractFromUrlAsync(string url)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Mimic a real browser — many sites block default HttpClient user agents.
            request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (X11; Linux x86_64)");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch URL {Url}: HTTP {StatusCode}", url, response.StatusCode);
                return string.Empty;
            }

            var html = await response.Content.ReadAsStringAsync();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Strip non-content elements
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

            text = HtmlEntity.DeEntitize(text);
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting content from URL: {Url}", url);
            return string.Empty;
        }
    }

    // -------------------------------------------------------------------------
    // PDF EXTRACTION
    // -------------------------------------------------------------------------
    public string ExtractFromPdf(string filePath)
    {
        try
        {
            _logger.LogInformation("Extracting text from PDF: {FilePath}", filePath);

            using var document = PdfDocument.Open(filePath);

            var sb = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                var words = page.GetWords();
                var pageText = string.Join(" ", words.Select(w => w.Text));

                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    sb.Append(pageText);
                    sb.Append(' ');
                }
            }

            var extracted = sb.ToString().Trim();

            extracted = Regex.Replace(extracted, @"\s+", " ");

            _logger.LogInformation(
                "PDF extraction complete: {CharCount} characters from {PageCount} pages",
                extracted.Length,
                document.NumberOfPages);

            return extracted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting content from PDF: {FilePath}", filePath);
            return string.Empty;
        }
    }
}