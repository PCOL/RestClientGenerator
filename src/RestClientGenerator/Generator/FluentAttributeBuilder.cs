namespace RestClient.Generator;

/// <summary>
/// Represents a fluent attribute builder.
/// </summary>
internal class FluentAttributeBuilder
{
    /// <summary>
    /// The attribute name.
    /// </summary>
    private string attributeTypeName;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentAttributeBuilder"/> class.
    /// </summary>
    public FluentAttributeBuilder()
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentAttributeBuilder"/> class.
    /// </summary>
    /// <param name="attributeTypeName">the attirbute name.</param>
    public FluentAttributeBuilder(string attributeTypeName)
    {
        this.attributeTypeName = attributeTypeName;
    }

    /// <summary>
    /// Specifies the type name.
    /// </summary>
    /// <param name="typeName">The name of the type.</param>
    /// <returns>The <see cref="FluentAttributeBuilder"/>.</returns>
    public FluentAttributeBuilder TypeName(string typeName)
    {
        this.attributeTypeName = typeName;
        return this;
    }

    /// <summary>
    /// Builds the attribute.
    /// </summary>
    /// <returns>The attribute string.</returns>
    public string Build()
    {
        return $"[{this.attributeTypeName}()]";
    }
}