namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Attribute syntax receiver.
/// </summary>
public class AttributeSyntaxReceiver<TAttribute>
    : ISyntaxReceiver
    where TAttribute : Attribute
{
    /// <summary>
    /// Gets a list of interfaces.
    /// </summary>
    public IList<InterfaceDeclarationSyntax> Interfaces { get; } = new List<InterfaceDeclarationSyntax>();

    /// <inheritdoc/>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
                interfaceDeclarationSyntax.AttributeLists.Count > 0 &&
                interfaceDeclarationSyntax.AttributeLists
                    .Any(al => al.Attributes
                        .Any(a => a.Name
                            .ToString()
                            .EnsureEndsWith("Attribute")
                            .Equals(typeof(TAttribute).Name))))
        {
            this.Interfaces.Add(interfaceDeclarationSyntax);
        }
    }
}
