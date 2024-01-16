namespace RestClient.Generator;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Roslyn extension methods.
/// </summary>
internal static class RoslynExtensionMethods
{
    /// <summary>
    /// Gets the type and its base types.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A list of types.</returns>
    private static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    /// <summary>
    /// Gets all members of a type and its base types.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>A list of member symbols.</returns>
    public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type)
    {
        return type.GetBaseTypesAndThis().SelectMany(n => n.GetMembers());
    }

    /// <summary>
    /// Checks if a type has a specific base type.
    /// </summary>
    /// <param name="type">A type symbol.</param>
    /// <param name="baseTypeName">The base type name.</param>
    /// <returns>True if is has; otherwise false.</returns>
    public static bool HasBaseType(this ITypeSymbol type, string baseTypeName)
    {
        return type.GetBaseTypesAndThis().Any(n => n.Name == baseTypeName);
    }

    /// <summary>
    /// Checks if a type has a specific base type.
    /// </summary>
    /// <param name="type">A type symbol.</param>
    /// <param name="baseTypeNamespace">The base type namespace.</param>
    /// <param name="baseTypeName">The base type name.</param>
    /// <returns>True if is has; otherwise false.</returns>
    public static bool HasBaseType(this ITypeSymbol type, string baseTypeNamespace, string baseTypeName)
    {
        return type
            .GetBaseTypesAndThis()
            .Any(n => n.ContainingNamespace.ToDisplayString() == baseTypeNamespace &&
                n.Name == baseTypeName);
    }
}
