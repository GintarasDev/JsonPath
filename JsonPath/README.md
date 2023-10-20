# JsonPath

JsonPath is a NewtonsoftJson wrapper that adds `JsonPath` attribute. This attribute allows modifications of the paths of output json.

Example usage:
```csharp
public class User
{
	public string Username
}
```

Only supports properties (no fields support)
Dot is used as path separator so your json key cannot have it in the name
# TODO:
  * Implement non-generic IEnumerable support (?)
  * Implement a way to define JsonPath as a serializer to use for requests and responses
  * Implement intelisense to warn about incorrect paths
  * Implement way to exclude all classes in specific namespace and its subnamespaces (usefull to not check built in classes)
  * Add intelisense to warn about not supported non-primitive dictionary keys