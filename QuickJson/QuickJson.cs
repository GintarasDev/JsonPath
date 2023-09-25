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
    public bool IsDictionary = false;

    public (string originalPath, string newPath) GetPathsForSwapping(bool isInverted)
    {
        if (isInverted)
            return (originalPath: NewPath, newPath: OriginalPath);

        return (originalPath: OriginalPath, newPath: NewPath);
    }
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
        typeof(string),
        typeof(decimal),
        typeof(decimal?),
    };

    private static JsonSerializerSettings? _defaultSettings = null;

    private static readonly Dictionary<Type, List<PathToModify>> KnownTypes = new();

    public static void ClearCache()
    {
        KnownTypes.Clear();
    }

    public static string SerializeObject(object? objectToSerialize, JsonSerializerSettings? settings = null)
    {
        if (objectToSerialize is null)
            return JsonConvert.SerializeObject(objectToSerialize, settings);

        settings = GetSettingsToUse(settings);

        var type = objectToSerialize.GetType();
        var pathsToModify = type.GetAllPathsToModify(settings);
        if (!pathsToModify.Any())
            return JsonConvert.SerializeObject(objectToSerialize, settings);

        var flattenedJson = GetFlattenedJsonDictionaryFromObject(objectToSerialize!);
        UpdateJsonPaths(flattenedJson, pathsToModify);
        var deepStructure = GenerateDeepObjectsStructure(flattenedJson);
        return JsonConvert.SerializeObject(deepStructure, settings);
    }

    public static T? DeserializeObject<T>(string json, JsonSerializerSettings? settings = null) where T : new()
    {
        if (string.IsNullOrEmpty(json))
            return JsonConvert.DeserializeObject<T>(json, settings);

        if (settings is null)
        {
            if (_defaultSettings is null)
                _defaultSettings = JsonConvert.DefaultSettings?.Invoke();
            settings = _defaultSettings;
        }

        var type = typeof(T);
        var pathsToModify = type.GetAllPathsToModify(settings);
        if (pathsToModify.Any())
        {
            var flattenedJson = GetFlattenedJsonDictionaryFromJson(json);
            UpdateJsonPaths(flattenedJson, pathsToModify, isInverted: true);
            var deepStructure = GenerateDeepObjectsStructure(flattenedJson);
            var remappedJson = JsonConvert.SerializeObject(deepStructure, settings);
            return JsonConvert.DeserializeObject<T>(remappedJson);
        }

        return JsonConvert.DeserializeObject<T>(json);
    }

    private static JsonSerializerSettings GetSettingsToUse(JsonSerializerSettings? settings)
    {
        if (settings is not null)
            return settings;

        return _defaultSettings ?? JsonConvert.DefaultSettings?.Invoke()!;
    }

    private static Dictionary<string, object?> GetFlattenedJsonDictionaryFromObject(object objectToSerialize) =>
        ToFlattenedJsonDictionary(JObject.FromObject(objectToSerialize));

    private static Dictionary<string, object?> GetFlattenedJsonDictionaryFromJson(string json) =>
        ToFlattenedJsonDictionary(JObject.Parse(json));

    private static Dictionary<string, object?> ToFlattenedJsonDictionary(JObject jObject) =>
        jObject.Descendants()
            .OfType<JValue>()
            .ToDictionary(v => v.Path, v => v.Value);

    private static Dictionary<string, object> GenerateDeepObjectsStructure(Dictionary<string, object?> jsonDictionary)
    {
        var root = new Dictionary<string, object>();
        foreach (var keyValuePair in jsonDictionary)
            ConvertKeyValuePairIntoDeepObjectStructure(keyValuePair, root);

        return root;
    }

    private static void ConvertKeyValuePairIntoDeepObjectStructure(KeyValuePair<string, object?> keyValuePair, Dictionary<string, object> root)
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
        Dictionary<string, object?> current,
        string[] pathParts,
        string part,
        int currentPathPartIndex,
        KeyValuePair<string, object?> keyValuePair)
    {
        // Check if this is the last part of the key
        if (currentPathPartIndex == pathParts.Length - 1)
            current.Add(part, keyValuePair.Value);
        else
        {
            // Ensure that the current dictionary contains a dictionary with this name
            if (!current.ContainsKey(part))
                current.Add(part, new Dictionary<string, object?>());
            current = (Dictionary<string, object?>)current[part];
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

    private static void UpdateJsonPaths(Dictionary<string, object?> jsonDictionary, List<PathToModify> pathsToModify, bool isInverted = false)
    {
        foreach (var pathModification in pathsToModify)
        {
            var (originalPath, newPath) = pathModification.GetPathsForSwapping(isInverted);

            if (pathModification.IsEnumerable)
            {
                foreach (var matchingPath in GetMatchingEnumerablePaths(jsonDictionary, originalPath))
                {
                    var newIEnumerablePath = PrepareNewIEnumerablePath(matchingPath, newPath);
                    jsonDictionary[newIEnumerablePath] = jsonDictionary[matchingPath];
                    jsonDictionary.Remove(matchingPath);
                }
            }
            else if (pathModification.IsDictionary)
            {
                var pattern = Regex.Escape(pathModification.OriginalPath).Replace("\\.\\*", "\\..*"); //pathModification.OriginalPath.Replace(".", "\\.").Replace("*", ".*");
                var pathsToUpdate = jsonDictionary
                    .Where(d => Regex.IsMatch(d.Key, pattern))
                    .ToList();

                var orgPathParts = pathModification.OriginalPath.Split(".");
                var keysIds = new List<int>();
                for (var i = 0; i < orgPathParts.Length; i++)
                {
                    if (orgPathParts[i] == "*")
                        keysIds.Add(i);
                }
                foreach (var path in pathsToUpdate)
                {
                    var pathParts = path.Key.Split(".");

                    var newKey = pathModification.NewPath;
                    foreach (var keyId in keysIds)
                    {
                        newKey = newKey.ReplaceFirstKeyPlaceholder(pathParts[keyId]); // will this work with array nested in a dictionary?
                        // TODO: fix reg above comment
                    }

                    jsonDictionary[newKey] = jsonDictionary[path.Key];
                    jsonDictionary.Remove(path.Key);
                }
            }
            else
            {
                if (jsonDictionary.ContainsKey(originalPath))
                {
                    jsonDictionary[newPath] = jsonDictionary[originalPath];
                    jsonDictionary.Remove(originalPath);
                }
            }
        }
    }

    private static string ReplaceFirstKeyPlaceholder(this string text, string key)
    {
        const string KeyPattern = "*";
        const int KeyPatternLength = 1;
        const string ArrPattern = "[*]";
        const int ArrPatternLength = 3;

        int pos = text.IndexOf(KeyPattern);
        while (pos >= 0)
        {
            // Check if the found position is part of the skip pattern
            int skipPos = text.IndexOf(ArrPattern);
            if (skipPos >= 0 && pos > skipPos && pos < skipPos + ArrPatternLength)
            {
                // If it is, find the next occurrence of the search string
                pos = text.IndexOf(KeyPattern, pos + KeyPatternLength);
            }
            else
            {
                // If it's not part of the skip pattern, do the replacement
                return text.Substring(0, pos) + key + text.Substring(pos + KeyPatternLength);
            }
        }
        return text;
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

    private static string[] GetMatchingEnumerablePaths(Dictionary<string, object> jsonDictionary, string path)
    {
        var template = path.Replace("[*]", @"\[\d+\]\");
        return jsonDictionary.Keys.Where(key => Regex.IsMatch(key, template)).ToArray();
    }

    private static List<PathToModify> GetAllPathsToModify(this Type type, JsonSerializerSettings? settings)
    {
        if (KnownTypes.ContainsKey(type))
            return KnownTypes[type];

        var pathsToModify = new List<PathToModify>();
        foreach (var path in type.GetPathsToModify(settings))
        {
            if (path.path == path.newPath)
                continue;

            pathsToModify.Add(new()
            {
                OriginalPath = path.path[1..],
                NewPath = path.newPath[1..],
                IsEnumerable = path.isEnumerable,
                IsDictionary = path.isDictionary
            });
        }

        KnownTypes.TryAdd(type, pathsToModify);
        return pathsToModify;
    }

    // No proper support for dictionaries yet
    // No proper support for non generic enumerables
    private static IEnumerable<(string path, string newPath, bool isEnumerable, bool isDictionary)> GetPathsToModify(this Type type, JsonSerializerSettings? settings, PropertyInfo? propertyInfo = null)
    {
        var isEnumerable = false;
        var isDictionary = false;
        var myName = propertyInfo is null ? "" : propertyInfo.Name;

        var jsonPathAttribute = propertyInfo?.GetCustomAttribute<JsonPathAttribute>();
        var myNewName = jsonPathAttribute is null ? myName : jsonPathAttribute.Path;

        if (myNewName.EndsWith("."))
            myNewName += propertyInfo is null ? "WTF_JUST_HAPPENED" : propertyInfo.Name; // TODO: Update

        if (type.IsAssignableTo(typeof(IDictionary)))
        {
            type = type.GetGenericArguments()[1];
            myName += ".*";
            myNewName += ".*";
            isDictionary = true;
        }
        else if (type.IsAssignableTo(typeof(IEnumerable)) && type.IsGenericType)
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
                    yield return ($"{myName}{path.path}", $"{myNewName}{path.newPath}", isEnumerable || path.isEnumerable, isDictionary || path.isDictionary);
            }
        }
        else
            yield return (myName, myNewName, isEnumerable, isDictionary);
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
}
