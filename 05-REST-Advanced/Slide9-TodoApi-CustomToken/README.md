# Steps

Add constant and replace GetAll method.

```csharp
const string SECRET_HEADER = "My-Secret";

public IActionResult GetAll()
{
    if (!HttpContext.Request.Headers.ContainsKey(SECRET_HEADER) ||
        !HttpContext.Request.Headers[SECRET_HEADER].Equals("my-secret"))
        {
            return new StatusCodeResult(401);
        }
    return new ObjectResult(_context.TodoItems.AsEnumerable());
}
```
