using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickJson.Tests.TestingClasses;
using QuickJson.Tests.TestingClasses.WithoutAttributes;

namespace QuickJson.Tests;

public class SerializationTests
{
    private const string BlogId = "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee";
    private const string AuthorId = "11111111-2222-3333-4444-555555555555";

    private static readonly DateTime AuthorSponsorCreatedDate = new DateTime(2025, 4, 11, 7, 23, 31);
    private static readonly DateTime OurSponsorCreatedDate = new DateTime(2031, 5, 17, 12, 15, 5);

    [Fact]
    public void Serialize_WithoutJsonPathAttributes_DoesNotInterfareWithNewtonsoftJson()
    {
        // Arrange
        var blog = CreateTestBlogWithoutAttributes();
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
        var blog = CreateTestBlog();
        var expectedJson = await GetJsonFromTestFile("SerializationWithAttributesResult");

        // Act
        var serializationResult = QuickJson.SerializeObject(blog);

        // Assert
        Assert.Equal(RemoveFormattingAndSpaces(expectedJson), RemoveFormattingAndSpaces(serializationResult));
        Assert.True(IsJsonEqual(expectedJson, serializationResult));
    }

    [Fact]
    public async Task Serialize_WithJsonPathAttributes_SerializesNumbersWithoutQuotes()
    {
        // Arrange
        var numerics = new Numerics();
        var expectedJson = await GetJsonFromTestFile("NumericsWithAttributesResult");

        // Act
        var serializationResult = QuickJson.SerializeObject(numerics);

        // Assert
        Assert.True(IsJsonEqual(expectedJson, serializationResult));
    }

    [Fact]
    public async Task Deserialize_WithoutJsonPathAttributes_DoesNotInterfareWithNewtonsoftJson()
    {
        // Arrange
        var expectedBlog = CreateTestBlogWithoutAttributes();
        var json = await GetJsonFromTestFile("BlockSimpleSerialized");

        // Act
        var blog = QuickJson.DeserializeObject<BlogSimple>(json);

        // Assert
        Assert.NotNull(blog);
        Assert.Equal(expectedBlog.BlogId, blog.BlogId);
        Assert.Equal(expectedBlog.UserId, blog.UserId);
        Assert.Equal(expectedBlog.Username, blog.Username);
        Assert.Equal(expectedBlog.Description, blog.Description);
        AssertSponsorSimpleEquals(expectedBlog.OurSponsor, blog.OurSponsor);
        AssertSponsorSimpleEquals(expectedBlog.AuthorsSponsor, blog.AuthorsSponsor);
        Assert.Equal(expectedBlog.Posts.Count, blog.Posts.Count);
        for (var i = 0; i < blog.Posts.Count; i++)
            AssertPostSimpleEquals(expectedBlog.Posts[i], blog.Posts[i]);
    }

    [Fact]
    public async Task Deserialize_WithJsonPathAttributes_DeserializesUsingProvidedPaths()
    {
        // Arrange
        var expectedBlog = CreateTestBlog();
        var json = await GetJsonFromTestFile("BlockSerialized");

        // Act
        var blog = QuickJson.DeserializeObject<Blog>(json);

        // Assert
        Assert.NotNull(blog);
        Assert.Equal(expectedBlog.BlogId, blog.BlogId);
        Assert.Equal(expectedBlog.UserId, blog.UserId);
        Assert.Equal(expectedBlog.Username, blog.Username);
        Assert.Equal(expectedBlog.Description, blog.Description);
        AssertSponsorEquals(expectedBlog.OurSponsor, blog.OurSponsor);
        AssertSponsorEquals(expectedBlog.AuthorsSponsor, blog.AuthorsSponsor);
        Assert.Equal(expectedBlog.Posts.Count, blog.Posts.Count);
        for (var i = 0; i < blog.Posts.Count; i++)
            AssertPostEquals(expectedBlog.Posts[i], blog.Posts[i]);
    }

    private void AssertPostEquals(Post expected, Post actual)
    {
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.Comments.Count, actual.Comments.Count);
        for (var i = 0; i < expected.Comments.Count; i++)
            AssertCommentEquals(expected.Comments[i], actual.Comments[i]);
    }

    private void AssertPostSimpleEquals(PostSimple expected, PostSimple actual)
    {
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.Comments.Count, actual.Comments.Count);
        for (var i = 0; i < expected.Comments.Count; i++)
            AssertCommentSimpleEquals(expected.Comments[i], actual.Comments[i]);
    }

    private void AssertCommentEquals(Comment expected, Comment actual)
    {
        Assert.Equal(expected.Username, actual.Username);
        Assert.Equal(expected.Content, actual.Content);
    }

    private void AssertCommentSimpleEquals(CommentSimple expected, CommentSimple actual)
    {
        Assert.Equal(expected.Username, actual.Username);
        Assert.Equal(expected.Content, actual.Content);
    }

    private void AssertSponsorEquals(Sponsor expected, Sponsor actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.CreatedDate, actual.CreatedDate);
        Assert.Equal(expected.NumberOfAds, actual.NumberOfAds);
    }

    private void AssertSponsorSimpleEquals(SponsorSimple expected, SponsorSimple actual)
    {
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Description, actual.Description);
        Assert.Equal(expected.CreatedDate, actual.CreatedDate);
        Assert.Equal(expected.NumberOfAds, actual.NumberOfAds);
    }

    private static bool IsJsonEqual(string jsonA, string jsonB) =>
        JToken.DeepEquals(JObject.Parse(jsonA), JObject.Parse(jsonB));

    private static string RemoveFormattingAndSpaces(string json) =>
        json.Replace(" ", "").Replace("\n", "").Replace("\r", "");

    private static async Task<string> GetJsonFromTestFile(string filename) =>
        await File.ReadAllTextAsync($"./ExpectedSerializationResults/{filename}.json");

    private static BlogSimple CreateTestBlogWithoutAttributes()
    {
        var blog = new BlogSimple
        {
            BlogId = new Guid(BlogId),
            UserId = new Guid(AuthorId),
            Username = "JohnDoe",
            Description = "A blog about technology and programming",
            OurSponsor = new SponsorSimple
            {
                Name = "Microsoft",
                Description = "Software company making software things",
                NumberOfAds = 4,
                CreatedDate = OurSponsorCreatedDate
            },
            AuthorsSponsor = new SponsorSimple
            {
                Name = "Google",
                Description = "Software company making different software things",
                NumberOfAds = 2,
                CreatedDate = AuthorSponsorCreatedDate
            },
            Posts = new List<PostSimple>
            {
                new PostSimple
                {
                    Title = "Introduction to C#",
                    Content = "C# is a modern, object-oriented programming language...",
                    Comments = new List<CommentSimple>
                    {
                        new CommentSimple {Username = "JaneDoe", Content = "Great post!"},
                        new CommentSimple {Username = "BobSmith", Content = "Very informative."}
                    }
                },
                new PostSimple
                {
                    Title = "Advanced C# Features",
                    Content = "C# has many advanced features such as LINQ, async/await...",
                    Comments = new List<CommentSimple>
                    {
                        new CommentSimple {Username = "AliceJones", Content = "Thanks for sharing!"},
                        new CommentSimple {Username = "CharlieBrown", Content = "Can't wait to try these out."}
                    }
                }
            }
        };
        return blog;
    }

    private static Blog CreateTestBlog()
    {
        var blog = new Blog
        {
            BlogId = new Guid(BlogId),
            UserId = new Guid(AuthorId),
            Username = "JohnDoe",
            Description = "A blog about technology and programming",
            OurSponsor = new Sponsor
            {
                Name = "Microsoft",
                Description = "Software company making software things",
                NumberOfAds = 4,
                CreatedDate = OurSponsorCreatedDate
            },
            AuthorsSponsor = new Sponsor
            {
                Name = "Google",
                Description = "Software company making different software things",
                NumberOfAds = 2,
                CreatedDate = AuthorSponsorCreatedDate
            },
            Posts = new List<Post>
            {
                new Post
                {
                    Title = "Introduction to C#",
                    Content = "C# is a modern, object-oriented programming language...",
                    Comments = new List<Comment>
                    {
                        new Comment {Username = "JaneDoe", Content = "Great post!"},
                        new Comment {Username = "BobSmith", Content = "Very informative."}
                    }
                },
                new Post
                {
                    Title = "Advanced C# Features",
                    Content = "C# has many advanced features such as LINQ, async/await...",
                    Comments = new List<Comment>
                    {
                        new Comment {Username = "AliceJones", Content = "Thanks for sharing!"},
                        new Comment {Username = "CharlieBrown", Content = "Can't wait to try these out."}
                    }
                }
            }
        };
        return blog;
    }
}