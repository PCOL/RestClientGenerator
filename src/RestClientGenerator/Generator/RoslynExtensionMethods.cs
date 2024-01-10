namespace RestClient.Generator;

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

public static class RoslynExtensionMethods
{
    private static IEnumerable<ITypeSymbol> GetBaseTypesAndThis(this ITypeSymbol type)
    {
        var current = type;
        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    public static IEnumerable<ISymbol> GetAllMembers(this ITypeSymbol type)
    {
        return type.GetBaseTypesAndThis().SelectMany(n => n.GetMembers());
    }

    public static bool HasBaseType(this ITypeSymbol type, string baseTypeName)
    {
        return type.GetBaseTypesAndThis().Any(n => n.Name == baseTypeName);
    }

    public static bool HasBaseType(this ITypeSymbol type, string baseTypeNamespace, string baseTypeName)
    {
        return type.GetBaseTypesAndThis().Any(n => n.ContainingNamespace.ToDisplayString() == baseTypeNamespace && n.Name == baseTypeName);
    }
}
