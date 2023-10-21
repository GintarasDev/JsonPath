# JsonPath

JsonPath is a NewtonsoftJson decorator that adds `JsonPath` attribute.

The `JsonPath` attribute takes a string representing the path you want a value to be serialized to and deserialized from. In this path '.' (dot character) represents nesting.

## Usage

Lets say we have a record as follows:
```csharp
public record PullRequestAction(
   [JsonPath("update.")] string State,
   [JsonPath("update.author.display_name")] string AuthorName,
   [JsonPath("update.author.account_id")] string AuthorAccountId,
   [JsonPath("update.author.links.self.href")] string AuthorSelfHref,
   [JsonPath("pull_request.title")] string PullRequestTitle,
   [JsonPath("pull_request.id")] int PullRequestId);
```
You can serialize this class to Json using `JsonPathConvert`:
```csharp
var res = JsonPathConvert.DeserializeObject<PullRequestAction>(json);
```
And you can deserialize json into this class as follows:
```csharp
var json = JsonPathConvert.SerializeObject<PullRequestAction>(pullRequestAction);
```
This example class would be serialized to (and can be deserialized from) this json structure:
```json
{
   "update": {
       "state": "OPEN",
       "author": {
           "display_name": "Name Lastname",
           "links": {
               "self": {
                   "href": "https://api.bitbucket.org/2.0/users/%7B%7D"
               },
           },
           "account_id": ""
       },
   },
   "pull_request": {
       "title": "username/NONE: small change from onFocus to onClick to handle tabbing through the page and not expand the editor unless a click event triggers it",
       "id": 5695
   }
}
```

Note: If the path passed to `JsonPath` attribute ends with '.' - the current name of the property will be used in json. Otherwise - the last path part is considered as a name of the property.

## Performance

This package is quite slow at the moment. It might be ok for light use in non performance sensitive API, but in general I would suggest waiting for performance oriented updates if performance is a concern.
```

BenchmarkDotNet v0.13.9+228a464e8be6c580ad9408e98f18813f6407fb5a, Windows 11 (10.0.22621.2428/22H2/2022Update/SunValley2)
AMD Ryzen 7 2700X, 1 CPU, 16 logical and 8 physical cores
.NET SDK 7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2


```
| Method                       | Mean       | Error     | StdDev    | Ratio | RatioSD |
|----------------------------- |-----------:|----------:|----------:|------:|--------:|
| Serialize                    | 217.035 μs | 1.3642 μs | 1.2094 μs | 25.18 |    0.29 |
| Serialize_WithoutCustomPaths |   8.625 μs | 0.1079 μs | 0.0901 μs |  1.00 |    0.02 |
| Serialize_WithPureNewtonsoft |   8.619 μs | 0.1086 μs | 0.0963 μs |  1.00 |    0.00 |

## Limitations

Currently this package is limited to:
* Only supports public properties (no fields support)
* Dot is used as path separator so your json key cannot have it in the name

## Planed updates

The following features are planned in the near future:
* Add fields support
* Optimizations
* Implement intelisense to warn about incorrect paths
* Implement way to exclude all classes in specific namespace and its subnamespaces (usefull to not check built in classes)
* Add intelisense to warn about not supported non-primitive dictionary keys