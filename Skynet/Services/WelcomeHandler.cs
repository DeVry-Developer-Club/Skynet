using DisCatSharp.Entities;
using DisCatSharp.Enums;
using DisCatSharp.Hosting;
using Skynet.Extensions;
using Skynet.Interfaces;
using Skynet.Options;

namespace Skynet.Services;
public class WelcomeHandler : IWelcomeHandler
{
    private readonly ILogger<WelcomeHandler> _logger;
    private readonly WelcomeOptions _options;

    /// <summary>
    /// Roles that we're expecting people to join for a given duration
    /// Perhaps a class lecture is happening right now
    /// 
    /// Key: Class/role to add user to
    /// Value: end time for class
    /// </summary>
    public Dictionary<DiscordRole, DateTime> ClassExpectations = new();

    /// <summary>
    /// List of members that shall be welcomed in the current message queue
    /// </summary>
    private List<DiscordMember> welcomeQueue = new();

    /// <summary>
    /// When the current time exceeds trigger time -- the message will be triggered and the queue
    /// will be reset
    /// </summary>
    private DateTime triggerTime = DateTime.Now;

    private int messageInterval;
    private readonly CancellationToken _cancellationToken;
    private readonly DiscordChannel _welcomeChannel;
    
    public WelcomeHandler(ILogger<WelcomeHandler> logger, DiscordOptions discordOptions, 
        WelcomeOptions options)
    {
        messageInterval = options?.WelcomeMessageInterval ?? 30;
        _cancellationToken = default;
        _logger = logger;
        _options = options;
        //_welcomeChannel = bot.Client.Guilds[discordOptions.MainGuildId].Channels[discordOptions.WelcomeChannelId];
        //Task.Run(ProcessQueue);
    }

    public void AddClass(DiscordRole role, DateTime expirationTime)
    {
        if (ClassExpectations.ContainsKey(role))
            ClassExpectations[role] = expirationTime;
        else
            ClassExpectations.Add(role, expirationTime);

        _logger.LogInformation($"Expecting folks to be joining {role.Name} | {expirationTime.ToString("F")} | Appending class count {ClassExpectations.Count}");
    }

    public void AddMember(DiscordMember member)
    {
        welcomeQueue.Add(member);

        // based on the number of users we shall determine how much time gets added to our message interval
        // so the more users we have the less time is added thus -- the message will occur sooner

        int interval = (int)(messageInterval - (0.1 * welcomeQueue.Count * messageInterval));
        triggerTime = DateTime.Now.AddSeconds(interval);

        _logger.LogInformation($"Welcome train has :{welcomeQueue.Count} in queue for greeting. " +
            $"Triggeringat: {triggerTime.ToString("F")} | Now: {DateTime.Now.ToString("F")}");
    }

    public async Task AddRoleToMember(DiscordMember member, string interactionId)
    {
        if (!interactionId.EndsWith("_role"))
            return;

        ulong roleId = ulong.Parse(interactionId.Split('_').First());

        if (!_welcomeChannel.Guild.Roles.ContainsKey(roleId))
            return;

        await member.GrantRoleAsync(_welcomeChannel.Guild.Roles[roleId]);
    }

    async Task ProcessQueue()
    {
        while(!_cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000);
            DateTime referenceTime = DateTime.Now;

            #region remove expired class expectations
            // need to check and remove anything that is past its expiration date
            var remove = ClassExpectations
                .Where(x => referenceTime > x.Value)
                .Select(x => x.Key);

            if(remove.Any())
            {
                _logger.LogInformation($"The following classes exceeded their welcome-expiration-time:\n\t" +
                    $"{string.Join("\n\t", remove.Select(x => x.Name))}");

                foreach (var role in remove)
                    ClassExpectations.Remove(role);
            }
            #endregion

            // we will only send messages if the queue is > 0 and if we have exceeded the trigger time
            if (!welcomeQueue.Any())
                continue;

            if (DateTime.Now < triggerTime)
                continue;

            try
            {
                string message = _options.WelcomeMessage.ToWelcomeMessage(welcomeQueue, _welcomeChannel);

                _logger.LogInformation($"Welcoming {string.Join(", ", welcomeQueue.Select(x => x.DisplayName))}, " +
                    $"following roles are appended to welcome message: \n\t" +
                    $"{string.Join("\n\t", ClassExpectations.Select(x => x.Key.Name))}");

                // Reset queue for later
                welcomeQueue.Clear();

                DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                    .WithTitle("Welcome to DeVry Student Community")
                    .WithDescription(message);

                DiscordMessageBuilder messageBuilder = new();

                // add some instruction to users to click the associated buttons -- if any
                var buttons = GenerateClassButtons();

                if(buttons.Any())
                {
                    embedBuilder.Description +=
                        "\n\tIf you're in one of the following majors or classes, please click the associated button(s)";

                    while(buttons.Any())
                    {
                        var subsection = buttons.Take(5);
                        messageBuilder.AddComponents(subsection);
                        buttons.RemoveRange(0, subsection.Count());
                    }
                }

                messageBuilder.AddEmbed(embedBuilder.Build());
                await _welcomeChannel.SendMessageAsync(messageBuilder);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in WelcomeHandler while greeting users");
            }
        }
    }

    List<DiscordButtonComponent> GenerateClassButtons()
    {
        List<DiscordButtonComponent> buttons = new();

        int option = 0;
        foreach(var pair in ClassExpectations.OrderBy(x=>x.Key.Name)
            .Take(24))
        {
            DiscordButtonComponent component = new DiscordButtonComponent((ButtonStyle)(option + 1),
                $"{pair.Key.Id}_role",
                pair.Key.Name);

            buttons.Add(component);

            // Just trying to vary the colors that appear
            option++;
            option %= 4;
        }

        return buttons;
    }
}
