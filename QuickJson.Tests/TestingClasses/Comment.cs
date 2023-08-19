namespace QuickJson.Tests.TestingClasses;

internal class Comment
{
    [JsonPath("Author.Name")] // This path should be relative from the current location (so in this example Username will be Posts[0].Comments[0].Author.Name
    public string Username { get; set; }
    public string Content { get; set; }
}
