using Newtonsoft.Json;

namespace QuickJson.Tests;

public class EnumerableSerializationTests
{
    [Fact]
    public async Task Serialize_WithDictionariesUsingAttributes_SerializesAsExpected()
    {
        // Arrange
        var enumerablesTest = new EnumerablesTest();
        var expectedJson = await Helpers.GetJsonFromTestFile("EnumerablesWithAttributesResult");

        // Act
        var serializationResult = QuickJson.SerializeObject(enumerablesTest);

        // Assert
        Assert.Equal(Helpers.RemoveFormattingAndSpaces(expectedJson), Helpers.RemoveFormattingAndSpaces(serializationResult));
        Assert.True(Helpers.IsJsonEqual(expectedJson, serializationResult));
    }
}

public class EnumerablesTest
{
    [JsonPath("Amazingness")]
    public Dictionary<string, string> StringsDictionary { get; set; } = new()
    {
        ["Key0"] = "Hello",
        ["Key1"] = "Super",
        ["Key2"] = "Great"
    };

    [JsonPath("Superb.Performance.Values")]
    public Dictionary<string, TestValue> TestValuesDictionary { get; set; } = new()
    {
        ["Key0"] = new TestValue { Greeting = "Morning!", Number = 68 },
        ["Key1"] = new TestValue { Greeting = "Hi", Number = 1 },
        ["Key2"] = new TestValue { Greeting = "Sup", Number = 85146 }
    };

    [JsonPath("Integers.Keys")]
    public Dictionary<int, AnotherTestValue> IntValuesDictionary { get; set; } = new()
    {
        [541] = new AnotherTestValue(),
        [5] = new AnotherTestValue(),
        [81] = new AnotherTestValue()
    };
}

public class TestValue
{
    public int Number { get; set; } = 451;
    [JsonPath("Personalization.Localization.")]
    public string Greeting { get; set; } = "Hello";

    [JsonPath("Nested.Values")]
    public Dictionary<string, string> NestedDictionary { get; set; } = new()
    {
        ["Hi0"] = "Armadilo",
        ["Hi1"] = "Crocodile",
        ["Hi2"] = "Aligator"
    };
}

public class AnotherTestValue
{
    public Guid Id { get; set; } = new Guid("aaaaaaaa-bbbb-cccc-dddd-ffffffffffff");
    public List<TestValue> TestValues { get; set; } = new List<TestValue>
    {
        new()
        {
            Greeting = "Lama",
            Number = 781,
            NestedDictionary = new()
            {
                ["Test"] = "Shark",
                ["What"] = "Horse",
                ["Morning"] = "Hippo"
            }
        },
        new()
        {
            Greeting = "Zebra",
            Number = 9951,
            NestedDictionary = new()
            {
                ["Almond"] = "Rhino",
                ["Tromb"] = "Girafe",
                ["Morning"] = "Dolphin"
            }
        },
    };
}