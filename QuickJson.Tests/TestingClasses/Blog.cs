using QuickJson.Tests.TestingClasses.WithoutAttributes;

namespace QuickJson.Tests.TestingClasses;

internal class Blog
{
    public Guid BlogId { get; set; }
    [JsonPath("Author.Id")]
    public Guid UserId { get; set; }
    [JsonPath("Author.Name")]
    public string Username { get; set; }
    [JsonPath("Author.")] // Ending with . means that we should put the field in Author object with the original name ("Description")
    public string Description { get; set; }
    [JsonPath("Articles")]
    public List<PostSimple> Posts { get; set; }
}
