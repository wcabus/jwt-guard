# ![JWT Guard logo](logo-75.png "JWT Guard logo") JWT Guard

JWT Guard is a free, open source, test suite written in C# for testing the security of JSON Web Token (JWT) implementations. 
It is designed to be used primarily by developers to test if their ASP.NET Core Web API's are properly validating JWT access tokens.

## Installation

JWT Guard is a .NET Test project that can be added to your solution. It is available as a NuGet package which can be installed using the following command:

```bash
dotnet new install JWTGuard.Template
```

## Usage

To add JWT Guard to your solution, you can use the following command:

```bash
dotnet new jwt-guard --apiProject <relative-path-to-web-api-project>
```

Or, using the shorthand notation
```bash
dotnet new jwt-guard -ap <relative-path-to-web-api-project>
```

This will add the JWT Guard test project to your solution and configure it to test the specified Web API project.
The test project will be added to the solution in its own folder, so you probably want to start the relative path to your Web API project with `../`, for example, `../MyApi/MyApi.csproj`.

For further documentation, visit https://jwtguard.net