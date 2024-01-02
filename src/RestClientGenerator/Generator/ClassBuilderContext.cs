namespace RestClient.Generator;

using Microsoft.CodeAnalysis;

internal class ClassBuilderContext
{
    public SemanticModel Model { get; set; }

    public INamedTypeSymbol Symbol { get; set; }


    public string Namespace {  get; set; }

    public string ClassName { get; set; }

    public string TypeName { get; set; }

    public string Route { get; set; }

    public string ContentType { get; set; }

    public FluentClassBuilder ClassBuilder { get; set; }

    public ISymbol CurrentMember {  get; set; }

    public IMethodSymbol CurrentMethodMember { get; set; }

}
