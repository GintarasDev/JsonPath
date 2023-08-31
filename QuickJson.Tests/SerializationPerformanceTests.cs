using Newtonsoft.Json;
using System.Diagnostics;

namespace QuickJson.Tests;

public class SerializationPerformanceTests
{
    private const int NumberOfTestingIterations = 20000;

    [Fact]
    public void Serialize_WithCustomPathsResetingCache_PerformsWell()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();

        // Act
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < NumberOfTestingIterations; i++)
        {
            QuickJson.ClearCache();
            QuickJson.SerializeObject(blog);
        }
        sw.Stop();

        // Assert
        Assert.Fail($"Paths Results: {sw.ElapsedMilliseconds / NumberOfTestingIterations}ms, {sw.ElapsedTicks / NumberOfTestingIterations}ticks");
    }

    [Fact]
    public void Serialize_WithCustomPathsIgnoringInitialCaching_PerformsWell()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();
        QuickJson.SerializeObject(blog); // Make sure type is cached

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
