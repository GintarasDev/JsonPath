using Newtonsoft.Json;
using System.Diagnostics;

namespace JsonPath.Tests;

public class SerializationPerformanceTests
{
    private const int NumberOfTestingIterations = 25000;
    private const float AcceptableDifferenceMultiplier = 30f;
    private const float AcceptableDifferenceWithoutAttributesMultiplier = 2f;

    [Fact(Skip = "Temp")]
    public void Serialize_WithCustomPaths_PerformsAcceptibly()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();
        _ = JsonPathConvert.SerializeObject(blog); // Make sure type is cached

        Warmup();
        var elapsedTicksNewtonsoftJson = GetAverageSpeed(() => JsonConvert.SerializeObject(blog));

        // Act
        var elapsedTicksJsonPath = GetAverageSpeed(() => JsonPathConvert.SerializeObject(blog));

        // Assert
        Assert.True(
            elapsedTicksNewtonsoftJson * AcceptableDifferenceMultiplier >= elapsedTicksJsonPath,
            $"NewtonsonJson took: {elapsedTicksNewtonsoftJson} ticks\n" +
            $"JsonPath took: {elapsedTicksJsonPath} ticks");
    }

    [Fact(Skip = "Temp")]
    public void Serialize_WithoutCustomPaths_PerformsWell()
    {
        // Arrange
        var blog = Helpers.CreateTestBlogWithoutAttributes();
        _ = JsonPathConvert.SerializeObject(blog); // Make sure type is cached

        Warmup();
        var elapsedTicksNewtonsoftJson = GetAverageSpeed(() => JsonConvert.SerializeObject(blog));

        // Act
        var elapsedTicksJsonPath = GetAverageSpeed(() => JsonPathConvert.SerializeObject(blog));

        // Assert
        Assert.True(
            elapsedTicksNewtonsoftJson * AcceptableDifferenceWithoutAttributesMultiplier >= elapsedTicksJsonPath,
            $"NewtonsonJson took: {elapsedTicksNewtonsoftJson} ticks\n" +
            $"JsonPath took: {elapsedTicksJsonPath} ticks");
    }

    private long GetAverageSpeed(Action action)
    {
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
            action();
        sw.Stop();

        return sw.ElapsedTicks / NumberOfTestingIterations;
    }

    private static void Warmup()
    {
        var blog = Helpers.CreateTestBlog();
        var blogWithoutAttributes = Helpers.CreateTestBlogWithoutAttributes();

        // Act
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            JsonPathConvert.SerializeObject(blog);
            JsonConvert.SerializeObject(blogWithoutAttributes);
        }
    }
}
