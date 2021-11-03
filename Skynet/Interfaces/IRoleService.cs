namespace Skynet.Interfaces;
public interface IRoleService
{
    IReadOnlyDictionary<ulong, DisCatSharp.Entities.DiscordRole> GetBlacklistedRolesDict(ulong guildId);
}
