# Steps

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

app.UseAuthentication(); // before UseMvc()
```

TodoController:

```csharp
[Route("GetToken")]
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

Authorization -> Bearer (token)