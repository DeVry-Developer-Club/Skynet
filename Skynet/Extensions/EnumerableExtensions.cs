namespace Skynet.Extensions;
public static class EnumerableExtensions
{
    static Random random = new();

    /// <summary>
    /// Get a random item from collection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns>Random item from collection</returns>
    public static T RandomItem<T>(this IEnumerable<T> collection) =>
        collection.ElementAt(random.Next(0, collection.Count()));
}
