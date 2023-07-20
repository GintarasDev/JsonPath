using Newtonsoft.Json;

namespace QuickJson;

internal class Program
{
    static void Main(string[] args)
    {
        var test = new TestClass
        {
            haha = "Supreme",
            Test = new TestNestedClass
            {
                bababa = "that sucks"
            }
        };
        var json = JsonConvert.SerializeObject(test);
        var result = QuickJson.DeserializeObject<TestClassToDeserialize>(json);
    }
}