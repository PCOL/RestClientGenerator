namespace RestClient;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

internal static class UtilityExtensionMethods
{
    /// <summary>
    /// Throws an exception if the passed argument is null.
    /// </summary>
    /// <param name="argument">The arguement.</param>
    /// <param name="argumentName">The argument name.</param>
    public static void ThrowIfArgumentNull(this object argument, string argumentName)
    {
        if (argument == null)
        {
            throw new ArgumentNullException(argumentName);
        }
    }

    /// <summary>
    /// Throws an exception if the passed argument is null or empty.
    /// </summary>
    /// <param name="argument">The arguement.</param>
    /// <param name="argumentName">The argument name.</param>
    public static void ThrowIfArgumentNullOrEmpty(this string argument, string argumentName)
    {
        argument.ThrowIfArgumentNull(argumentName);

        if (argument == string.Empty)
        {
            throw new ArgumentException("Argument is empty", argumentName);
        }
    }

    /// <summary>
    /// Throws an exception if the passed argument is null, empty or contains whitespace.
    /// </summary>
    /// <param name="argument">The arguement.</param>
    /// <param name="argumentName">The argument name.</param>
    public static void ThrowIfArgumentNullEmptyOrWhitespace(this string argument, string argumentName)
    {
        argument.ThrowIfArgumentNullOrEmpty(argumentName);

        if (argument.IndexOf(' ') != -1)
        {
            throw new ArgumentException("Argument contains whitespace", argumentName);
        }
    }

    /// <summary>
    /// Throws an <see cref="InvalidOperationException"/> if the type is not an interface.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="name">The types name.</param>
    public static void ThrowIfNotInterface(this Type type, string name)
    {
        if (type.IsInterface == false)
        {
            throw new InvalidOperationException(string.Format("{0} must be an interface", name));
        }
    }

    /// <summary>
    /// Checks if a list is null or empty.
    /// </summary>
    /// <typeparam name="T">The list type.</typeparam>
    /// <param name="list">The list to check.</param>
    /// <returns>True if null or empty; otherwise false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
    {
        return list == null ||
            list.Any() == false;
    }

    /// <summary>
    /// Checks if the return type is a task.
    /// </summary>
    /// <param name="returnType">The return type.</param>
    /// <param name="taskType">A variable to receive the task type.</param>
    /// <returns>True if the return type is a <see cref="Task"/> and therefore asynchronous; otherwise false.</returns>
    public static bool IsAsync(this Type returnType, out Type taskType)
    {
        taskType = null;
        if (typeof(Task).IsAssignableFrom(returnType) == true)
        {
            taskType = returnType.GetGenericArguments().FirstOrDefault() ?? typeof(void);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Sets the value of an objects property.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="value">The value to set.</param>
    public static void SetObjectProperty<T>(
        this object obj,
        string propertyName,
        T value)
    {
        var property = obj?.GetType().GetProperty(propertyName, typeof(T));
        if (property != null && property.SetMethod != null && property.SetMethod.IsPublic)
        {
            property.SetValue(obj, value);
        }
    }

    /// <summary>
    /// Set the value of an objects property by the type of property.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="value">The value.</param>
    public static void SetObjectProperty<T>(
        this object obj,
        T value)
    {
        obj.SetObjectProperty(() => value);
    }

    /// <summary>
    /// Set the value of an objects property by the type of property.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="valueFunction">The function that provides the value.</param>
    public static void SetObjectProperty<T>(
        this object obj,
        Func<T> valueFunction)
    {
        obj?.GetType()
            .GetProperties().SetProperty(obj, valueFunction());
    }

    /// <summary>
    /// Set the value of a property by the type of property.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="properties">A list of properties.</param>
    /// <param name="obj">The object.</param>
    /// <param name="value">The value.</param>
    public static void SetProperty<T>(
        this IEnumerable<PropertyInfo> properties,
        object obj,
        T value)
    {
        properties.SetProperty(obj, () => value);
    }

    /// <summary>
    /// Set the value of a property by the type of property.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="properties">A list of properties.</param>
    /// <param name="obj">The object.</param>
    /// <param name="valueFunction">The function that provides the value.</param>
    public static void SetProperty<T>(
        this IEnumerable<PropertyInfo> properties,
        object obj,
        Func<T> valueFunction)
    {
        var property = properties?.FirstOrDefault(p => p.PropertyType == typeof(T));
        if (property != null)
        {
            property.SetValue(obj, valueFunction());
        }
    }

    /// <summary>
    /// Set the value of a property by the type of property.
    /// </summary>
    /// <param name="properties">A list of properties.</param>
    /// <param name="obj">The object.</param>
    /// <param name="propertyType">The properties type.</param>
    /// <param name="valueFunction">The function that provides the value.</param>
    public static void SetProperty(
        this IEnumerable<PropertyInfo> properties,
        object obj,
        Type propertyType,
        Func<object> valueFunction)
    {
        var property = properties?.FirstOrDefault(p => p.PropertyType == propertyType);
        if (property != null)
        {
            property.SetValue(obj, valueFunction());
        }
    }

    /// <summary>
    /// Checks if a type a subclass of a generic type.
    /// </summary>
    /// <param name="type">The generic type.</param>
    /// <param name="check">The type to check.</param>
    /// <returns>True if it is a subclass; otherwise false.</returns>
    public static bool IsSubclassOfGeneric(
        this Type type,
        Type check)
    {
        while (type != null && type != typeof(object))
        {
            var genType = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (check == genType)
            {
                return true;
            }

            type = type.BaseType;
        }

        return false;
    }

    public static bool HasAttribute(
        this INamedTypeSymbol symbol,
        string attributeName)
    {
        return symbol.GetAttributes().Any(a => a.AttributeClass.Name == attributeName);
    }

    /// <summary>
    /// Gets attribute named arguments from a symbol.
    /// </summary>
    /// <param name="symbol">A symbol.</param>
    /// <param name="attributeName">The attribute name.</param>
    /// <param name="propertySetters">A list of property setters.</param>
    public static void GetAttributeNamedArguments(
        this INamedTypeSymbol symbol,
        string attributeName,
        IEnumerable<(string, string, Action<object>)> propertySetters)
    {
        GetAttributeNamedArguments(symbol.GetAttributes(), attributeName, propertySetters);
    }

    /// <summary>
    /// Gets attribute named arguments from a list of attributes.
    /// </summary>
    /// <param name="attributes">A list of attributes.</param>
    /// <param name="attributeName">The attribute name.</param>
    /// <param name="propertySetters">A list of property setters.</param>
    public static void GetAttributeNamedArguments(
        this ImmutableArray<AttributeData> attributes,
        string attributeName,
        IEnumerable<(string, string, Action<object>)> propertySetters)
    {

        foreach (var attr in attributes)
        {
            if (attr.AttributeClass.Name == attributeName)
            {
                attr.NamedArguments.GetNamedArguments(propertySetters);
            }
        }
    }

    /// <summary>
    /// Gets named arguments from a list of named arguments.
    /// </summary>
    /// <param name="args">A list of named arguments.</param>
    /// <param name="propertySetters">A list of property setters.</param>
    public static void GetNamedArguments(
        this ImmutableArray<KeyValuePair<string, TypedConstant>> args,
        IEnumerable<(string, string, Action<object>)> propertySetters)
    {
        foreach (var arg in args)
        {
            foreach (var (name, typeName, setter) in propertySetters)
            {
                if (arg.GetValue<object>(name, typeName, out var value))
                {
                    setter(value);
                }
            }
        }
    }

    /// <summary>
    /// Gets an argument from a list of named arguments.
    /// </summary>
    /// <typeparam name="T">The argument value type.</typeparam>
    /// <param name="args">A list of named arguments.</param>
    /// <param name="name">The name of the argument.</param>
    /// <returns>The argument value.</returns>
    public static T GetNamedArgument<T>(
        this ImmutableArray<KeyValuePair<string, TypedConstant>> args,
        string name)
    {
        foreach (var arg in args)
        {
            if (arg.GetValue<T>(name, typeof(T).Name, out var value))
            {
                return value;
            }
        }

        return default;
    }

    /// <summary>
    /// Gets an argument value from a matching argument key/value pair.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="argument">The argument key/value pair.</param>
    /// <param name="argName">The argument name.</param>
    /// <param name="argTypeName">The argument type name.</param>
    /// <param name="value">A variable to receive the value.</param>
    /// <returns>A value indicating whether or not the value was returned.</returns>
    public static bool GetValue<T>(
        this KeyValuePair<string, TypedConstant> argument,
        string argName,
        string argTypeName,
        out T value)
    {
        value = default;
        if (argument.Key == argName && argument.Value.Type.Name == argTypeName)
        {
            value = (T)argument.Value.Value;
            return true;
        }

        return false;
    }

    internal static string GetInnerType(this ImmutableArray<SymbolDisplayPart> parts)
    {
        var capture = false;
        var typeName = string.Empty;
        for (int i = 0; i < parts.Length; i++)
        {
            if (capture == false &&
                parts[i].Kind == SymbolDisplayPartKind.Punctuation &&
                parts[i].ToString() == "<")
            {
                capture = true;
                continue;
            }
            else if (capture == true &&
                parts[i].Kind == SymbolDisplayPartKind.Punctuation &&
                parts[i].ToString() == ">" &&
                i == parts.Length - 1)
            {
                capture = false;
                continue;
            }

            if (capture == true)
            {
                typeName += parts[i].ToString();
            }
        }

        return typeName;
    }

    internal static IParameterSymbol GetParameterWithAttribute(
        this IMethodSymbol methodMember,
        string attributeName,
        out AttributeData attribute)
    {
        return methodMember.Parameters.GetParameterWithAttribute(attributeName, out attribute);
    }

    internal static IParameterSymbol GetParameterWithAttribute(
        this ImmutableArray<IParameterSymbol> parameters,
        string attributeName,
        out AttributeData attribute)
    {
        attribute = null;
        foreach (var param in parameters)
        {
            var attrs = param.GetAttributes();
            if (attrs.Any() == true)
            {
                foreach (var attr in attrs)
                {
                    if (attr.AttributeClass.Name == attributeName)
                    {
                        attribute = attr;
                        return param;
                    }
                }
            }
        }

        return null;
    }

    public static string BuildParametersList(
        this IMethodSymbol method,
        bool includeCancellationToken = false)
    {
        return method?.Parameters.BuildParametersList(includeCancellationToken);
    }

    public static string BuildParametersList(
        this ImmutableArray<IParameterSymbol> parameters,
        bool includeCancellationToken = false)
    {
        if (parameters != null)
        {
            var paramsList = parameters
                .Select(p => p.Name)
                .ToList();

            if (includeCancellationToken)
            {
                if (paramsList.Contains("cancellationToken") == false)
                {
                    paramsList.Add("cancellationToken");
                }
            }

            return string.Join(", ", paramsList);
        }

        return string.Empty;
    }
}
