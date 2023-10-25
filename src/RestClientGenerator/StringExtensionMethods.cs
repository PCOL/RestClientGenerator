namespace RestClient;

/// <summary>
/// <see cref="string"/> extension methods.
/// </summary>
public static class StringExtensionMethods
{
    /// <summary>
    /// E
    /// </summary>
    /// <param name="source"></param>
    /// <param name="suffix"></param>
    /// <returns></returns>
    public static string EnsureEndsWith(
            this string source,
            string suffix)
    {
        if (source.EndsWith(suffix))
        {
            return source;
        }

        return source + suffix;
    }
}
