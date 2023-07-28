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
    static readonly List<PathToModify> EmptyList = new();

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
            var originalPathParts = path.OriginalPath.Split('.');
            var newPathParts = path.NewPath.Split('.');
            var valueToMove = jObject[originalPathParts[0]];
            foreach (var pathPart in originalPathParts[1..])
            {
                valueToMove = valueToMove[pathPart];
            }

            if (jObject[newPathParts[0]] is null)
                jObject[newPathParts[0]] = new JObject();
            var currentObject = jObject[newPathParts[0]];

            foreach (var pathPart in newPathParts[1..^1])
            {
                if (currentObject[pathPart] is null)
                    currentObject[pathPart] = new JObject();

                currentObject = currentObject[pathPart];
            }

            var name = newPathParts[^1];
            currentObject[name] = valueToMove; 

            jObject[originalPathParts[^2]]
                .Children<JProperty>()
                .First(x => x.Name == originalPathParts[^1])
                .Remove();
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

    public static List<PathToModify> GetPathsToModify(Type type, string startingPath = "") // TODO: Use string builder?
    {
        if (type.IsPrimitive)
            return EmptyList;

        // TODO: make it so the path would be considered from parent
        var pathsToModify = new List<PathToModify>();

        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var startingPathForProperty = startingPath;

            var jsonPathAttribute = property.GetCustomAttribute<JsonPathAttribute>();
            if (jsonPathAttribute is null) //iterate deeper if null
                startingPathForProperty += "." + property.Name; // TODO: Update to use correct name if JsonPropertyAttribute from NewtonsoftJson was used. Maybe create QuickJson equivalent to not use different namespaces?
            else
            {
                var originalPath = startingPathForProperty + "." + property.Name;
                startingPathForProperty += "." + jsonPathAttribute.Path;
                if (startingPathForProperty[^1] == '.') // If path ends with . - then use original property name, otherwise - last part is property name
                    startingPathForProperty += property.Name;

                pathsToModify.Add(new PathToModify { OriginalPath = originalPath[1..], NewPath = startingPathForProperty[1..] });
            }

            pathsToModify.AddRange(GetPathsToModify(property.PropertyType, startingPathForProperty));
        }

        return pathsToModify;
    }
}
