using Newtonsoft.Json;

namespace QuickJson.Tests;

public class EnumerableSerializationTests
{
    [Fact]
    public void Serialize_WithDictionariesUsingAttributes_SerializesAsExpected()
    {
        // Arrange
        var enumerablesTest = new EnumerablesTest();
        var newtonsoftResult = JsonConvert.SerializeObject(enumerablesTest);

        // Act
        var result = QuickJson.SerializeObject(enumerablesTest);

        // Assert
        Assert.Equal(newtonsoftResult, result);
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

    //[JsonPath("Superb.")]
    //public Dictionary<TestKey, TestValue> DictionaryWithObjectsAsKeys { get; set; } = new()
    //{
    //    [new TestKey { SomeOtherId = "0 Super Key 0" }] = new TestValue { Greeting = "Good evening sir", Number = 75 },
    //    [new TestKey { SomeOtherId = "1 Super Key 1" }] = new TestValue { Greeting = "Oh hi", Number = 4582 },
    //    [new TestKey { SomeOtherId = "2 Super Key 2" }] = new TestValue { Greeting = "Long time no see!", Number = -175 }
    //};
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

public class TestKey
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SomeOtherId { get; set; } = "How are you?";
}