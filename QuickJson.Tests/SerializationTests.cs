using Newtonsoft.Json;
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

        // Act
        var result = QuickJson.SerializeObject(blog);

        // Assert
        Assert.Equal("expected", result);
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
            //Posts = new List<Post>
            //{
            //    new Post
            //    {
            //        Title = "Introduction to C#",
            //        Content = "C# is a modern, object-oriented programming language...",
            //        Comments = new List<Comment>
            //        {
            //            new Comment {Username = "JaneDoe", Content = "Great post!"},
            //            new Comment {Username = "BobSmith", Content = "Very informative."}
            //        }
            //    },
            //    new Post
            //    {
            //        Title = "Advanced C# Features",
            //        Content = "C# has many advanced features such as LINQ, async/await...",
            //        Comments = new List<Comment>
            //        {
            //            new Comment {Username = "AliceJones", Content = "Thanks for sharing!"},
            //            new Comment {Username = "CharlieBrown", Content = "Can't wait to try these out."}
            //        }
            //    }
            //}
        };
        return blog;
    }
}