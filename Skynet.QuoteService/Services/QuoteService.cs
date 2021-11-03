using HtmlAgilityPack;
using Skynet.Core.Interfaces;
using Skynet.QuoteService.Interfaces;

namespace Skynet.QuoteService.Services;
public class QuoteService : IQuoteService
{
    private readonly string _quoteOfTheDayUrl = @"https://zenquotes.io/api/today";
    private readonly string _randomQuoteUrl = @"https://zenquotes.io/api/random";
    private readonly string _onThisDayUrl = @"https://apizen.date/";

    private readonly IStorageService _storageService;

    private readonly string _quoteOfTheDayCache = "quote-of-the-day-cache.json";
    private readonly string _onThisDayCache = "on-this-day-cache";
    private readonly Random _random;

    HttpClient _httpClient;

    public QuoteService(IStorageService storageService)
    {
        _httpClient = new();
        _storageService = storageService;
        _random = new();
    }

    OnThisDayWord ExtractWord(HtmlNode? node)
    {
        switch(node.Name)
        {
            case "a":
                return new(node.Attributes["href"].Value, node.Attributes["title"].Value);
            case "i":
                return ExtractWord(node.FirstChild);
            case "#text":
            default:
                return new(null, node.InnerText);
        }
    }

    public async Task<OnThisDayRecord> GetOnThisDay()
    {
        string path = _storageService.GetPathFromRoot("Quotes", _onThisDayCache);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if(File.Exists(path))
        {
            var cachedItems = Newtonsoft.Json.JsonConvert.DeserializeObject<OnThisDayCache>(await File.ReadAllTextAsync(path));

            if(cachedItems.CachedOn == today)
                return cachedItems.Records[_random.Next(0, cachedItems.Records.Length)];
        }

        HtmlWeb web = new();
        HtmlDocument doc = await web.LoadFromWebAsync(_onThisDayUrl);

        var blobs = doc.DocumentNode.SelectNodes("//span")
            .Skip(1)
            .ToList();

        List<OnThisDayRecord> records = new();

        foreach(var section in blobs)
        {
            List<OnThisDayWord> words = new();

            foreach (var node in section.ChildNodes)
                words.Add(ExtractWord(node));

            records.Add(new(words.ToArray()));
        }

        OnThisDayCache cache = new(today, records.ToArray());
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(cache);

        FileInfo fileInfo = new FileInfo(path);
        Directory.CreateDirectory(fileInfo.Directory.FullName);

        await File.WriteAllTextAsync(path, json);
        
        return cache.Records[_random.Next(0, cache.Records.Length)];
    }

    public async Task<QuoteRecord> GetQuoteOfTheDay()
    {
        var path = _storageService.GetPathFromRoot("Quotes", _quoteOfTheDayCache);

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);


        // If this file doesn't exist we know we haven't polled
        if(File.Exists(path))
        {
            var cachedItem = Newtonsoft.Json.JsonConvert.DeserializeObject<QuoteRecord>(await File.ReadAllTextAsync(path));
            
            if (cachedItem.DatePulled == today)
                return cachedItem;
        }

        string result = await _httpClient.GetStringAsync(_quoteOfTheDayUrl);

#pragma warning disable CS8604 // Possible null reference argument.
        var item = Newtonsoft.Json.JsonConvert.DeserializeObject<QuoteRecord[]>(result)
#pragma warning restore CS8604 // Possible null reference argument.
            .FirstOrDefault();

        item.DatePulled = today;

        FileInfo info = new FileInfo(path);
        Directory.CreateDirectory(info.Directory.FullName);
        // Ensure we update our cache
        await File.WriteAllTextAsync(path, Newtonsoft.Json.JsonConvert.SerializeObject(item));

        return item;
    }

    public async Task<QuoteRecord?> GetRandomQuote()
    {
        string result = await _httpClient.GetStringAsync(_randomQuoteUrl);

        return Newtonsoft.Json.JsonConvert.DeserializeObject<QuoteRecord[]>(result)?
            .FirstOrDefault();
    }
}
