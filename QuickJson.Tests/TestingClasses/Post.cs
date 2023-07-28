using QuickJson.Tests.TestingClasses.WithoutAttributes;

namespace QuickJson.Tests.TestingClasses;

internal class Post
{
    public string Title { get; set; }
    public string Content { get; set; }
    public List<Comment> Comments { get; set; }
}
