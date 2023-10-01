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

        // TODO: fix nesting combinations of dictionaries and arrays
        // Maybe just use different separators for arrays and dictionaries (eg. arrays would use [#] instead of [*]
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
        ["TestKey0"] = new TestValue("Name", "Crocodyle") { Greeting = "Morning!", Number = 68 },
        ["TestKey1"] = new TestValue("Name", "Chicken") { Greeting = "Hi", Number = 1 },
        ["TestKey2"] = new TestValue("Shape", "Star") { Greeting = "Sup", Number = 85146 }
    };

    [JsonPath("Integers.Keys")]
    public Dictionary<int, AnotherTestValue> IntValuesDictionary { get; set; } = new()
    {
        [3] = new AnotherTestValue("00000000-1111-1111-1111-222222222222", "Lama", 15, "Zebra", 54, "Fruit", "apple", "milkType", "Almond"),
        [125] = new AnotherTestValue("aaaaaaaa-8888-9999-9999-eeeeeeeeeeee", "Dog", 99, "Mamoth", 157, "vegetable", "Carrot!", "Tea", "Green"),
        [-879] = new AnotherTestValue("ffffcccc-5555-3335-7777-775599ddee88", "Cat", 5110, "Rat", -457, "Algea", "WateredAndAlive", "Food", "FermentedSoybeans")
    };
}

public class TestValue
{
    public TestValue(string key, string value)
    {
        NestedDictionary = new()
        {
            ["Hi0"] = "Armadilo",
            [key] = value,
            ["Hi2"] = "Aligator"
        };
    }

    public int Number { get; set; } = 451;
    [JsonPath("Personalization.Localization.")]
    public string Greeting { get; set; } = "Hello";

    [JsonPath("Nested.Values")]
    public Dictionary<string, string> NestedDictionary { get; set; }
}

public class AnotherTestValue
{
    public AnotherTestValue(
        string id,
        string greeting0, int number0,
        string greeting1, int number1,
        string key01, string value01,
        string key10, string value10)
    {
        Id = new Guid(id);
        TestValues = new List<TestValue>
        {
            new("Food", "Soybeans")
            {
                Greeting = greeting0,
                Number = number0,
                NestedDictionary = new()
                {
                    ["Test"] = "Shark",
                    [key01] = value01,
                    ["Morning"] = "Hippo"
                }
            },
            new("Mushrooms", "Champagnions")
            {
                Greeting = greeting1,
                Number = number1,
                NestedDictionary = new()
                {
                    [key10] = value10,
                    ["Tromb"] = "Girafe",
                    ["Morning"] = "Dolphin"
                }
            },
        };
    }

    public Guid Id { get; set; } = new Guid("aaaaaaaa-bbbb-cccc-dddd-ffffffffffff");
    public List<TestValue> TestValues { get; set; }

    [JsonPath("Multiple.Nests.")]
    public Dictionary<string, string>[] ArrayOfStringsDictionaries { get; set; } = new Dictionary<string, string>[]
    {
        new ()
        {
            ["Test0"] = "I",
            ["Nest0"] = "Am",
            ["Fest0"] = "Groot"
        },
        new ()
        {
            ["Kilo"] = "620",
            ["Mega"] = "571",
            ["Giga"] = "951"
        },
        new ()
        {
            ["Phone"] = "Xiaomi Mi",
            ["OtherPhone"] = "Samsung Galaxy",
            ["ThirdPhone"] = "Apple iPhone"
        }
    };
}