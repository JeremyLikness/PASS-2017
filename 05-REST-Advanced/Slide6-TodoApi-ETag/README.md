# Steps

```csharp
using TodoApi.Models;
using System.Security.Cryptography;
using System.Text;

namespace TodoApi
{
    public static class HashFactory
    {
        public static string GetHash(TodoItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            var itemText = $"{item.Id}|{item.IsComplete}|{item.Name}";
            using (var md5 = MD5.Create())
            {
                byte[] retVal = md5.ComputeHash(Encoding.Unicode.GetBytes(itemText));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
```

```csharp
const string ETAG_HEADER = "ETag";
const string MATCH_HEADER = "If-Match";
```

Get:

```csharp
var eTag = HashFactory.GetHash(item);
HttpContext.Response.Headers.Add(ETAG_HEADER, eTag);

if (HttpContext.Request.Headers.ContainsKey(MATCH_HEADER) &&
    HttpContext.Request.Headers[MATCH_HEADER].Contains(eTag))
{
    return new StatusCodeResult(304);
}
return new ObjectResult(item);
```

Put:

```csharp
 var dbTag = HashFactory.GetHash(todo);
if (!HttpContext.Request.Headers.ContainsKey(MATCH_HEADER) ||
    !HttpContext.Request.Headers[MATCH_HEADER].Contains(dbTag))
    {
        return new StatusCodeResult(412);
    }
```
