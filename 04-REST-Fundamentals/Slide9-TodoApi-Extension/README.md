# Steps

```csharp
using Microsoft.Net.Http.Headers;

services.AddMvc(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", MediaTypeHeaderValue.Parse("text/xml"));
                options.FormatterMappings.SetMediaTypeMappingForFormat("json", MediaTypeHeaderValue.Parse("text/json"));
            })
                .AddXmlSerializerFormatters();
```

Add `[FormatFilter]` to controller, add `.{format?}` to get {id}
