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

    private AuthInfo _authInfo = new ( "", "","","", "");

    /// <summary>
    /// Class <c>CognitoAuthenticationManager</c> is initialized with cognito settings.
    /// Has methods to sign up, login, and get credentials, and get authentication info for a cognito user.
    /// </summary>
    public CognitoAuthenticationManager(String region, String userPoolId, String identityPool, String appClientID)
    {
        _identityPool = identityPool;
        _appClientID = appClientID;
        _userPoolId = userPoolId;
        _region = Amazon.RegionEndpoint.GetBySystemName(region);
        _provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), _region);
    }

    /// <summary>
    /// Method <c>Signup</c> signups up a user with username email and password.
    /// </summary>
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
            _authInfo = new AuthInfo( "", "","",signupResponse.UserSub, username);
            Console.Write("Sign up successful");
            return true;
        }
        catch (Exception e)
        {
            Console.Write("Sign up failed, exception: " + e);
            return false;
        }
    }

    /// <summary>
    /// Method <c>Login</c> login with username and password.
    /// saves authentication info and aws credentials.
    /// User will need to be verified before login will work.
    /// </summary>
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
            _authInfo = new AuthInfo(
                authFlowResponse.AuthenticationResult.IdToken,
                authFlowResponse.AuthenticationResult.AccessToken,
                authFlowResponse.AuthenticationResult.RefreshToken,
                _userid,
                username
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

    /// <summary>
    /// Method <c>GetUserAuthInfo</c> returns struct of type AuthInfo with session, access, and refresh tokens.
    /// </summary>
    public AuthInfo GetUserAuthInfo()
    {
        return _authInfo;
    }
    
    
    // access to the user's authenticated credentials to be used to call other AWS APIs
    
    
    /// <summary>
    /// Method <c>GetCredentials</c> returns saved aws credentials generated during login.
    /// Credentials will be null prior to login (or if login fails)
    /// </summary>
    public CognitoAWSCredentials GetCredentials()
    {
        return _cognitoAwsCredentials;
    }

    /// <summary>
    /// Method <c>SignOut</c> will sign out user form ALL devices. Other sessions in website or apps would also be logged
    /// out. Have not found a workaround to just log out a single session yet. 
    /// </summary>
    public async void SignOut()
    {
        await _user.GlobalSignOutAsync();
        Console.Write("user logged out.");
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
}