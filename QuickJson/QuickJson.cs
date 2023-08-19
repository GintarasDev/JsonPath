using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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
    public static List<Type> TypesToIgnore = new()
    {
        typeof(DateTime),
        typeof(DateTime?),
        typeof(DateTimeOffset),
        typeof(DateTimeOffset?),
        typeof(Guid),
        typeof(Guid?),
        typeof(TimeSpan),
        typeof(TimeSpan?),
        typeof(string)
    };

    static JsonSerializerSettings? _defaultSettings = null;

    static readonly List<PathToModify> EmptyList = new(); // Order of this list is very important

    static readonly Dictionary<Type, List<PathToModify>> KnownTypes = new();

    public static string SerializeObject(object? objectToSerialize, JsonSerializerSettings? settings = null)
    {
        if (objectToSerialize is null)
            return JsonConvert.SerializeObject(objectToSerialize);

        var type = objectToSerialize?.GetType();

        if (settings is null)
        {
            if (_defaultSettings is null)
                _defaultSettings = JsonConvert.DefaultSettings?.Invoke();
            settings = _defaultSettings;
        }

        var pathsToModify = type.GetAllPaths(settings);
        return "";
        //var jObject = JObject.Parse(JsonConvert.SerializeObject(objectToSerialize));

        //var result = UpdateJsonPaths(jObject, pathsToModify);

        //return result.ToString();
    }

    private static Dictionary<string, string> GetAllPaths(this Type type, JsonSerializerSettings? settings)
    {
        var paths = new Dictionary<string, string>();
        foreach (var path in type.GetPathsToModify(settings))
            paths.Add(path.path, path.newPath);

        return paths;
    }

    private static IEnumerable<(string path, string newPath)> GetPathsToModify(this Type type, JsonSerializerSettings? settings, PropertyInfo? propertyInfo = null)
    {
        var jsonPathAttribute = propertyInfo?.GetCustomAttribute<JsonPathAttribute>();
        var myNewName = jsonPathAttribute is null ? "" : jsonPathAttribute.Path;
        if (myNewName.EndsWith("."))
            myNewName += propertyInfo is null ? "" : propertyInfo.Name;

        if (type.CanHaveSubPaths(settings))
        {
            var propertiesToCheck = type.GetProperties();

            // TODO: instead of returning string, return (string string) where the first one is path and the second one is new path
            //var propertiesWithModifications = propertiesToCheck
            //    .ToDictionary(p => p, p => p.GetCustomAttribute<JsonPathAttribute>())
            //    .Where(p => p.Value != null);
            var myName = propertyInfo is null ? "" : $"{propertyInfo.Name}.";
            myNewName = myNewName == "" ? myName : $"{myNewName}.";
            foreach (var property in propertiesToCheck)
            {
                foreach (var path in property.PropertyType.GetPathsToModify(settings, property))
                    yield return ($"{myName}{path.path}", $"{myNewName}{path.newPath}");
            }
        }
        else
        {
            var myName = propertyInfo is null ? "" : propertyInfo.Name;
            yield return (myName, myNewName == "" ? myName : myNewName);
        }
    }

    private static bool CanHaveSubPaths(this Type type, JsonSerializerSettings? settings) =>
        !type.IsPrimitive && !TypesToIgnore.Contains(type) && !type.HasCustomConverter(settings);

    private static bool HasCustomConverter(this Type type, JsonSerializerSettings? settings)
    {
        if (settings is null)
            return false;

        foreach (var converter in settings.Converters)
        {
            if (converter.CanConvert(type))
                return true;
        }
        return false;
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

    //private static List<PathToModify> GetPathsToModify(Type type, string startingPath = "")
    //{
    //    if (type.IsPrimitive || type.IsValueType) // TODO: we need a better solution as otherwise it wont work with any value types, not just built in ones
    //        return EmptyList;

    //    if (KnownTypes.ContainsKey(type))
    //        return KnownTypes[type];

    //    var pathsToModify = new List<PathToModify>();

    //    foreach (var property in type.GetProperties())
    //        pathsToModify.AddRange(GetPathsToModifyForNonPrimitiveProperty(property, startingPath));

    //    KnownTypes.Add(type, pathsToModify);
    //    return pathsToModify;
    //}

    //private static List<PathToModify> GetPathsToModifyForNonPrimitiveProperty(PropertyInfo property, string currentPath)
    //{
    //    var pathsToModify = new List<PathToModify>();

    //    var jsonPathAttribute = property.GetCustomAttribute<JsonPathAttribute>();
    //    if (jsonPathAttribute is null)
    //        currentPath += $".{GetJsonPropertyName(property)}";
    //    else
    //    {
    //        var originalPath = $"{currentPath}.{GetJsonPropertyName(property)}";
    //        currentPath = jsonPathAttribute.GetPropertyPath(currentPath, property);

    //        pathsToModify.Add(new PathToModify { OriginalPath = originalPath[1..], NewPath = currentPath[1..] });
    //    }

    //    pathsToModify.AddRange(GetPathsToModify(property.PropertyType, currentPath));
    //    return pathsToModify;
    //}

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
