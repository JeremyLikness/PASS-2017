# Steps

On Get, add version parameter as string and code:

```csharp
if (version == "v2.0")
                {
                    return new ObjectResult(
                        new {
                            item.Id,
                            Description = item.Name,
                            State = item.IsComplete ? "Completed" : "Pending"
                        }
                    );
                }
```