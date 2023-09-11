namespace QuickJson.Tests.TestingClasses.WithoutAttributes;

internal class BlogSimple
{
    public Guid BlogId { get; set; }
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public string? Description { get; set; }
    public SponsorSimple? OurSponsor { get; set; }
    public SponsorSimple? AuthorsSponsor { get; set; }
    public List<PostSimple>? Posts { get; set; }
}
