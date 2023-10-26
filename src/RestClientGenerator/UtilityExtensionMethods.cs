namespace RestClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

public static class UtilityExtensionMethods
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
    public static void SetObjectProperty<T>(this object obj, string propertyName, T value)
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
    public static void SetObjectProperty<T>(this object obj, T value)
    {
        obj.SetObjectProperty(() => value);
    }

    /// <summary>
    /// Set the value of an objects property by the type of property.
    /// </summary>
    /// <typeparam name="T">The property type.</typeparam>
    /// <param name="obj">The object.</param>
    /// <param name="valueFunction">The function that provides the value.</param>
    public static void SetObjectProperty<T>(this object obj, Func<T> valueFunction)
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
    public static void SetProperty<T>(this IEnumerable<PropertyInfo> properties, object obj, T value)
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
    public static void SetProperty<T>(this IEnumerable<PropertyInfo> properties, object obj, Func<T> valueFunction)
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
    public static void SetProperty(this IEnumerable<PropertyInfo> properties, object obj, Type propertyType, Func<object> valueFunction)
    {
        var property = properties?.FirstOrDefault(p => p.PropertyType == propertyType);
        if (property != null)
        {
            property.SetValue(obj, valueFunction());
        }
    }
}
