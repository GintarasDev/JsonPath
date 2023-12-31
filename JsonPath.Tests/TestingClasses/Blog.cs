﻿namespace JsonPath.Tests.TestingClasses;

internal class Blog
{
    public Guid BlogId { get; set; }
    [JsonPath("Author.Id")]
    public Guid UserId { get; set; }
    [JsonPath("Author.Name")]
    public string? Username { get; set; }
    [JsonPath("Author.")]
    public string? Description { get; set; }

    [JsonPath("Sponsor")]
    public Sponsor? OurSponsor { get; set; }
    [JsonPath("Author.Sponsor")]
    public Sponsor? AuthorsSponsor { get; set; }

    [JsonPath("Articles")]
    public List<Post>? Posts { get; set; }
}
