using JsonPath;

namespace JsonPath.Tests.TestingClasses;

internal class Sponsor
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int NumberOfAds { get; set; }

    [JsonPath("Metadata.SponsorSince")]
    public DateTime CreatedDate { get; set; }
}
