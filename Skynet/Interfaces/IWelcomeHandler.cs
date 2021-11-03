using DisCatSharp.Entities;

namespace Skynet.Interfaces;
public interface IWelcomeHandler
{
    /// <summary>
    /// Based on interaction ID format -- add that associated role to <paramref name="member"/>
    /// </summary>
    /// <param name="member">Member who should be receiving role associated to <paramref name="interactionId"/></param>
    /// <param name="interactionId">ID of interaction</param>
    Task AddRoleToMember(DiscordMember member, string interactionId);

    /// <summary>
    /// Add a class that we're expecting users to be coming from (DeVry)
    /// Along with an expiration time for when the class button should no
    /// longer be appended to the welcome message
    /// </summary>
    /// <param name="role">Role to apply</param>
    /// <param name="expirationTime">Time to stop asking users</param>
    void AddClass(DiscordRole role, DateTime expirationTime);

    /// <summary>
    /// Adds member to greeting queue
    /// </summary>
    /// <param name="member"></param>
    void AddMember(DiscordMember member);
}
