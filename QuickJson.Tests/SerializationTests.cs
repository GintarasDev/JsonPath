using Newtonsoft.Json;
using QuickJson.Tests.TestingClasses;

namespace QuickJson.Tests;

public class SerializationTests
{
    [Fact]
    public void Serialize_WithoutJsonPathAttributes_DoesNotInterfareWithNewtonsoftJson()
    {
        // Arrange
        var blog = Helpers.CreateTestBlogWithoutAttributes();
        var settings = JsonConvert.DefaultSettings?.Invoke();
        var newtonsoftResult = JsonConvert.SerializeObject(blog, settings);

        // Act
        var result = QuickJson.SerializeObject(blog);

        // Assert
        Assert.Equal(newtonsoftResult, result);
    }

    [Fact]
    public async Task Serialize_WithJsonPathAttributes_FollowsProvidedPaths()
    {
        // Arrange
        var blog = Helpers.CreateTestBlog();
        var expectedJson = await Helpers.GetJsonFromTestFile("SerializationWithAttributesResult");

        // Act
        var serializationResult = QuickJson.SerializeObject(blog);

        // Assert
        Assert.Equal(Helpers.RemoveFormattingAndSpaces(expectedJson), Helpers.RemoveFormattingAndSpaces(serializationResult));
        Assert.True(Helpers.IsJsonEqual(expectedJson, serializationResult));
    }

    [Fact]
    public async Task Serialize_WithJsonPathAttributes_SerializesNumbersWithoutQuotes()
    {
        // Arrange
        var numerics = new Numerics();
        var expectedJson = await Helpers.GetJsonFromTestFile("NumericsWithAttributesResult");

        // Act
        var serializationResult = QuickJson.SerializeObject(numerics);

        // Assert
        Assert.True(Helpers.IsJsonEqual(expectedJson, serializationResult));
    }
}