using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuickJson.Tests.TestingClasses;
using QuickJson.Tests.TestingClasses.WithoutAttributes;

namespace QuickJson.Tests;

public class SerializationTests
{
    [Fact]
    public void Serialize_WithoutJsonPathAttributes_DoesNotInterfareWithNewtonsoftJson()
    {
        // Arrange
        var blog = CreateTestBlogWithoutAttributes();
        var newtonsoftResult = JsonConvert.SerializeObject(blog);

        // Act
        var result = QuickJson.SerializeObject(blog);

        // Assert
        Assert.Equal(newtonsoftResult, result);
    }

    [Fact]
    public void Serialize_WithJsonPathAttributes_FollowsProvidedPaths()
    {
        // Arrange
        var blog = CreateTestBlog();

        var flattened = JObject.FromObject(blog)
            .Descendants()
            .OfType<JValue>()
            .ToDictionary(jv => jv.Path, jv => jv.ToString());

        // Act
        var result = QuickJson.SerializeObject(blog);

        // Assert
        Assert.Equal("{\r\n  \"BlogId\": \"25609f46-3ea8-41a1-9d0c-931d272a542b\",\r\n  \"Author\": {\r\n    \"Id\": \"995c1dd6-ad73-4546-a22d-6279a9770d02\",\r\n    \"Name\": \"JohnDoe\",\r\n    \"Description\": \"A blog about technology and programming\"\r\n  }\r\n}", result);
    }

    private static BlogSimple CreateTestBlogWithoutAttributes()
    {
        var blog = new BlogSimple
        {
            BlogId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Username = "JohnDoe",
            Description = "A blog about technology and programming",
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
            BlogId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Username = "JohnDoe",
            Description = "A blog about technology and programming",
            OurSponsor = new Sponsor
            {
                Name = "Microsoft",
                Description = "Software company making software things",
                NumberOfAds = 4,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            },
            AuthorsSponsor = new Sponsor
            {
                Name = "Google",
                Description = "Software company making different software things",
                NumberOfAds = 2,
                CreatedDate = DateTime.UtcNow.AddDays(-3)
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