namespace AwsCognitoNetstandard;
using System;

public struct AuthInfo
{
    public AuthInfo(String idToken, String accessToken, String refreshToken, String userId, String username)
    {
        IdToken = idToken;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
        Username = username;
    }

    public String IdToken { get; }
    public String AccessToken { get; }
    public String RefreshToken { get; }
    public String UserId { get; }
    public String Username { get; }
}