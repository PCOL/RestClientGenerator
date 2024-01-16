namespace RestClient.Generator;

using Microsoft.CodeAnalysis;

/// <summary>
/// Represents the context for the class builder.
/// </summary>
internal class ClassBuilderContext
{
    /// <summary>
    /// Gets or sets the semantic model.
    /// </summary>
    public SemanticModel Model { get; set; }

    /// <summary>
    /// Gets or sets the symbol.
    /// </summary>
    public INamedTypeSymbol Symbol { get; set; }

    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    public string Namespace {  get; set; }

    /// <summary>
    /// Gets or sets the class name.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// Gets or sets the type name.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the route.
    /// </summary>
    public string Route { get; set; }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the class builder.
    /// </summary>
    public FluentClassBuilder ClassBuilder { get; set; }
}
