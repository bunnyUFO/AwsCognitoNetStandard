namespace AwsCognitoNetstandard;
using System;

public struct UserInfo
{
    public UserInfo(String idToken, String accessToken, String refreshToken, String userId)
    {
        IdToken = idToken;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        UserId = userId;
    }

    public String IdToken { get; }
    public String AccessToken { get; }
    public String RefreshToken { get; }
    public String UserId { get; }
}