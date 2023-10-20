using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonPath.Tests.TestingClasses;
using JsonPath.Tests.TestingClasses.WithoutAttributes;
using Newtonsoft.Json;

namespace JsonPath.Tests;

#nullable disable warnings
#pragma warning disable xUnit1013
public class SerializationPerformanceTests
{
    private BlogSimple _blogWithoutAttributes;
    private Blog _blog;

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
    }

    [Benchmark]
    public void Serialize()
    {
        _ = JsonPathConvert.SerializeObject(_blog);
    }

    [Benchmark]
    public void Serialize_WithoutCustomPaths()
    {
        _ = JsonPathConvert.SerializeObject(_blogWithoutAttributes);
    }

    [Benchmark(Baseline = true)]
    public void Serialize_WithPureNewtonsoft()
    {
        _ = JsonConvert.SerializeObject(_blogWithoutAttributes);
    }
}
