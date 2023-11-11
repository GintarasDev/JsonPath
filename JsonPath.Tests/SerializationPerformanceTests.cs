using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonPath.Tests.TestingClasses;
using JsonPath.Tests.TestingClasses.WithoutAttributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonPath.Tests;

#nullable disable warnings
#pragma warning disable xUnit1013
public class SerializationPerformanceTests
{
    private BlogSimple _blogWithoutAttributes;
    private Blog _blog;

    private Dictionary<string, object?> _tempFlattenedJson;
    private List<PathToModify> _tempPathsToModify;
    private JObject _tempJObject;
    private string _tempJson;
    private JsonSerializerSettings _tempSerializerSettings;

    [Fact]
    public void RunPerformanceTests()
    {
        BenchmarkRunner.Run<SerializationPerformanceTests>();
    }

    [GlobalSetup]
    public void Setup()
    {
        _blogWithoutAttributes = Helpers.CreateTestBlogWithoutAttributes();
        _ = JsonPathConvert.SerializeObject(_blogWithoutAttributes); // Make sure type is cached

        _blog = Helpers.CreateTestBlog();
        _ = JsonPathConvert.SerializeObject(_blog); // Make sure type is cached

        var settings = JsonConvert.DefaultSettings?.Invoke();
        _tempSerializerSettings = settings;

        var type = _blog.GetType();
        _tempPathsToModify = JsonPathConvert.GetAllPathsToModify(type, settings);

        _tempFlattenedJson = JsonPathConvert.GetFlattenedJsonDictionaryFromObject(_blog!);

        _tempJObject = JObject.FromObject(_blog);
        _tempJson = JsonConvert.SerializeObject(_blog);
    }

    [Benchmark]
    public void Serialize()
    {
        _ = JsonPathConvert.SerializeObject(_blog);
    }

    //[Benchmark]
    //public void Serialize_WithoutCustomPaths()
    //{
    //    _ = JsonPathConvert.SerializeObject(_blogWithoutAttributes);
    //}

    //[Benchmark(Baseline = true)]
    //public void Serialize_WithPureNewtonsoft()
    //{
    //    _ = JsonConvert.SerializeObject(_blogWithoutAttributes);
    //}


    //[Benchmark]
    //public void GetPathsToModify()
    //{
    //    _ = JsonPathConvert.GetPathsToModify(typeof(Blog), null).ToList();
    //}

    //[Benchmark]
    //public void GetPathsToModify_WithoutCache()
    //{
    //    JsonPathConvert.ClearCache();
    //    _ = JsonPathConvert.GetPathsToModify(typeof(Blog), null).ToList();
    //}


    //[Benchmark]
    //public void UpdateJsonPaths()
    //{
    //    JsonPathConvert.UpdateJsonPaths(_tempFlattenedJson, _tempPathsToModify);
    //}

    //[Benchmark]
    //public void GenerateDeepObjectsStructure()
    //{
    //    _ = JsonPathConvert.GenerateDeepObjectsStructure(_tempFlattenedJson);
    //}

    [Benchmark]
    public void GetFlattenedJsonDictionaryFromObjectV2()
    {
        _ = JsonPathConvert.GetFlattenedJsonDictionaryFromObjectV2(_blog, _tempSerializerSettings);
    }

    [Benchmark]
    public void GetFlattenedJsonDictionaryFromObject()
    {
        _ = JsonPathConvert.GetFlattenedJsonDictionaryFromObject(_blog);
    }

    //[Benchmark]
    //public void GetFlattenedJsonDictionaryFromObject()
    //{
    //    _ = JsonPathConvert.ToFlattenedJsonDictionary(_tempJObject);
    //}

    //[Benchmark]
    //public void JObjectFromObject()
    //{
    //    _ = JObject.FromObject(_blog);
    //}

    //[Benchmark]
    //public void JObjectParse()
    //{
    //    _ = JObject.Parse(_tempJson);
    //}

    //[Benchmark]
    //public void SerializeObject_WithDictionary()
    //{
    //    _ = JsonConvert.SerializeObject(_tempFlattenedJson);
    //}

    //[Benchmark]
    //public void GetSettingsToUse()
    //{
    //    _ = JsonPathConvert.GetSettingsToUse(null);
    //}
}
