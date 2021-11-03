namespace Skynet.Discord.Extensions;
using DisCatSharp.Entities;

public static class RoleDictionaryExtensions
{
    public static List<DiscordRole> RemoveBlacklistedRoles(this IEnumerable<DiscordRole> roles,
        IEnumerable<ulong> blacklistedRoles)
    {
        var list = blacklistedRoles.ToList();

        return roles
                .Where(x => x is not null && 
                            !list.Contains(x.Id) && 
                            !x.Name.StartsWith("^") && 
                            !x.Name.ToLower().Contains("moderator"))
                .ToList();
    }

}
