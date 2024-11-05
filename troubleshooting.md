---
title: Troubleshooting
nav_order: 3
layout: page
permalink: /troubleshooting
---

# Troubleshooting

If you add JWT Guard to your solution and keep getting compiler errors or the tests fail to run, keep on reading.
We may yet have a solution on this page to get you up and running in no time!

## Compiler errors

### The referenced project '..\path\to\ApiProject.csproj' does not exist.

You're seeing one of the following compiler errors:

```gitignore
Unable to find project '\path\to\ApiProject.csproj'. Check that the project reference is valid and that the project file exists.

# or

The referenced project '..\path\to\ApiProject.csproj' does not exist.
```

{: .important-title }
> The solution
> 
> Check that the added JWT Guard project has the correct reference to your Web API project.

The easiest way to fix this issue, is to open the JWT Guard project's `.csproj` file in a text editor and remove the lines that look like this:
```msbuild
<ItemGroup>
    <ProjectReference Include="..\path\to\ApiProject.csproj" />
</ItemGroup>
```
Then, reload the solution in Visual Studio, Rider or another IDE, and add your Web API project to the JWT Guard project
as a reference again.


#### Visual Studio instructions

In Visual Studio, you can simply drag your Web API project in the Solution Explorer window and drop it on the
JWT Guard project node to add the missing project reference. 

You can also right-click the JWT Guard project and
choose the context menu item _Add > Project Reference..._, followed by selecting your Web API project.
Click the OK button to confirm.


#### Rider instructions

In Rider, right-click on the JWT Guard project node and choose the context menu item _Add > Reference..._. 
In the dialog, tick the checkbox next to your Web API project and click the Add button to confirm.

### The type or namespace 'JwtBearer' does not exist in the namespace 'Microsoft.AspNetCore.Authentication' (are you missing an assembly reference?)

You're seeing the following compiler error:

```gitignore
The type or namespace 'JwtBearer' does not exist in the namespace 'Microsoft.AspNetCore.Authentication' (are you missing an assembly reference?)
```

{: .important-title }
> The solution
> 
> Your Web API project is not (yet) configured to use JWT Bearer tokens or is not using the NuGet package 
> `Microsoft.AspNetCore.Authentication.JwtBearer`
 
To fix this issue, add the NuGet package `Microsoft.AspNetCore.Authentication.JwtBearer` to your Web API project and 
modify your API configuration to enable JWT authentication. You can find some guidance [here for Minimal API projects](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/security?view=aspnetcore-8.0#enabling-authentication-in-minimal-apps).
Adding the necessary services and configuration is exactly the same for controller-based API projects.

If you are using another method to add JWT support to your Web API, then you'll need to modify the method `ConfigureWebHost`
in the file `Helpers\TargetApiWebApplicationFactory.cs`. This method reconfigures the API under test so that it trusts
the tokens issued by JWT Guard when running its tests.

### 'Program' is inaccessible due to its protection level

You're seeing the following compiler error:

```gitignore
'Program' is inaccessible due to its protection level
```

{: .important-title }
> The solution
>
> Your Web API project is either using top-level statements and has no actual Program class, or your Web API project's
> Program class has the `internal` access modifier. At the bottom of your Web API's `Program.cs` file, add the line 
> `public partial class Program {}` if you're using top-level statements, or change the access modifier of your existing 
> `Program` class to `public`.

JWT Guard uses xUnit, which requires test classes to be publicly visible, and JWT Guard's tests are integration tests
which use a `WebApplicationFactory` to be able to interact with your Web API project. Long story short, this means that
JWT Guard needs public access to your Web APIs `Program` class.

## Test errors

### All the test cases that expect an `Unauthorized` result failed. The actual result is something other than "500 Internal Server Error"

When you run the JWT Guard tests, every test which uses an invalid token and expects to receive an `Unauthorized` response 
from your Web API, has instead received one of the following actual results:
- 200 OK
- 201 Created
- 204 No Content
- 400 Bad Request
- 404 Not Found
- (there could be others depending on the results used in your Web API)

{: .important-title }
> The solution
>
> There could be multiple solutions:
> 1. If the tests return a `404 Not Found` response in every case, then check if you have overridden the current settings in `TestSettings.cs` to set the correct `TargetUrl`, `DefaultAudience` and `AllowedAudiences` for your Web API.
> 2. Your `TargetUrl` can be accessed anonymously. Either add authorization to the API endpoint, or use another endpoint as your `TargetUrl`.
> 3. Your Web API is not yet configured to validate incoming JWT tokens. Check your API configuration.

### All the test cases that expect an `Unauthorized` result failed. The actual result is "500 Internal Server Error"

When you run the JWT Guard tests, every test which uses an invalid token and expects to receive an `Unauthorized` response
from your Web API, has instead received an Internal Server Error response.

{: .important-title }
> The solution
>
> There could be multiple solutions:
> 1. Your Web API is not yet configured to validate incoming JWT tokens. Check your API configuration.
> 2. Your `TargetUrl` is throwing an unhandled exception, causing the API to return a 500 Internal Server Error.

### Some of the test cases fail

When you run the JWT Guard tests, most tests are green except a few. You'll typically see the following tests fail:

- One or more tests in the class `JwtTypeTests`
- One or more tests in the class `SignatureAlgorithmTests`

{: .important-title }
> The solution
>
> JWT Guard deliberately uses a specific test configuration that causes default setups to fail some test cases, to make 
> you aware of both your specific test configuration and how your Web API has been configured.
> 
> You'll need to either:
> - modify your current test settings in `TestSettings.cs` to match your API configuration, changing the valid or invalid token types and allowed/disallowed signature algorithms;
> - adapt your Web API configuration to fix the failing tests, by changing the JWT Bearer configuration to allow only specific values for the JWT "typ" claim and/or limit the valid signature algorithms; 
> - or a combination of both. 

To change your test settings, head into the `TestSettings.cs` file. The easiest way to update the settings, is to override the current settings in the static constructor:

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
            TargetUrl = "/your-secure-api-endpoint",
            DefaultAudience = "your-api-audience",
            AllowedAudiences = ["your-api-audience"],
            ValidTokenTypes = ["jwt", "at+jwt"],
            InvalidTokenTypes = ["none"],
            AllowedAlgorithms = [ "ES256" ],
            DisallowedAlgorithms = [ "RS256", "PS256", /* and the list goes on... */, "HS256" ]
        };
    }
    
    // ... omitted
}
```

{: .note }
Regarding the list of allowed and disallowed signature algorithms, it's probably easier to simply change the properties `AllowedAlgorithms` and `DisallowedAlgorithms`, because of their contents.

If you want to update the configuration of the JWT Bearer authentication handler in your Web API, 
head over to the line `AddJwtBearer` in your `Program.cs` file (or wherever you're configuring authentication),
and update the configuration.

This example will configure your API to:
- only allow tokens with "at+jwt" for their "typ" claim.
- only allow the signature algorithms ES256, ES384, ES512, PS256, PS384 and PS512.
- only allow tokens from a specific issuer. This setting, however, will be overridden by JWT Guard during the test run.
- require tokens to have an audience.
- require tokens to have an expiration time.
- require tokens to have a valid signature.
- validate the audience against the configured one. The configured audience, however, will be overridden by JWT Guard during the test run.
- validate the issuer against the configured one. The configured issuer, however, will be overridden by JWT Guard during the test run.
- validate the token's lifetime.

{: .note }
While some of these settings are in fact the defaults, it's not a bad practice to be verbose when configuring security settings.

```csharp
builder.Services
    .AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.Authority = "https://your-token-service.com";
        options.Audience = "your-api-audience";
        
        options.TokenValidationParameters = new()
        {
            ValidAlgorithms = [
                SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512,
                SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512
            ],
            ValidIssuer = "https://your-token-service.com",
            ValidTypes = ["at+jwt"],

            RequireAudience = true,
            RequireExpirationTime = true,
            RequireSignedTokens = true,

            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true
        };
    });
```