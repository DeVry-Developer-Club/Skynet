using Newtonsoft.Json;

namespace Skynet.QuoteService.Interfaces;

public struct QuoteRecord
{
    [JsonProperty("a")]
    public string Author { get; set; }

    [JsonProperty("q")]
    public string Quote { get; set; }

    [JsonProperty("h")]
    public string Html { get; set; }

    public DateOnly? DatePulled { get; set; }
}

public record struct OnThisDayWord(string? Link, string Text);
public record struct OnThisDayRecord(OnThisDayWord[] Words);
public record struct OnThisDayCache(DateOnly CachedOn, OnThisDayRecord[] Records);

public interface IQuoteService
{
    /// <summary>
    /// Get the quote of the day
    /// </summary>
    /// <returns>Quote and author</returns>
    Task<QuoteRecord> GetQuoteOfTheDay();

    /// <summary>
    /// Get a random quote
    /// </summary>
    /// <returns>Quote and author</returns>
    Task<QuoteRecord?> GetRandomQuote();

    /// <summary>
    /// Retrieve a random fact about today
    /// </summary>
    /// <returns>Quote containing links/text that you can format as desired</returns>
    Task<OnThisDayRecord> GetOnThisDay();
}
