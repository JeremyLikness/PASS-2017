# Steps

```csharp
using System;

namespace TodoApi
{
    public static class ExceptionHelper
    {
        public static object ProcessError(Exception ex)
        {
            return new {
                error = new {
                    code = ex.HResult,
                    message = ex.Message
                }
            };
        }
    }
}
```

Add "using System" to top of TodoController
Wrap Get in try ... catch block using exceptionhelper
Force exception when todo id is 8675309

```csharp
catch(Exception ex)
{
    Context.Response.StatusCode = 400;
    return new BadRequestObjectResult(ExceptionHelper.ProcessError(ex));
}
```