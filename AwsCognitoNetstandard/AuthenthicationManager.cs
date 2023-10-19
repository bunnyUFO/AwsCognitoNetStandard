namespace AwsCognitoNetstandard;

using System.Collections.Generic;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using System;
using System.Threading.Tasks;

public class CognitoAuthenticationManager
{
    private Amazon.RegionEndpoint _region;
    private readonly string _identityPool;
    private readonly string _appClientID;
    private readonly string _userPoolId;

    private readonly AmazonCognitoIdentityProviderClient _provider;
    private CognitoAWSCredentials _cognitoAwsCredentials;
    private string _userid = "";
    private CognitoUser _user;

    private UserInfo _userInfo = new UserInfo( "", "","","");

    public CognitoAuthenticationManager(String region, String userPoolId, String identityPool, String appClientID)
    {
        _identityPool = identityPool;
        _appClientID = appClientID;
        _userPoolId = userPoolId;
        _region = Amazon.RegionEndpoint.GetBySystemName(region);
        _provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), _region);
    }

    public async Task<bool> Signup(string username, string email, string password)
    {
        SignUpRequest signUpRequest = new SignUpRequest()
        {
            ClientId = _appClientID,
            Username = username,
            Password = password
        };

        List<AttributeType> attributes = new List<AttributeType>()
        {
            new AttributeType()
            {
                Name = "email", Value = email
            }
        };
        signUpRequest.UserAttributes = attributes;

        try
        {
            SignUpResponse signupResponse = await _provider.SignUpAsync(signUpRequest);
            _userInfo = new UserInfo( "", "","",signupResponse.UserSub);
            Console.Write("Sign up successful");
            return true;
        }
        catch (Exception e)
        {
            Console.Write("Sign up failed, exception: " + e);
            return false;
        }
    }

    public async Task<bool> Login(string username, string password)
    {
        CognitoUserPool userPool = new CognitoUserPool(_userPoolId, _appClientID, _provider);
        CognitoUser user = new CognitoUser(username, _appClientID, userPool, _provider);

        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = password
        };

        try
        {
            AuthFlowResponse authFlowResponse = await user.StartWithSrpAuthAsync(authRequest).ConfigureAwait(false);

            _userid = await GetUserIdFromProvider(authFlowResponse.AuthenticationResult.AccessToken);
            _userInfo = new UserInfo(
                authFlowResponse.AuthenticationResult.IdToken,
                authFlowResponse.AuthenticationResult.AccessToken,
                authFlowResponse.AuthenticationResult.RefreshToken,
                _userid
            );

            // This how you get credentials to use for accessing other services.
            // This _identityPool is your Authorization, so if you tried to access using an
            // _identityPool that didn't have the policy to access your target AWS service, it would fail.
            _cognitoAwsCredentials = user.GetCognitoAWSCredentials(_identityPool, _region);

            _user = user;

            return true;
        }
        catch (Exception e)
        {
            Console.Write("Login failed, exception: " + e);
            return false;
        }
    }

    public UserInfo GetUserAuthInfo()
    {
        return _userInfo;
    }
    
    public CognitoUser GetUser()
    {
        return _user;
    }

    private async Task<string> GetUserIdFromProvider(string accessToken)
    {
        // Console.Write("Getting user's id...");
        string subId = "";

        Task<GetUserResponse> responseTask =
            _provider.GetUserAsync(new GetUserRequest
            {
                AccessToken = accessToken
            });

        GetUserResponse responseObject = await responseTask;

        // set the user id
        foreach (var attribute in responseObject.UserAttributes)
        {
            if (attribute.Name == "sub")
            {
                subId = attribute.Value;
                break;
            }
        }

        return subId;
    }

    // Limitation note: so this GlobalSignOutAsync signs out the user from ALL devices, and not just the game.
    // So if you had other sessions for your website or app, those would also be killed.  
    // Currently, I don't think there is native support for granular session invalidation without some work arounds.
    public async void SignOut()
    {
        await _user.GlobalSignOutAsync();
        Console.Write("user logged out.");
    }

    // access to the user's authenticated credentials to be used to call other AWS APIs
    public CognitoAWSCredentials GetCredentials()
    {
        return _cognitoAwsCredentials;
    }
}