using Newtonsoft.Json;
using System.Diagnostics;

namespace QuickJson.Tests;

public class SerializationPerformanceTests
{
    private const int NumberOfTestingIterations = 25000;
    private const int NumberOfBatches = 5;

    [Fact]
    public void Serialize_WithCustomPathsIgnoringInitialCaching_PerformsAcceptibly()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();
        var blogWithoutAttributes = Helpers.CreateTestBlogWithoutAttributes();
        _ = QuickJson.SerializeObject(blog); // Make sure type is cached

        Warmup();

        // Act
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            QuickJson.SerializeObject(blog);
        }
        sw.Stop();
        var elapsedTicksQuickJson = sw.ElapsedTicks / NumberOfTestingIterations;

        sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            JsonConvert.SerializeObject(blogWithoutAttributes);
        }
        sw.Stop();
        var elapsedTicksNewtonsoftJson = sw.ElapsedTicks / NumberOfTestingIterations;

        // Assert
        Assert.True(
            elapsedTicksNewtonsoftJson * 24 >= elapsedTicksQuickJson,
            $"NewtonsonJson took: {elapsedTicksNewtonsoftJson} ticks\n" +
            $"QuickJson took: {elapsedTicksQuickJson} ticks");
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

    [Fact]
    public void Serialize_WithCustomPaths_PerformsWell()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();

        // Act
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            QuickJson.SerializeObject(blog);
        }
        sw.Stop();

        // Assert
        Assert.Fail($"Paths Results: {sw.ElapsedMilliseconds / NumberOfTestingIterations}ms, {sw.ElapsedTicks / NumberOfTestingIterations}ticks");
    }

    [Fact]
    public void Serialize_WithoutCustomPaths_PerformsWell()
    {
        // Arrange
        var blog = Helpers.CreateTestBlogWithoutAttributes();

        // Act
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            QuickJson.SerializeObject(blog);
        }
        sw.Stop();

        // Assert
        Assert.Fail($"NoPaths Results: {sw.ElapsedMilliseconds / NumberOfTestingIterations}ms, {sw.ElapsedTicks / NumberOfTestingIterations}ticks");
    }

    [Fact]
    public void Serialize_WithNewtonsoft_Baseline()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();

        // Act
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            JsonConvert.SerializeObject(blog);
        }
        sw.Stop();

        // Assert
        Assert.Fail($"Newtonsoft Results: {sw.ElapsedMilliseconds / NumberOfTestingIterations}ms, {sw.ElapsedTicks / NumberOfTestingIterations}ticks");
    }
}
