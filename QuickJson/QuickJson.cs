using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
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

    private static Dictionary<string, object> GenerateDeepObjectsStructure(Dictionary<string, string> jsonDictionary)
    {
        var root = new Dictionary<string, object>();
        foreach (var keyValuePair in jsonDictionary)
            ConvertKeyValuePairIntoDeepObjectStructure(keyValuePair, root);

        return root;
    }

    private static void ConvertKeyValuePairIntoDeepObjectStructure(KeyValuePair<string, string> keyValuePair, Dictionary<string, object> root)
    {
        // Split the key into its parts
        var pathParts = keyValuePair.Key.Split('.');
        var current = root;
        for (int i = 0; i < pathParts.Length; i++)
        {
            var part = pathParts[i];
            // Check if the part represents an indexed element in a list
            if (TryGetListIndex(part, out string listName, out int index))
                current = AddIndexedElement(current, listName, index);
            else
                current = AddElement(current, pathParts, part, i, keyValuePair);
        }
    }

    private static Dictionary<string, object> AddElement(
        Dictionary<string, object> current,
        string[] pathParts,
        string part,
        int currentPathPartIndex,
        KeyValuePair<string, string> keyValuePair)
    {
        // Check if this is the last part of the key
        if (currentPathPartIndex == pathParts.Length - 1)
            current.Add(part, keyValuePair.Value);
        else
        {
            // Ensure that the current dictionary contains a dictionary with this name
            if (!current.ContainsKey(part))
                current.Add(part, new Dictionary<string, object>());
            current = (Dictionary<string, object>)current[part];
        }

        return current;
    }

    private static Dictionary<string, object> AddIndexedElement(Dictionary<string, object> current, string listName, int index)
    {
        // Ensure that the current dictionary contains a list with this name
        if (!current.ContainsKey(listName))
            current.Add(listName, new List<Dictionary<string, object>>());
        var list = (List<Dictionary<string, object>>)current[listName];
        // Ensure that the list has enough elements to include the specified index
        while (list.Count <= index)
            list.Add(new Dictionary<string, object>());
        return list[index];
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

    public static T DeserializeObject<T>(string json) where T : new()
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}
