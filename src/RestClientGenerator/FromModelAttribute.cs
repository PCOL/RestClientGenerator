namespace RestClient;

using System;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// A return value attribute that allows the return value to be extracted from
/// a model property.
/// </summary>
[AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
public class FromModelAttribute
    : FromResponseAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FromModelAttribute"/> class.
    /// </summary>
    /// <param name="modelType">The model type.</param>
    /// <param name="propertyName">The property name.</param>
    public FromModelAttribute(Type modelType, string propertyName)
    {
        this.ModelType = modelType;
        this.PropertyName = propertyName;
    }

    /// <summary>
    /// Gets the model type.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// Gets the property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets or sets the return type property.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public new Type ReturnType { get; set; }

    /// <inheritdoc/>
    public override async Task<object> ToObjectAsync(
        HttpResponseMessage response,
        Type dataType,
        IObjectSerializer serializer)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (content.IsNullOrEmpty() == true)
        {
            return null;
        }

        object model = serializer.DeserializeObject(content, this.ModelType);
        if (model == null)
        {
            return null;
        }

        var property = this.ModelType.GetProperty(this.PropertyName);
        if (property == null)
        {
            throw new InvalidOperationException($"Property not found on model {this.ModelType.Name}");
        }

        if (dataType.IsAssignableFrom(property.PropertyType) == false)
        {
            throw new InvalidCastException($"Cannot cast {property.PropertyType} to {dataType.Name}");
        }

        return property.GetValue(model);
    }
}