namespace Skynet.Options;
public class DiscordOptions
{
    public ulong MainGuildId { get; set; }
    public ulong WelcomeChannelId { get; set; }
    public ulong HelpChannelId { get; set; }
    public ulong GeneralChannelId { get; set; }

    /// <summary>
    /// Customizable invitation message.... a call to arms if you will
    /// </summary>
    public string InviteMessage { get; set; } =
        "Spread the word, our trusted scout! Spread the word of our kingdom! Amass an army of knowledge seeking minions! " +
        "Lay waste to the legions of doubt and uncertainty!!";

    /// <summary>
    /// Invitation link to use within the application
    /// </summary>
    public string InviteLink { get; set; } = "https://discord.gg/7geGwSET5B";

    public string InviteImage { get; set; } = "https://unofficialdevrycom.dev/images/Stein-Hogwarts-1200.jpg";

    /// <summary>
    /// Footer text for invitation messages
    /// </summary>
    public string InviteFooter { get; set; } = "Minons of Knowledge! Assembblleeee!";

    public string[] BlacklistedRoleNames { get; set; } = new[]
    {
        "@everyone",
        "Admin",
        "Senior Moderators",
        "Junior Moderators",
        "Pollmaster",
        "Professor",
        "Database",
        "Programmer",
        "Motivator",
        "Server Booster",
        "DeVry-SortingHat",
        "Devry-Service-Bot",
        "Devry-Challenge-Bot",
        "Devry-Test-Bot",
        "MathBot",
        "See-All-Channels",
        "Devry",
        "tutor"
    };
}
