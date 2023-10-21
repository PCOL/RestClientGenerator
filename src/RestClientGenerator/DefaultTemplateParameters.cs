namespace RestClientGenerator;

using System;

public class DefaultTemplateParameters
{
    public DefaultTemplateParameters(
        string typeName,
        string typeNamespace,
        string rootNamespace)
    {
        ClassName = typeName
          ?? throw new ArgumentNullException(nameof(typeName));
        Namespace = typeNamespace
          ?? throw new ArgumentNullException(nameof(typeNamespace));
        PrefferredNamespace = rootNamespace
          ?? throw new ArgumentNullException(nameof(rootNamespace));
    }

    /// <summary>
    /// Gets or sets the class name.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// Gets or sets the interface name.
    /// </summary>
    public string InterfaceName { get; set; }

    /// <summary>
    /// Gets or sets the interface implementation.
    /// </summary>
    public string InterfaceImpl { get; set; }

    /// <summary>
    /// Gets or sets the namespace.
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Gets or sets the prefferred namespace.
    /// </summary>
    public string PrefferredNamespace { get; set; }
}