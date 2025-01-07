---
title: Test Settings
nav_order: 2
layout: page
permalink: /test-settings
---

# Test Settings

Using the `TestSettings.cs` file, you can configure the following aspects within the JWT Guard test suite:

1. [The target URL used to run every test against.](#target-url)
2. [The default assertions to verify authorized and unauthorized HTTP responses](#default-assertions)
3. [The default test user used to generate an access token for.](#default-test-user)
4. [The default, allowed and disallowed audiences.](#audience)
5. [The default, allowed and disallowed issuers.](#issuer)
6. [The default, allowed and disallowed signature algorithms.](#signature-algorithms)
7. [And which token types are valid or invalid.](#token-types)

Let's go over each of these configuration aspects in more detail.

## Target URL

Every test needs to call a valid API endpoint of your Web API project to verify whether the call is authenticated successfully or returns an unauthorized response, depending on the test case. Using the `TestSettings.TargetUrl` property, you can configure which API endpoint to use during testing.

## Default assertions 

At the end of each test, the HTTP response returned by the API needs to be verified to match the test scenario. Some tests expect the HTTP response to indicate an authorized response, while others expect an unauthorized response.

To configure these assertions, the `TestSettings` class has two properties:
* `TestSettings.AssertAuthorizedResponse`: an `Action<HttpResponseMessage` which asserts that the HTTP response is a valid, authorized response.

  By default, this delegate asserts that the HTTP response has a status code equal to `200 OK`. 
 
* `TestSettings.AssertUnauthorizedResponse`: an `Action<HttpResponseMessage` which asserts that the HTTP response is an invalid, unauthorized response.

  By default, this delegate asserts that the HTTP response has a status code equal to `401 Unauthorized`.
 
## Default test user

When an access token is being generated, JWT Guard uses the `TestSettings.DefaultTestUser` property to populate the claims in the token. 
At this time, only the `SubjectId` and `Username` properties are being used. 

If your APIs target URL needs specific claims in order to yield a valid authenticated result, you can add those claims to the default test user and include the claims inside the access token by extending the `Subject` property in the `JwtBuilder` class.

## Audience

Your API will typically have one or more valid audiences when accepting JWT access tokens.

When running the audience tests, JWT Guard uses the list of `TestSettings.AllowedAudiences` and `TestSettings.DisallowedAudiences` to verify that the audience validation works as intended.
The default audience used when generating an access token is set using `TestSettings.DefaultAudience`. 

## Issuer

The issuer is the trusted source of your JWT access tokens.
During the test setup, by default, JWT Guard will reconfigure the Web APIs JWT Bearer middleware and set both the authority and issuer to be the one defined in `TestSettings.DefaultIssuer`.

When running the issuer tests, JWT Guard uses the list of `TestSettings.AllowedIssuers` and `TestSettings.DisallowedIssuers` to verify that the issuer validation works as intended.

## Signature algorithms

Whenever JWT Guard generates an access token for a test case that doesn't target testing signature algorithms, `TestSettings.DefaultSignatureAlgorithm` is used to ensure that the access token has a valid signature.

To test access token signatures, JWT Guard contains a whole range of tests to verify that:
- unsigned tokens (using the "none" algorithm or another spelling like "nOnE") are not being accepted as valid access tokens
- only tokens using an allowed signature algorithm are accepted. JWT Guard uses `TestSettings.AllowedSignatureAlgorithms` and `TestSettings.DisallowedSignatureAlgorithms` to decide which algorithms are allowed or disallowed, respectively.
- tokens using included key material or externally hosted key material are not being accepted.

What do we mean with "tokens using included key material or externally hosted key material are not being accepted."? 
Most of us are familiar with the concept that an issued token will have a signature, and the token's header contains the following information about said signature:
- `alg`: the signature algorithm being used
- `kid`: the Key ID to uniquely identify the signature key used to sign the contents of the token.

The `kid` will match one of the keys known by the token issuer service, and your Web API can use the discovery documents to find the necessary information to verify the signature against the public key matching the `kid` property.

[RFC 7515](https://datatracker.ietf.org/doc/html/rfc7515) which describes JSON Web Signature (JWS), however, defines additional ways how a JWT can be signed:
- A `jwk` header can be used to immediately include the public key material for verifying the token contents.
- A `jku` header can be used to provide a URL that refers to a set of JSON-encoded public keys.
- An `x5c` header can be used to immediately include the X.509 public key certificate or certificate chain for verifying the token contents.
- An `x5u` header can be used to provide a URL that refers to the X.509 public key certificate or certificate chain for verifying the token contents.

If your Web API allows any of these valid JWS properties to be used when validating the token signature, anyone would be able to create valid access tokens!

## Token types

The header of a token typically contains the `typ` header with the value `JWT` for JSON Web Tokens. Some token issuers use a more specific type for the different token types, and use, for example, the value `at+jwt` for an access token.

To configure the token type validation tests, you can use `TestSettings.ValidTokenTypes` and `TestSettings.InvalidTokenTypes` to control the list of valid and invalid token types respectively.