﻿using JsonPath;

namespace JsonPath.Tests.TestingClasses;

internal class Comment
{
    [JsonPath("Author.Name")]
    public string? Username { get; set; }
    public string? Content { get; set; }
}
