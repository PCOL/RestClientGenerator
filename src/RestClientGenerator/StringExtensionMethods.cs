namespace RestClient;

/// <summary>
/// <see cref="string"/> extension methods.
/// </summary>
public static class StringExtensionMethods
{
    /// <summary>
    /// Ensure the string ends with the specified suffix.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="suffix">The suffix.</param>
    /// <returns>The source with the suffix.</returns>
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

    /// <summary>
    /// Removes the leading I from a type name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The type name without the leading I.</returns>
    internal static string RemoveLeadingI(this string typeName)
    {
        return typeName.StartsWith("I") ? typeName.Substring(1) : typeName;
    }
}
