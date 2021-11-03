namespace Skynet.Services;
using DisCatSharp.Hosting;
using DisCatSharp.Entities;
using Skynet.Options;
using Interfaces;

public class RoleService : IRoleService
{
    private readonly DiscordOptions _options;
    private readonly IDiscordHostedService _bot;
    private readonly ILogger<RoleService> _logger;
    private Dictionary<ulong, List<DiscordRole>> _blacklistedRolesByGuild = new();

    public RoleService(DiscordOptions options, IDiscordHostedService bot, ILogger<RoleService> logger)
    {
        _options = options;
        _logger = logger;
        _bot = bot;
    }

    void InitializeBlacklistedRolesByName(ulong? guildId = null)
    {
        if(guildId.HasValue)
        {
            DiscordGuild guild = _bot.Client.Guilds[guildId.Value];

            if (!_blacklistedRolesByGuild.ContainsKey(guild.Id))
                _blacklistedRolesByGuild.Add(guild.Id, new());
            else
                _blacklistedRolesByGuild[guild.Id].Clear();

            InitializeGuildRoles(guild);        
            return;
        }

        _blacklistedRolesByGuild.Clear();
        
        foreach(DiscordGuild guild in _bot.Client.Guilds.Values)
        {
            _blacklistedRolesByGuild.Add(guild.Id, new());
            InitializeGuildRoles(guild);
        }

    }

    void InitializeGuildRoles(DiscordGuild guild)
    {
        foreach (string roleName in _options.BlacklistedRoleNames)
        {
            var role = guild.Roles.FirstOrDefault(x =>
            x.Value.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));

            if (role.Key > 0)
                _blacklistedRolesByGuild[guild.Id].Add(role.Value);
            else
                _logger.LogWarning($"Could not locate {roleName} within guild {guild.Name}");
        }
    }

    public IReadOnlyDictionary<ulong, DiscordRole> GetBlacklistedRolesDict(ulong guildId)
    {
        Dictionary<ulong, DiscordRole> results = new();

        if (!_bot.Client.Guilds.ContainsKey(guildId))
            return results;

        if (_blacklistedRolesByGuild.ContainsKey(guildId))
            return _blacklistedRolesByGuild[guildId]
                .ToDictionary(x => x.Id, x => x);

        InitializeBlacklistedRolesByName(guildId);

        if(_blacklistedRolesByGuild.ContainsKey(guildId))
            return _blacklistedRolesByGuild[guildId]
                .ToDictionary(x=>x.Id, x => x);

        return results;
    }
}
