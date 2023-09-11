using Newtonsoft.Json;
using System.Diagnostics;

namespace QuickJson.Tests;

public class SerializationPerformanceTests
{
    private const int NumberOfTestingIterations = 25000;
    private const float AcceptableDifferenceMultiplier = 30f;
    private const float AcceptableDifferenceWithoutAttributesMultiplier = 2f;

    [Fact]
    public void Serialize_WithCustomPaths_PerformsAcceptibly()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();
        _ = QuickJson.SerializeObject(blog); // Make sure type is cached

        Warmup();
        var elapsedTicksNewtonsoftJson = GetAverageSpeed(() => JsonConvert.SerializeObject(blog));

        // Act
        var elapsedTicksQuickJson = GetAverageSpeed(() => QuickJson.SerializeObject(blog));

        // Assert
        Assert.True(
            elapsedTicksNewtonsoftJson * AcceptableDifferenceMultiplier >= elapsedTicksQuickJson,
            $"NewtonsonJson took: {elapsedTicksNewtonsoftJson} ticks\n" +
            $"QuickJson took: {elapsedTicksQuickJson} ticks");
    }

    [Fact]
    public void Serialize_WithoutCustomPaths_PerformsWell()
    {
        // Arrange
        var blog = Helpers.CreateTestBlogWithoutAttributes();
        _ = QuickJson.SerializeObject(blog); // Make sure type is cached

        Warmup();
        var elapsedTicksNewtonsoftJson = GetAverageSpeed(() => JsonConvert.SerializeObject(blog));

        // Act
        var elapsedTicksQuickJson = GetAverageSpeed(() => QuickJson.SerializeObject(blog));

        // Assert
        Assert.True(
            elapsedTicksNewtonsoftJson * AcceptableDifferenceWithoutAttributesMultiplier >= elapsedTicksQuickJson,
            $"NewtonsonJson took: {elapsedTicksNewtonsoftJson} ticks\n" +
            $"QuickJson took: {elapsedTicksQuickJson} ticks");
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
            QuickJson.SerializeObject(blog);
            JsonConvert.SerializeObject(blogWithoutAttributes);
        }
    }
}
