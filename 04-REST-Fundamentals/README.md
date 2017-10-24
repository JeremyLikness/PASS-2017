# Steps

## Content negotiation

`dotnet add package Microsoft.AspNetCore.Mvc.Formatters.Xml`

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

Add `[FormatFilter]` then part of route, and `.{format?}` on get {id}

## Custom formatter

```csharp
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using TodoApi.Models;

namespace TodoApi
{
    public class CsvFormatter : TextOutputFormatter
    {
        public CsvFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));
            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        protected override bool CanWriteType(System.Type type)
        {
            return type == typeof(TodoItem);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var response = context.HttpContext.Response;
            var buffer = new StringBuilder();
            if (context.Object is IEnumerable<TodoItem>)
            {
                foreach(var todoItem in (IEnumerable<TodoItem>)context.Object)
                {
                    FormatCsv(buffer, todoItem);
                }
            }
            else
            {
                FormatCsv(buffer, (TodoItem)context.Object);
            }
            using (var writer = context.WriterFactory(response.Body, selectedEncoding))
            {
                return writer.WriteAsync(buffer.ToString());
            }
        }

        private static void FormatCsv(StringBuilder buffer, TodoItem item)
        {
            buffer.AppendLine($"{item.Id},\"{item.Name}\",{item.IsComplete}");
        }
    }
}
```

```csharp
options.FormatterMappings.SetMediaTypeMappingForFormat("csv", MediaTypeHeaderValue.Parse("text/csv"));
                options.OutputFormatters.Add(new CsvFormatter());
```

## HATEOAS

```csharp
using System.Collections.Generic;

namespace TodoApi
{
    public class Link
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string method { get; set; }
    }

    public class LinkHelper<T> where T: class 
    {
        public T Value { get; set; }
        public List<Link> Links { get; set;}

        public LinkHelper()
        {
            Links = new List<Link>();
        }

        public LinkHelper(T item) : base()
        {
            Value = item;
            Links = new List<Link>();
        }
    }
}
```

`[FromHeader(Name="Accept")]string accept`

Add names to PUT, DELETE

```csharp
if (accept.EndsWith("hateoas"))
            {
                var link = new LinkHelper<TodoItem>(item);
                link.Links.Add(new Link {
                    Href = Url.Link("GetTodo", new { item.Id }),
                    Rel = "self",
                    method = "GET"
                });
                link.Links.Add(new Link {
                    Href = Url.Link("PutTodo", new { item.Id }),
                    Rel = "put-todo",
                    method = "PUT"
                });
                link.Links.Add(new Link {
                    Href = Url.Link("DeleteTodo", new { item.Id }),
                    Rel = "delete-todo",
                    method = "DELETE"
                });
                return new ObjectResult(link);
            }
```

## Exceptions

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

## ETag

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

## Custom Token

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

## JWT

`dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer`

`dotnet add package System.IdentityModel.Tokens.Jwt`

Startup:

```csharp
public const string AES256KEY = @"0123456789ABCDEF";

services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "JwtBearer";
    options.DefaultChallengeScheme = "JwtBearer";
})
.AddJwtBearer("JwtBearer", jwtBearerOptions =>
{
    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(AES256KEY)),

        ValidateIssuer = true,
        ValidIssuer = "http://localhost:5001",

        ValidateAudience = true,
        ValidAudience = "http://localhost:5000",

        ValidateLifetime = true, //validate the expiration and not before values in the token

        ClockSkew = System.TimeSpan.FromMinutes(5) //5 minute tolerance for the expiration date
    };
});

app.UseAuthentication();
```

ValuesController:

```csharp
[HttpGet]
public IActionResult GetToken()
{
    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, "jeremy@jeremylikness.com"),
        new Claim(JwtRegisteredClaimNames.Jti, System.Guid.NewGuid().ToString()),
    };

    var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Startup.AES256KEY));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken("http://localhost:5001",
        "http://localhost:5000",
        claims,
        expires: System.DateTime.Now.AddMinutes(30),
        signingCredentials: creds);

    var tokenEncoded = new JwtSecurityTokenHandler().WriteToken(token);

    return new OkObjectResult(new { token = tokenEncoded });
}
```

Decode it on [jwt.io](http://jwt.io).

Add `[Authorize]` to method on controller. 

## Swagger

`dotnet add package Swashbuckle.AspNetCore`

`StartUp.cs -> ConfigureServices`

`using Swashbuckle.AspNetCore.Swagger;`

```csharp
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
    });
```

```csharp
public void Configure(IApplicationBuilder app)
{
    // Enable middleware to serve generated Swagger as a JSON endpoint.
    app.UseSwagger();

    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

    app.UseMvc();
}
```

localhost ... /swagger
