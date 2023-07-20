namespace QuickJson;

internal class TestClassToDeserialize
{
    public string haha { get; set; }
    [JsonPath("Test.bababa")]
    public string bababa { get; set; }
}

internal class TestClass
{
    public string haha { get; set; }
    public TestNestedClass Test { get; set; }
}

internal class TestNestedClass
{
    public string bababa { get; set; }
}
