using DisCatSharp.Entities;
using DisCatSharp.EventArgs;
namespace Skynet.Discord;
public interface IInteractionHandler
{
    Task<bool> Handle(DiscordMember member, ComponentInteractionCreateEventArgs args);
}
