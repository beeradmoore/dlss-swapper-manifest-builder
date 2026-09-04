using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace DLSS_Swapper_Manifest_Builder.Helpers;


/// <summary>
/// Pretty jank class. Given an object with a public property we can fetch it by JsonPropertyName attribute and cast it to what we expect it to be.
/// </summary>
internal static class ObjectPropertyGrabber
{
    /// <summary>
    /// Returned public propety T from object instance that has JsonPropertyName attribute with jsonName.
    /// </summary>
    /// <typeparam name="T">Type of object associated with jsonName.</typeparam>
    /// <param name="instance">Object with public properties on it.</param>
    /// <param name="jsonName">JsonPropertyName to go find</param>
    /// <returns>Object T found at JsonPropertyName of jsonName</returns>
    /// <exception cref="Exception"></exception>
    internal static T GetPropertyByJsonName<T>(object instance, string jsonName)
    {
        var type = instance.GetType();

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (properties.Length == 0)
        {
            throw new Exception($"GetPropertyByJsonName - Could not find any public properties for {type}");
        }

        var property = properties.FirstOrDefault(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name == jsonName);

        if (property is null)
        {
            throw new Exception($"GetPropertyByJsonName - Could not match {jsonName} for JsonPropertyName.");
        }

        var value = property.GetValue(instance);

        if (value is null)
        {
            throw new Exception($"GetPropertyByJsonName - Could not get value for {jsonName}.");
        }

        if (value is T valueT)
        {
            return valueT;
        }
        else
        {
            throw new Exception($"GetPropertyByJsonName - Could not cast value to given type {typeof(T)} for {jsonName}.");
        }
    }

}
