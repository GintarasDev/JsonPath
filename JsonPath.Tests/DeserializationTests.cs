using JsonPath.Tests.TestingClasses;
using JsonPath.Tests.TestingClasses.WithoutAttributes;

namespace JsonPath.Tests;

public class DeerializationTests
{
    [Fact]
    public async Task Deserialize_WithoutJsonPathAttributes_DoesNotInterfareWithNewtonsoftJson()
    {
        // Arrange
        var expectedBlog = Helpers.CreateTestBlogWithoutAttributes();
        var json = await Helpers.GetJsonFromTestFile("BlockSimpleSerialized");

        // Act
        var blog = JsonPathConvert.DeserializeObject<BlogSimple>(json);

        // Assert
        Assert.NotNull(blog);
        Assert.Equal(expectedBlog.BlogId, blog.BlogId);
        Assert.Equal(expectedBlog.UserId, blog.UserId);
        Assert.Equal(expectedBlog.Username, blog.Username);
        Assert.Equal(expectedBlog.Description, blog.Description);
        Helpers.AssertSponsorSimpleEquals(expectedBlog.OurSponsor!, blog?.OurSponsor);
        Helpers.AssertSponsorSimpleEquals(expectedBlog.AuthorsSponsor!, blog?.AuthorsSponsor);
        Assert.Equal(expectedBlog.Posts!.Count, blog?.Posts?.Count);
        for (var i = 0; i < blog?.Posts?.Count; i++)
            Helpers.AssertPostSimpleEquals(expectedBlog.Posts[i], blog.Posts[i]);
    }

    [Fact]
    public async Task Deserialize_WithJsonPathAttributes_DeserializesUsingProvidedPaths()
    {
        // Arrange
        var expectedBlog = Helpers.CreateTestBlog();
        var json = await Helpers.GetJsonFromTestFile("BlockSerialized");

        // Act
        var blog = JsonPathConvert.DeserializeObject<Blog>(json);

        // Assert
        Assert.NotNull(blog);
        Assert.Equal(expectedBlog.BlogId, blog.BlogId);
        Assert.Equal(expectedBlog.UserId, blog.UserId);
        Assert.Equal(expectedBlog.Username, blog.Username);
        Assert.Equal(expectedBlog.Description, blog.Description);
        Helpers.AssertSponsorEquals(expectedBlog.OurSponsor!, blog.OurSponsor);
        Helpers.AssertSponsorEquals(expectedBlog.AuthorsSponsor!, blog.AuthorsSponsor);
        Assert.Equal(expectedBlog.Posts!.Count, blog.Posts?.Count);
        for (var i = 0; i < blog.Posts?.Count; i++)
            Helpers.AssertPostEquals(expectedBlog.Posts[i], blog.Posts[i]);
    }
}