using Newtonsoft.Json.Linq;
using QuickJson.Tests.TestingClasses;
using QuickJson.Tests.TestingClasses.WithoutAttributes;

namespace QuickJson.Tests;

internal static class Helpers
{
    internal static readonly Guid BlogId = new("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    internal static readonly Guid AuthorId = new("11111111-2222-3333-4444-555555555555");

    internal static readonly DateTime AuthorSponsorCreatedDate = new DateTime(2025, 4, 11, 7, 23, 31);
    internal static readonly DateTime OurSponsorCreatedDate = new DateTime(2031, 5, 17, 12, 15, 5);
    internal static readonly DateTime PostCreatedDate0 = new DateTime(2045, 3, 12, 9, 14, 7);
    internal static readonly DateTime PostCreatedDate1 = new DateTime(2053, 4, 9, 7, 4, 1);

    internal static string RemoveFormattingAndSpaces(string json) =>
        json.Replace(" ", "").Replace("\n", "").Replace("\r", "");

    internal static bool IsJsonEqual(string jsonA, string jsonB) =>
        JToken.DeepEquals(JObject.Parse(jsonA), JObject.Parse(jsonB));

    internal static void AssertPostEquals(Post expected, Post actual)
    {
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.Comments?.Count, actual.Comments?.Count);
        for (var i = 0; i < expected.Comments?.Count; i++)
            AssertCommentEquals(expected.Comments[i], actual.Comments?[i]);
    }

    internal static void AssertPostSimpleEquals(PostSimple expected, PostSimple actual)
    {
        Assert.Equal(expected.Title, actual.Title);
        Assert.Equal(expected.Content, actual.Content);
        Assert.Equal(expected.Comments?.Count, actual.Comments?.Count);
        for (var i = 0; i < expected.Comments?.Count; i++)
            AssertCommentSimpleEquals(expected.Comments[i], actual.Comments?[i]);
    }

    internal static void AssertSponsorEquals(Sponsor expected, Sponsor? actual)
    {
        Assert.Equal(expected.Name, actual?.Name);
        Assert.Equal(expected.Description, actual?.Description);
        Assert.Equal(expected.CreatedDate, actual?.CreatedDate);
        Assert.Equal(expected.NumberOfAds, actual?.NumberOfAds);
    }

    internal static void AssertSponsorSimpleEquals(SponsorSimple expected, SponsorSimple? actual)
    {
        Assert.Equal(expected.Name, actual?.Name);
        Assert.Equal(expected.Description, actual?.Description);
        Assert.Equal(expected.CreatedDate, actual?.CreatedDate);
        Assert.Equal(expected.NumberOfAds, actual?.NumberOfAds);
    }

    internal static BlogSimple CreateTestBlogWithoutAttributes()
    {
        var blog = new BlogSimple
        {
            BlogId = BlogId,
            UserId = AuthorId,
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
                    CreatedDate = PostCreatedDate0,
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
                    CreatedDate = PostCreatedDate1,
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

    internal static Blog CreateTestBlog()
    {
        var blog = new Blog
        {
            BlogId = BlogId,
            UserId = AuthorId,
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
                    CreatedDate = PostCreatedDate0,
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
                    CreatedDate = PostCreatedDate1,
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

    internal static async Task<string> GetJsonFromTestFile(string filename) =>
        await File.ReadAllTextAsync($"./ExpectedSerializationResults/{filename}.json");

    private static void AssertCommentEquals(Comment expected, Comment? actual)
    {
        Assert.Equal(expected.Username, actual?.Username);
        Assert.Equal(expected.Content, actual?.Content);
    }

    private static void AssertCommentSimpleEquals(CommentSimple expected, CommentSimple? actual)
    {
        Assert.Equal(expected.Username, actual?.Username);
        Assert.Equal(expected.Content, actual?.Content);
    }
}
