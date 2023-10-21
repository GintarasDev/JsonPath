using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace JsonPath;

internal class PathToModify
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

public static class JsonPathConvert
{
    private const string DICTIONARY_KEY_PLACEHOLDER = "DictionaryKeyPlaceholder_Dϖὗf5Ἡ🚘";
    private const string ITERATOR_PLACEHOLDER = "IteratorPlaceholder_Aϖὗf5Ἡ🚘";

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

    public static void ClearCache() => KnownTypes.Clear();

    public static string SerializeObject(object? value) =>
        SerializeObject(value, null, JsonConvert.SerializeObject);

    public static string SerializeObject(object? value, Formatting formatting) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, formatting));

    public static string SerializeObject(object? value, params JsonConverter[] converters) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, converters));

    public static string SerializeObject(object? value, Formatting formatting, params JsonConverter[] converters) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, formatting, converters));

    public static string SerializeObject(object? value, JsonSerializerSettings? settings) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, settings));

    public static string SerializeObject(object? value, Type? type, JsonSerializerSettings? settings) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, type, settings));

    public static string SerializeObject(object? value, Formatting formatting, JsonSerializerSettings? settings) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, formatting, settings));

    public static string SerializeObject(object? value, Type? type, Formatting formatting, JsonSerializerSettings? settings) =>
        SerializeObject(value, null, internalValue => JsonConvert.SerializeObject(internalValue, type, formatting, settings));

    /**
     * Note: to use this overload serializeObjectMethod must be able to serialize Dictionary<string, string>
     */
    public static string SerializeObject(object? objectToSerialize, JsonSerializerSettings? settings, Func<object?, string> serializeObjectMethod)
    {
        if (objectToSerialize is null)
            return serializeObjectMethod(objectToSerialize);

        settings = GetSettingsToUse(settings);

        var type = objectToSerialize.GetType();
        var pathsToModify = type.GetAllPathsToModify(settings);
        if (pathsToModify.Count == 0)
            return serializeObjectMethod(objectToSerialize);

        var flattenedJson = GetFlattenedJsonDictionaryFromObject(objectToSerialize!);
        UpdateJsonPaths(flattenedJson, pathsToModify);
        var deepStructure = GenerateDeepObjectsStructure(flattenedJson);
        return serializeObjectMethod(deepStructure);
    }

    public static object? DeserializeObject(string value) =>
        JsonConvert.DeserializeObject(value);

    public static object? DeserializeObject(string value, JsonSerializerSettings settings) =>
        JsonConvert.DeserializeObject(value, settings);

    public static object? DeserializeObject(string value, Type type) =>
        DeserializeObject(value, type, null, internalValue => JsonConvert.DeserializeObject(internalValue, type), JsonConvert.SerializeObject);

    public static T? DeserializeObject<T>(string value) where T : new() =>
        DeserializeObject(value, typeof(T), null, internalValue => JsonConvert.DeserializeObject<T>(internalValue), JsonConvert.SerializeObject);

    public static T? DeserializeObject<T>(string value, params JsonConverter[] converters) where T : new() =>
        DeserializeObject(value, typeof(T), null, internalValue => JsonConvert.DeserializeObject<T>(internalValue, converters), internalValue => JsonConvert.SerializeObject(internalValue, converters));

    public static T? DeserializeObject<T>(string value, JsonSerializerSettings? settings) where T : new() =>
        DeserializeObject(value, typeof(T), settings, internalValue => JsonConvert.DeserializeObject<T>(internalValue, settings), internalValue => JsonConvert.SerializeObject(internalValue, settings));

    public static object? DeserializeObject(string value, Type type, params JsonConverter[] converters) =>
        DeserializeObject(value, type, null, internalValue => JsonConvert.DeserializeObject(internalValue, type), internalValue => JsonConvert.SerializeObject(internalValue, converters));

    public static object? DeserializeObject(string value, Type? type, JsonSerializerSettings? settings) =>
        DeserializeObject(value, type, settings, internalValue => JsonConvert.DeserializeObject(internalValue, type, settings), internalValue => JsonConvert.SerializeObject(internalValue, type, settings));

    /**
     * Note: to use this overload deserializeObjectMethod must be able to serialize Dictionary<string, string>
     */
    public static T? DeserializeObject<T>(
        string json,
        Type type,
        JsonSerializerSettings? settings,
        Func<string, T?> deserializeObjectMethod,
        Func<object?, string> serializeObjectMethod) where T : new()
    {
        if (string.IsNullOrEmpty(json))
            return deserializeObjectMethod(json);

        settings = GetSettingsToUse(settings);

        var pathsToModify = type?.GetAllPathsToModify(settings);
        if (pathsToModify is null || !pathsToModify.Any())
            return deserializeObjectMethod(json);

        var flattenedJson = GetFlattenedJsonDictionaryFromJson(json);
        UpdateJsonPaths(flattenedJson, pathsToModify, isInverted: true);

        var deepStructure = GenerateDeepObjectsStructure(flattenedJson);
        var remappedJson = serializeObjectMethod(deepStructure);
        return deserializeObjectMethod(remappedJson);
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
        var pathParts = keyValuePair.Key.Split('.');
        var current = root;
        for (int i = 0; i < pathParts.Length; i++)
        {
            var part = pathParts[i];
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
        if (currentPathPartIndex == pathParts.Length - 1)
            current.Add(part, keyValuePair.Value);
        else
        {
            if (!current.ContainsKey(part))
                current.Add(part, new Dictionary<string, object?>());
            current = (Dictionary<string, object?>)current[part];
        }

        return current;
    }

    private static Dictionary<string, object> AddIndexedElement(Dictionary<string, object> current, string listName, int index)
    {
        if (!current.ContainsKey(listName))
            current.Add(listName, new List<Dictionary<string, object>>());
        var list = (List<Dictionary<string, object>>)current[listName];

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
            if (pathModification.IsDictionary)
                UpdateDictionaryPaths(jsonDictionary, originalPath, newPath);
            else if (pathModification.IsEnumerable)
                UpdateEnumerablePaths(jsonDictionary, originalPath, newPath);
            else if (jsonDictionary.ContainsKey(originalPath))
            {
                jsonDictionary[newPath] = jsonDictionary[originalPath];
                jsonDictionary.Remove(originalPath);
            }
        }
    }

    private static void UpdateDictionaryPaths(Dictionary<string, object?> jsonDictionary, string originalPath, string newPath)
    {
        var matchingPaths = GetMatchingDictionaryPaths(jsonDictionary, originalPath);
        var orgPathParts = originalPath.Split(DICTIONARY_KEY_PLACEHOLDER);
        var newPathParts = newPath.Split(DICTIONARY_KEY_PLACEHOLDER);
        foreach (var matchingPath in matchingPaths)
        {
            var pathParts = matchingPath.Split(".");
            var updatedPath = matchingPath;
            var currentSkipCount = 0;

            for (var i = 0; i < orgPathParts.Length; i++)
            {
                if (string.IsNullOrEmpty(orgPathParts[i]))
                    continue;

                var enumerableIndices = GetEnumerableIndex(pathParts.Length > i * 2 ? pathParts[i * 2] : pathParts[i]);
                var replacement = newPathParts[i];
                var valueToReplace = orgPathParts[i];
                foreach (var index in enumerableIndices)
                {
                    replacement = replacement.Replace(ITERATOR_PLACEHOLDER, index);
                    valueToReplace = valueToReplace.Replace(ITERATOR_PLACEHOLDER, index);
                }

                updatedPath = updatedPath.ReplaceFirst(valueToReplace, replacement, out var indexToContinueFrom, currentSkipCount);
                currentSkipCount = indexToContinueFrom;
            }

            jsonDictionary[updatedPath] = jsonDictionary[matchingPath];
            jsonDictionary.Remove(matchingPath);
        }
    }

    private static void UpdateEnumerablePaths(Dictionary<string, object?> jsonDictionary, string originalPath, string newPath)
    {
        foreach (var matchingPath in GetMatchingEnumerablePaths(jsonDictionary, originalPath))
        {
            var updatedPath = PrepareNewIEnumerablePath(matchingPath, newPath);
            jsonDictionary[updatedPath] = jsonDictionary[matchingPath];
            jsonDictionary.Remove(matchingPath);
        }
    }

    private static string[] GetEnumerableIndex(string pathPart)
    {
        var matches = Regex.Matches(pathPart, @"\[(\d+)\]");
        return matches.Cast<Match>().Select(m => m.Value).ToArray();
    }

    private static string PrepareNewIEnumerablePath(string matchingPath, string newPath)
    {
        var result = newPath;
        var indices = GetEnumerablePathIndices(matchingPath);

        var regex = new Regex(ITERATOR_PLACEHOLDER);
        foreach (var i in indices)
            result = regex.Replace(result, $"[{i}]", 1);

        return result;
    }

    private static int[] GetEnumerablePathIndices(string path)
    {
        var matches = Regex.Matches(path, @"\[(\d+)\]");
        return matches.Select(m => int.Parse(m.Groups[1].Value)).ToArray();
    }

    private static string[] GetMatchingEnumerablePaths(Dictionary<string, object> jsonDictionary, string path)
    {
        var pattern = "^" + Regex.Escape(path)
            .Replace(ITERATOR_PLACEHOLDER, @"\[\d+\]")
            .Replace(DICTIONARY_KEY_PLACEHOLDER, @".+?");
        return jsonDictionary.Keys
            .Where(key => Regex.IsMatch(key, pattern))
            .ToArray();
    }

    private static string[] GetMatchingDictionaryPaths(Dictionary<string, object> jsonDictionary, string path)
    {
        var pattern = Regex.Escape(path)
            .Replace(DICTIONARY_KEY_PLACEHOLDER, "\\..*")
            .Replace(ITERATOR_PLACEHOLDER, @"\[\d+\]");
        return jsonDictionary.Keys
            .Where(key => Regex.IsMatch(key, pattern))
            .ToArray();
    }

    private static List<PathToModify> GetAllPathsToModify(this Type type, JsonSerializerSettings? settings)
    {
        if (KnownTypes.TryGetValue(type, out var result))
            return result;

        var pathsToModify = type.GetPathsToModify(settings);
        KnownTypes.TryAdd(type, pathsToModify);
        return pathsToModify;
    }

    private static List<PathToModify> GetPathsToModify(this Type type, JsonSerializerSettings? settings, PropertyInfo? propertyInfo = null)
    {
        var queue = new Queue<(Type type, string path, string newPath, bool isEnumerable, bool isDictionary)>();
        queue.Enqueue((type, propertyInfo is null ? "" : propertyInfo.Name, propertyInfo?.GetCustomAttribute<JsonPathAttribute>()?.Path ?? propertyInfo?.Name ?? "", false, false));
        
        var pathsToModify = new List<PathToModify>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            type = current.type;
            var myName = current.path;
            var myNewName = current.newPath;
            var isEnumerable = current.isEnumerable;
            var isDictionary = current.isDictionary;

            if (type.IsAssignableTo(typeof(IEnumerable))
                && !type.IsAssignableTo(typeof(IDictionary))
                && type != typeof(string))
            {
                if (type.IsGenericType)
                    type = type.GetGenericArguments()[0];
                else if (type.IsArray)
                    type = type.GetElementType()!;
                myName += ITERATOR_PLACEHOLDER;
                myNewName += ITERATOR_PLACEHOLDER;
                isEnumerable = true;
            }

            if (type.IsAssignableTo(typeof(IDictionary)))
            {
                type = type.GetGenericArguments()[1];
                myName += DICTIONARY_KEY_PLACEHOLDER;
                myNewName += DICTIONARY_KEY_PLACEHOLDER;
                isDictionary = true;
            }

            if (type.CanHaveSubPaths(settings))
            {
                var propertiesToCheck = type.GetProperties();

                myName += ".";
                myNewName += ".";
                foreach (var property in propertiesToCheck)
                {
                    var propertyPath = property.GetCustomAttribute<JsonPathAttribute>()?.Path ?? property.Name;
                    if (propertyPath.EndsWith("."))
                        propertyPath += property.Name;
                    queue.Enqueue((property.PropertyType, $"{myName}{property.Name}", $"{myNewName}{propertyPath}", isEnumerable, isDictionary));
                }
            }
            else
            {
                if (myName != myNewName)
                    pathsToModify.Add(new()
                    {
                        OriginalPath = myName[1..],
                        NewPath = myNewName[1..],
                        IsEnumerable = isEnumerable,
                        IsDictionary = isDictionary
                    });
            }
        }

        return pathsToModify;
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

    private static string ReplaceFirst(
        this string org,
        string valueToReplace,
        string replacementValue,
        out int indexAfterReplacement,
        int skipCount = 0)
    {
        indexAfterReplacement = 0;
        if (skipCount >= org.Length)
            return org;

        int pos = org[skipCount..].IndexOf(valueToReplace) + skipCount;

        if (pos < skipCount)
            return org;

        indexAfterReplacement = pos + replacementValue.Length;
        return org.Remove(pos, valueToReplace.Length).Insert(pos, replacementValue);
    }
}
