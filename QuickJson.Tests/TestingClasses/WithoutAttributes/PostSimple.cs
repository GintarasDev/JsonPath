namespace QuickJson.Tests.TestingClasses.WithoutAttributes;

internal class PostSimple
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public DateTime? CreatedDate { get; set; }
    public List<CommentSimple>? Comments { get; set; }
}
