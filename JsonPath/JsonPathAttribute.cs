namespace JsonPath;

public class JsonPathAttribute : Attribute
{
    public string Path;

    public JsonPathAttribute(string path)
    {
        Path = path;
    }
}
