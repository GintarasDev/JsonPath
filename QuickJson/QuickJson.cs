using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace QuickJson;

public class PathToModify
{
    public required string OriginalPath;
    public required string NewPath;
    public bool IsEnumerable = false;
}

// TODO: we need to be able to convert this to json:
/*
BlogId
Author.Id
Author.Name
Author.Description
Sponsor.Name
Sponsor.Description
Sponsor.NumberOfAds
Sponsor.Metadata.SponsorSince
Author.Sponsor.Name
Author.Sponsor.Description
Author.Sponsor.NumberOfAds
Author.Sponsor.Metadata.SponsorSince
Articles[0].Title
Articles[1].Title
Articles[1].Content
Articles[1].Metadata.PublishedOn
Articles[0].Comments[1].Author.Name
Articles[0].Comments[1].Content
Articles[1].Comments[0].Author.Name
Articles[1].Comments[0].Content
Articles[0].Content
Articles[0].Metadata.PublishedOn,
Articles[0].Comments[0].Author.Name
Articles[1].Comments[1].Author.Name
Articles[1].Comments[1].Content
Articles[0].Comments[0].Content
*/

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

    static readonly Dictionary<Type, List<PathToModify>> KnownTypes = new();

    public static string SerializeObject(object? objectToSerialize, JsonSerializerSettings? settings = null)
    {
        if (objectToSerialize is null)
            return JsonConvert.SerializeObject(objectToSerialize, settings);

        if (settings is null)
        {
            if (_defaultSettings is null)
                _defaultSettings = JsonConvert.DefaultSettings?.Invoke();
            settings = _defaultSettings;
        }

        var type = objectToSerialize?.GetType();
        var pathsToModify = type.GetAllPathsToModify(settings);
        if (pathsToModify.Any())
        { 
            var flattenedJson = GetFlattenedJsonDictionary(objectToSerialize);
            UpdateJsonPaths(flattenedJson, pathsToModify);
            var deepStructure = GenerateDeepObjectsStructure(flattenedJson);
            return JsonConvert.SerializeObject(deepStructure, settings);
        }

        return JsonConvert.SerializeObject(objectToSerialize, settings);
    }

    // TODO: refactor this
    private static Dictionary<string, object> GenerateDeepObjectsStructure(Dictionary<string, string> jsonDictionary)
    {
        var root = new Dictionary<string, object>();
        foreach (var keyValuePair in jsonDictionary)
        {
            // Split the key into its parts
            var keyParts = Regex.Split(keyValuePair.Key, @"(?<=[\w\]])\.");
            var current = root;
            for (int i = 0; i < keyParts.Length; i++)
            {
                var part = keyParts[i];
                // Check if the part represents an indexed element in a list
                if (TryGetListIndex(part, out string listName, out int index))
                {
                    // Ensure that the current dictionary contains a list with this name
                    if (!current.ContainsKey(listName))
                        current.Add(listName, new List<Dictionary<string, object>>());
                    var list = (List<Dictionary<string, object>>)current[listName];
                    // Ensure that the list has enough elements to include the specified index
                    while (list.Count <= index)
                        list.Add(new Dictionary<string, object>());
                    current = list[index];
                }
                else
                {
                    // Check if this is the last part of the key
                    if (i == keyParts.Length - 1)
                        current.Add(part, keyValuePair.Value);
                    else
                    {
                        // Ensure that the current dictionary contains a dictionary with this name
                        if (!current.ContainsKey(part))
                            current.Add(part, new Dictionary<string, object>());
                        current = (Dictionary<string, object>)current[part];
                    }
                }
            }
        }

        return root;
    }

    private static bool TryGetListIndex(string part, out string listName, out int index)
    {
        var match = Regex.Match(part, @"(.+)\[(\d+)\]$");
        if (match.Success)
        {
            listName = match.Groups[1].Value;
            index = int.Parse(match.Groups[2].Value);
            return true;
        }
        else
        {
            listName = null;
            index = -1;
            return false;
        }
    }

    private static Dictionary<string, string> GetFlattenedJsonDictionary(object? objectToSerialize) =>
        JObject.FromObject(objectToSerialize)
            .Descendants()
            .OfType<JValue>()
            .ToDictionary(v => v.Path, v => v.ToString());

    private static void UpdateJsonPaths(Dictionary<string, string> jsonDictionary, List<PathToModify> pathsToModify)
    {
        foreach (var pathModification in pathsToModify.Where(p => !p.IsEnumerable))
        {
            if (jsonDictionary.ContainsKey(pathModification.OriginalPath))
            {
                jsonDictionary[pathModification.NewPath] = jsonDictionary[pathModification.OriginalPath];
                jsonDictionary.Remove(pathModification.OriginalPath);
            }
        }

        foreach (var pathModification in pathsToModify.Where(p => p.IsEnumerable))
        {
            foreach (var matchingPath in GetMatchingEnumerablePaths(jsonDictionary, pathModification.OriginalPath))
            {
                var newPath = PrepareNewIEnumerablePath(matchingPath, pathModification.NewPath);
                jsonDictionary[newPath] = jsonDictionary[matchingPath];
                jsonDictionary.Remove(matchingPath);
            }
        }
    }

    private static string PrepareNewIEnumerablePath(string matchingPath, string newPath)
    {
        var result = newPath;
        var indices = GetEnumerablePathIndices(matchingPath);

        var regex = new Regex(@"\[\*\]");
        foreach (var i in indices)
        {
            result = regex.Replace(result, $"[{i}]", 1);
        }

        return result;
    }

    private static int[] GetEnumerablePathIndices(string path)
    {
        var matches = Regex.Matches(path, @"\[(\d+)\]");
        return matches.Select(m => int.Parse(m.Groups[1].Value)).ToArray();
    }

    private static string[] GetMatchingEnumerablePaths(Dictionary<string, string> jsonDictionary, string path)
    {
        var template = path.Replace("[*]", @"\[\d+\]\");
        return jsonDictionary.Keys.Where(key => Regex.IsMatch(key, template)).ToArray();
    }

    private static List<PathToModify> GetAllPathsToModify(this Type type, JsonSerializerSettings? settings)
    {
        var pathsToModify = new List<PathToModify>();
        foreach (var path in type.GetPathsToModify(settings))
        {
            if (path.path == path.newPath)
                continue;

            pathsToModify.Add(new()
            {
                OriginalPath = path.path[1..],
                NewPath = path.newPath[1..],
                IsEnumerable = path.isEnumerable
            });
        }

        return pathsToModify;
    }

    // No proper support for dictionaries yet
    // No proper support for non generic enumerables
    private static IEnumerable<(string path, string newPath, bool isEnumerable)> GetPathsToModify(this Type type, JsonSerializerSettings? settings, PropertyInfo? propertyInfo = null)
    {
        var isEnumerable = false;
        var myName = propertyInfo is null ? "" : propertyInfo.Name;

        var jsonPathAttribute = propertyInfo?.GetCustomAttribute<JsonPathAttribute>();
        var myNewName = jsonPathAttribute is null ? myName : jsonPathAttribute.Path;

        if (myNewName.EndsWith("."))
            myNewName += propertyInfo is null ? "" : propertyInfo.Name;

        if (type.IsAssignableTo(typeof(IEnumerable)) && type.IsGenericType)
        {
            type = type.GetGenericArguments()[0];
            myName += "[*]";
            myNewName += "[*]";
            isEnumerable = true;
        }

        if (type.CanHaveSubPaths(settings))
        {
            var propertiesToCheck = type.GetProperties();

            // TODO: instead of returning string, return (string string) where the first one is path and the second one is new path
            //var propertiesWithModifications = propertiesToCheck
            //    .ToDictionary(p => p, p => p.GetCustomAttribute<JsonPathAttribute>())
            //    .Where(p => p.Value != null);
            myName += ".";
            myNewName += ".";
            foreach (var property in propertiesToCheck)
            {
                foreach (var path in property.PropertyType.GetPathsToModify(settings, property))
                    yield return ($"{myName}{path.path}", $"{myNewName}{path.newPath}", isEnumerable || path.isEnumerable);
            }
        }
        else
            yield return (myName, myNewName, isEnumerable);
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
