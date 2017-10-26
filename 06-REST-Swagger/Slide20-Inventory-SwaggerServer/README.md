# IO.Swagger - ASP.NET Core 1.0 Server

This is a simple API.

Added this code to DeveloperApi for an example inventory item:

```csharp
var sampleInventory = new InventoryItem(
                Guid.NewGuid(),
                "Sample Item",
                DateTime.UtcNow.AddDays(-1).ToString(),
                new Manufacturer(
                    "Jeremy Likness",
                    "https://blog.jeremylikness.com/",
                    "404-555-1212")
            );

            string exampleJson = $"[{sampleInventory.ToJson()}]";
```

## Run

Linux/OS X:

```bash
sh build.sh
```

Windows:

```cmd
build.bat
```

## Run in Docker

```bash
cd src/IO.Swagger
docker build -t IO.Swagger .
docker run -p 5000:5000 IO.Swagger
```
