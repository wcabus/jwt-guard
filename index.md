---
title: Home
nav_order: 1
---

# JWT Guard

JWT Guard is a free, open source, test suite written in C# for testing the security of JSON Web Token (JWT) implementations. It is designed to be used primarily by developers to test if their ASP.NET Core Web API's are properly validating JWT access tokens.

## Getting started

To get started, you'll need to first install the JWT Guard project template by using the `dotnet` CLI:

```bash
dotnet new install JWTGuard.Template
```

This allows you to use either `dotnet new jwt-guard` or use Visual Studio to add a JWT Guard test project to your existing solution. Using the `dotnet` CLI, you can add a JWT Guard test project by using the following command:

```bash
dotnet new jwt-guard --apiProject ..\\relative\\path\\to\\your\\webapi.csproj
```

Or, using the shorthand notation:

```bash
dotnet new jwt-guard -ap ..\\relative\\path\\to\\your\\webapi.csproj
```

This will add the JWT Guard test project to your solution and configure it to test the specified Web API project.
The test project will be added to the solution in its own folder, so you probably want to start the relative path to your Web API project with `../`, for example, `../MyApi/MyApi.csproj`.

## Configuring JWT Guard

Before you can run the JWT Guard test suite, you'll most likely need to update some settings and perhaps add a few lines of code to your API project. Let's step through these steps.

### 1. Configuring the target API testing endpoint

JWT Guard needs to perform its tests against a secured API endpoint to verify the functionality of the JWT Bearer token middleware used by ASP.NET Core. By default, it is configured to connect to an API at `https://localhost:5901` and to use the API endpoint `/weatherforecast`, but you can very easily override these settings by going into the `TestSettings.cs` file.

At the top of the file, you can override the default test settings in the static constructor. For example, if your local API runs on port 8888 and the target endpoint lives at `/your-secure-api-endpoint`, your configuration would look like this:

```csharp
public readonly struct TestSettings
{
    /// <summary>
    /// Static constructor for the <see cref="TestSettings"/> struct.
    /// </summary>
    static TestSettings()
    {
        // Override the default test settings here
        CurrentTestSettings = DefaultTestSettings with
        {
            Issuer = "https://localhost:8888",
            AllowedIssuers = ["https://localhost:8888"],
            TargetUrl = "/your-secure-api-endpoint"
        };
    }

    // ... the rest remains as-is.
}
```

### 2. Make your API project compatible with the JWT Guard test project

JWT Guard contains integration tests, which uses a `WebApplicationFactory` implementation to run the Web API project during each test run. For this to work correctly, the factory class needs to be able to find the Web API project's `Program` class. Newer C# projects using top-level statements, however, have no explicit `Program` class. If your project uses top-level statements, you'll need to add a partial `Program` class:

```csharp
// ... contents of your Program.cs

public partial class Program {}
```