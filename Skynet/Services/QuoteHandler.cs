using System;
using DisCatSharp.Hosting;
using Skynet.Core.Interfaces;
using Skynet.Extensions;
using Skynet.ImageCreator.Interfaces;
using Skynet.ImageCreator.Utilities;
using Skynet.Options;
using Skynet.QuoteService.Interfaces;

namespace Skynet.Services;

public class QuoteHandler
{
    private readonly ILogger<QuoteHandler> _logger;
    private readonly IQuoteService _quoteService;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IStorageService _storageService;
    private readonly IImageService _imageService;
    private readonly IDiscordHostedService _bot;
    private readonly ImageSearchOptions _searchOptions;
    private readonly DiscordOptions _discordOptions;
    private readonly Images _imageUtility = new();

    public QuoteHandler(ILogger<QuoteHandler> logger, 
        IQuoteService quoteService, 
        IImageService imageService,
        IHostApplicationLifetime lifetime,
        IDiscordHostedService bot,
        ImageSearchOptions searchOptions,
        DiscordOptions discordOptions,
        IStorageService storageService)
    {
        _logger = logger; 
        _quoteService = quoteService;
        _lifetime = lifetime;
        _storageService = storageService;
        _imageService = imageService;
        _bot = bot;
        _searchOptions = searchOptions;
        _discordOptions = discordOptions;

        Task.Run(ProcessAsync);
    }

    async Task ProcessAsync()
    {
        string path = _storageService.GetPathFromRoot("Quote", "last-sent.txt");
        DateOnly last = DateOnly.FromDateTime(DateTime.Now);

        if (File.Exists(path))
            last = DateOnly.Parse(File.ReadAllText(path));

        while(!_lifetime.ApplicationStarted.IsCancellationRequested)
        {
            await Task.Delay(1000);
            DateOnly now = DateOnly.FromDateTime(DateTime.Now);

            if (now < last)
                continue;

            // Update our cached date
            last = now;
            await File.WriteAllTextAsync(path, now.ToString());
            await SendQuote();
        }
    }

    internal async Task SendQuote()
    {
        var qotdTask = _quoteService.GetQuoteOfTheDay();
        var ondTask = _quoteService.GetOnThisDay();

        // Images to use for either message
        var qotdImageTask = _imageService.RandomImageUrl(_searchOptions.StartOfDayKeywords.RandomItem());
        var ondImageTask = _imageService.RandomImageUrl(_searchOptions.StartOfDayKeywords.RandomItem());

        Task.WaitAll(qotdTask, ondTask, qotdTask, ondImageTask);

        DiscordMessageBuilder quoteOfTheDayMessage = new DiscordMessageBuilder();
        DiscordEmbedBuilder onThisDayEmbedBuilder = new DiscordEmbedBuilder()
            .WithFooter("On This Day")
            .WithTimestamp(DateTime.Today);
        
        List<string> words = new();

        foreach(var word in ondTask.Result.Words)
        {
            if (!string.IsNullOrEmpty(word.Link))
                words.Add(Formatter.MaskedUrl(word.Text, new Uri(word.Link)));
            else
                words.Add(word.Text);
        }

        onThisDayEmbedBuilder.Description = string.Join(" ", words).Replace("&#8211;","-");
        onThisDayEmbedBuilder.Title = "On This Day";

        if (!ondImageTask.Result.ToLower().Contains("adserver"))
            onThisDayEmbedBuilder.ImageUrl = ondImageTask.Result;
        
        var guild = await _bot.Client.GetGuildAsync(_discordOptions.MainGuildId);
        var channel = guild.GetChannel(_discordOptions.GeneralChannelId);

        string imagePath = await _imageUtility.CreateImageAsync(
            new TextPosition(0, -120, qotdTask.Result.Author),
            new TextPosition(0, 120, qotdTask.Result.Quote, FontSize: 12));

        using var file = File.Open(imagePath, FileMode.Open);

        Task messageTaskOne = channel.SendMessageAsync(onThisDayEmbedBuilder.Build());
        Task messageTaskTwo = channel.SendMessageAsync(quoteOfTheDayMessage.WithFile(file));

        Task.WaitAll(messageTaskOne, messageTaskTwo);
        
        // Continue trying to remove the image until it is finally freed up
        _ = Task.Run(async () =>
       {
           while (true)
           {
               try
               {
                   await Task.Delay(1000);
                   File.Delete(imagePath);
                   break;
               }
               catch
               {

               }
           }
       });
    }
}