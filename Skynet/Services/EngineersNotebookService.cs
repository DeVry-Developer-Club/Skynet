using System.Net;
using EngineerNotebook.Shared.Endpoints.Guide;
using EngineerNotebook.Shared.Endpoints.Tag;
using EngineerNotebook.Shared.Models;
using Newtonsoft.Json;

namespace Skynet.Services;
using System.Text.RegularExpressions;
using System.Linq;

public class EngineersNotebookService
{
    private readonly ILogger<EngineersNotebookService> _logger;
    private readonly string _engineersNotebookAddress;
    private readonly HttpClient _client;
    public List<TagDto> AvailableTags { get; private set; } = new();
    private Regex _regex;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public EngineersNotebookService(IConfiguration configuration, ILogger<EngineersNotebookService> logger)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        _logger = logger;
        _engineersNotebookAddress = configuration.GetValue<string>("EngineersNotebook");
        
        _client = new HttpClient
        {
            BaseAddress = new Uri(_engineersNotebookAddress)
        };
        
        Task.Run(async () => await InitializeTags());
    }

    public async Task InitializeTags()
    {
        AvailableTags.Clear(); // ensure it's empty just in case

        string tagsResponse = await _client.GetStringAsync("tags");
        AvailableTags = JsonConvert.DeserializeObject<TagDto[]>(tagsResponse)?.ToList() ?? new();
        
        _logger.LogInformation($"Initialized EngineersNotebook Tags. Total: {AvailableTags.Count}");

        const string format = @"{0}";
        string all = string.Join("|", AvailableTags.Where(x => x.TagType != TagType.Phrase).Select(x => x.Name))
            .Replace("#",@"\043");
        _regex = new($@"({all})\s({all})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
    }    

    /// <summary>
    /// Attempt to locate PDF Guide(s) that fit the criteria
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task<(bool success, byte[]? pdfGuide)> FindGuide(string message)
    {
        var tags = await GetTagsFromDiscordMessage(message);

        if (!tags.Any())
            return (false, null);

        GetByTagsRequest request = new GetByTagsRequest();
        request.TagIds.AddRange(tags.Select(x => x.Id));
        var response = await _client.PostAsJsonAsync("guide", request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return (false, null);
        
        var content = await response.Content.ReadAsByteArrayAsync();
        
        return (true, content);
    }

    /// <summary>
    /// Retrieve the tags that were discovered within <paramref name="message"/>
    /// </summary>
    /// <param name="message">Message to analyze for tags</param>
    /// <returns>List of tags found within the message</returns>
    public Task<List<TagDto>> GetTagsFromDiscordMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return Task.FromResult(new List<TagDto>());

        var matches = _regex.Matches(message);
        var tags = matches
            .SelectMany(x => x.Groups
                                    .Values
                                        .SelectMany(y => y.Value
                                        .Split(" ")))
            .Distinct();
        return Task.FromResult(AvailableTags.Where(x => tags.Contains(x.Name.ToLower())).ToList());
    }
}