namespace Skynet.ImageCreator.Services;

using HtmlAgilityPack;
using Interfaces;

record struct CachedResult(List<HtmlNode> Nodes, DateTime DeleteAfter);

public class UnsplashImageService : IImageService
{
    readonly Random _random = new();
    readonly Dictionary<string, CachedResult> _cache = new();

    public string BaseUrl => "https://unsplash.com";

    async Task<List<HtmlNode>> Query(string query)
    {
        if(_cache.ContainsKey(query))
        {
            var result = _cache[query];

            // ensure we used up-to-date images from cache
            if (result.DeleteAfter >= DateTime.Now)
                return result.Nodes;

            _cache.Remove(query);
        }

        string searchUrl = string.Concat(BaseUrl, "/s/photos/", query);
        HtmlWeb web = new();
        HtmlDocument doc = await web.LoadFromWebAsync(searchUrl);

        // grab al the links with the photo url
        var images = doc.DocumentNode.SelectNodes("//a[@itemprop='contentUrl']")
            .Where(x => x.Attributes["href"].Value.StartsWith("/photos"))
            .ToList();

        if(_cache.ContainsKey(query))
            _cache[query].Nodes.AddRange(images);
        else
            _cache.Add(query, new()
            {
                Nodes = images,
                DeleteAfter = DateTime.Now.AddDays(1)
            });

        return images;
    }

    public async Task<string> RandomImageUrl(string query)
    {
        var images = await Query(query);

        int index = _random.Next(0, images.Count);
        string photoUrl = $"{BaseUrl}{images[index].Attributes["href"].Value}";

        HtmlWeb web = new();
        HtmlDocument photoPage = await web.LoadFromWebAsync(photoUrl);

        var items = photoPage.DocumentNode.SelectNodes("//img").ToList();

        if (items.Count > 1)
            return items[2].Attributes["src"].Value;

        return null;
    }
}
