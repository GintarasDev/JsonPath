using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace QuickJson;

public class JsonProperty
{
    public required PropertyInfo Property;
    public required string[]? Path;
    public required string? Name;
}

public class PathToModify
{
    public required string OriginalPath;
    public required string NewPath;
}

public static class QuickJson
{
    static readonly List<PathToModify> EmptyList = new(); // Order of this list is very important

    public static string SerializeObject(object? objectToSerialize)
    {
        if (objectToSerialize is null)
            return JsonConvert.SerializeObject(objectToSerialize);

        var type = objectToSerialize?.GetType();
        var pathsToModify = GetPathsToModify(type);
        var jObject = JObject.Parse(JsonConvert.SerializeObject(objectToSerialize));

        var result = UpdateJsonPaths(jObject, pathsToModify);

        return result.ToString();
    }

    private static JObject UpdateJsonPaths(JObject jObject, List<PathToModify> pathsToModify)
    {
        foreach (var path in pathsToModify)
        {
            var originalPath = path.OriginalPath.Split('.');
            var newPath = path.NewPath.Split('.');

            MoveJsonData(jObject, originalPath, newPath);
        }

        return jObject;
    }

    public static T DeserializeObject<T>(string json) where T : new()
    {
        return JsonConvert.DeserializeObject<T>(json);
        //var type = typeof(T);
        //var properties =
        //// TODO: Update this, it was designed to work with path string not array of parts
        //var instance = new T();
        //var jObject = JObject.Parse(json);
        //foreach (var property in properties)
        //{
        //    JToken? value = null;
        //    if (property.Path is not null)
        //    {
        //        var pathParts = property.Path.Split('.');
        //        value = jObject[pathParts[0]];
        //        foreach (var part in pathParts[1..])
        //        {
        //            value = value[part];
        //        }
        //    }
        //    else if (property.Name is not null)
        //        value = jObject[property.Name];
        //    else
        //        value = jObject[property.Property.Name];

        //    property.Property.SetValue(instance, value?.ToObject(property.Property.PropertyType));
        //}

        //return instance;
    }

    private static void MoveJsonData(JObject jObject, string[] originalPath, string[] newPath)
    {
        var valueToMove = jObject[originalPath[0]];
        foreach (var pathPart in originalPath[1..])
        {
            valueToMove = valueToMove[pathPart];
        }

        if (jObject[newPath[0]] is null)
            jObject[newPath[0]] = new JObject();
        var currentObject = jObject[newPath[0]];

        if (newPath.Length > 1)
        {
            foreach (var pathPart in newPath[1..^1]) // TODO: handle nulls
            {
                if (currentObject[pathPart] is null)
                    currentObject[pathPart] = new JObject();

                currentObject = currentObject[pathPart];
            }
        }

        if (newPath.Length > 1)
        {
            var name = newPath[^1];
            currentObject[name] = valueToMove;
        }
        else
        {
            jObject[newPath[0]] = valueToMove;
        }

        if (originalPath.Length < 2)
        {
            jObject[originalPath[0]].Parent.Remove();
        }
        else
        {
            jObject[originalPath[^2]]
                .Children<JProperty>()
                .First(x => x.Name == originalPath[^1])
                .Remove();
        }
    }

    private static List<PathToModify> GetPathsToModify(Type type, string startingPath = "")
    {
        if (type.IsPrimitive)
            return EmptyList;

        var pathsToModify = new List<PathToModify>();

        foreach (var property in type.GetProperties())
            pathsToModify.AddRange(GetPathsToModifyForNonPrimitiveProperty(property, startingPath));

        return pathsToModify;
    }

    private static List<PathToModify> GetPathsToModifyForNonPrimitiveProperty(PropertyInfo property, string currentPath)
    {
        var pathsToModify = new List<PathToModify>();

        var jsonPathAttribute = property.GetCustomAttribute<JsonPathAttribute>();
        if (jsonPathAttribute is null)
            currentPath += $".{GetJsonPropertyName(property)}";
        else
        {
            var originalPath = $"{currentPath}.{GetJsonPropertyName(property)}";
            currentPath = jsonPathAttribute.GetPropertyPath(currentPath, property);

            pathsToModify.Add(new PathToModify { OriginalPath = originalPath[1..], NewPath = currentPath[1..] });
        }

        pathsToModify.AddRange(GetPathsToModify(property.PropertyType, currentPath));
        return pathsToModify;
    }

    private static string GetPropertyPath(this JsonPathAttribute jsonPathAttribute, string currentPath, PropertyInfo property)
    {
        currentPath += "." + jsonPathAttribute.Path;
        if (currentPath[^1] == '.') // If path ends with . - then use original property name, otherwise - last part is property name
            currentPath += property.Name;

        return currentPath;
    }

    private static string GetJsonPropertyName(PropertyInfo property)
    {
        var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
        if (jsonPropertyAttribute is not null)
            return jsonPropertyAttribute.PropertyName;

        return property.Name;
    }
}
