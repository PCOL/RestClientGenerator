namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

/// <summary>
/// Attribute syntax receiver.
/// </summary>
public class AttributeSyntaxReceiver<TAttribute1, TAttribute2, TAttribute3>
    : ISyntaxReceiver
    where TAttribute1 : Attribute
    where TAttribute2 : Attribute
    where TAttribute3 : Attribute
{
    /// <summary>
    /// Gets a list of interfaces.
    /// </summary>
    public IList<InterfaceDeclarationSyntax> Interfaces { get; } = new List<InterfaceDeclarationSyntax>();

    /// <summary>
    /// Gets a list of Classes.
    /// </summary>
    public IList<ClassDeclarationSyntax> Classes { get; } = new List<ClassDeclarationSyntax>();

    /// <inheritdoc/>
    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclarationSyntax &&
            interfaceDeclarationSyntax.AttributeLists.Count > 0 &&
            interfaceDeclarationSyntax.AttributeLists
                .Any(al => al.Attributes
                    .Any(a =>
                    {
                        var attr = a.Name
                        .ToString()
                        .EnsureEndsWith("Attribute");

                        return attr.Equals(typeof(TAttribute1).Name) ||
                            attr.Equals(typeof(TAttribute2).Name);
                    })))
        {
            this.Interfaces.Add(interfaceDeclarationSyntax);
        }
        else if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
            classDeclarationSyntax.AttributeLists.Count > 0 &&
            classDeclarationSyntax.AttributeLists
                .Any(al => al.Attributes
                    .Any(a => a.Name
                        .ToString()
                        .EnsureEndsWith("Attribute")
                        .Equals(typeof(TAttribute3).Name))))
        {
            this.Classes.Add(classDeclarationSyntax);
        }
    }
}
