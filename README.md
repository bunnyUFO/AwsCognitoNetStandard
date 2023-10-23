# AwsCognitoNetStandard
Goal of this repository is to create a dotnet standard 2.1 dll (dynamic link library) compatible with unity<br>
to facilitate authentication with AWS cognito and invoking lamdbas usign suer credentials.<br>

This repository is based on this [youtube video](https://www.youtube.com/watch?v=qzr57U2gWeE&list=PLN9fo6wxjySHe6VkGn_JAXIfeRS7Zp-3i&index=2&ab_channel=BatteryAcidDev) by Battery Acid Dev<br>
If not familair with AWS cognito reccomend watching that video.

## CognitoAuthenticationManager
Class to help sign up, login, and get aws creentials for cognito useres.<br>
Initialized with cognito settings as shown in example below<br>
```C#
var cognitoManager = new CognitoAuthenticationManager(
    "us-west-2",
    "us-west-2-your-cognito-userpool-id",
    "us-west-2-your-cognito-identity-pool-id",
    "your-cognito-app-client"
);
```

Once class is initialized, can call signup to create a user:
```C#
bool success = await cognitoManager.Signup(username, email, password);
```

It's likely user will need to do some form of verification before being able to log in.<br>
Common methods are using verficaiton code or a verificaiton link sent to email.<br>
This project assumes verification link with email is being used, so it does not ass utility to veriy using code yet.

Once user is verified can log in with:
```C#
bool success = await cognitoManager.Login(username, password);
```
Login will also generate aws credentials.

## LambdaInvoker
To intialize needs region and aws credentials you can get credentials from an instance of `CognitoAuthenticationManager` after a login
```C#
LambdaInvoker lambdaInvoker = new LambdaInvoker(region, cognitoManager.GetCredentials());
```

You can invoke a lambda usign those credentials now
```C#
string responsePayload = await lambdaInvoker.InvokeLambda("function-name");
```

You can also invoke a lambda using a payload.
```C#
string functionPaylaod = "{\"Key\": \"value\"}";
string responsePayload = await lambdaInvoker.InvokeLambda("function-with-payload", functionPaylaod);
```

In previous example payload is a raw JSON string, you can also create one by serializing a struct or class.
```C#

public record struct User( string username;
User user = new User { username = "John" };

string functionPayload = JsonSerializer.Serialize(user);
string responsePayload = await lambdaInvoker.InvokeLambda("function-with-payload", functionPaylaod);
```
