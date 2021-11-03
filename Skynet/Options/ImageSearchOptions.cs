namespace Skynet.Options;

public class ImageSearchOptions
{
    /// <summary>
    /// Keywords for programming to search for using <see cref="ImageCreator.Interfaces.IImageService"/>
    /// </summary>
    public string[] ProgrammingKeywords { get; set; }

    /// <summary>
    /// Keywords for welcomg people to search for using <see cref="ImageCreator.Interfaces.IImageService"/>
    /// </summary>
    public string[] WelcomeKeywords { get; set; }

    /// <summary>
    /// Keywords for start-of-day to search for using <see cref="ImageCreator.Interfaces.IImageService"/>
    /// </summary>
    public string[] StartOfDayKeywords { get; set; }
}
