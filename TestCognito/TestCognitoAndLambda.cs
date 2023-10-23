// Executable cli class to test cognito user creation, login, and lambda invoke with user credentials

using AwsCognitoNetstandard;

string GetPassword()
{
    String pwd = "";
    while (true)
    {
        ConsoleKeyInfo i = Console.ReadKey(true);
        if (i.Key == ConsoleKey.Enter)
        {
            break;
        }
        
        if (i.Key == ConsoleKey.Backspace)
        {
            if (pwd.Length <= 0) continue;
            pwd.Remove(pwd.Length - 1);
            Console.Write("\b \b");
        }
        else if (i.KeyChar != '\u0000' ) // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
        {
            pwd += i.KeyChar;
            Console.Write("*");
        }
    }
    return pwd;
}

// Update region as needed
string region = "us-west-2";

// Update cognito configuration as needed
var cognitoManager = new CognitoAuthenticationManager(
    region,
    "<YOUR USER POOL ID HERE>",
    "<YOUR IDENTITY POOL ID HERE>",
    "<YOUR APP CLIENT ID HERE>"
);

Console.WriteLine("Already Have user y/n?");
String response = Console.ReadLine();

bool login = response is "Y" or "y"; 


Console.WriteLine("\nEnter username:");
string username = Console.ReadLine();


Console.WriteLine("\nEnter password:");
String password = GetPassword();
Console.WriteLine("");

if (login)
{
    await cognitoManager.Login(username, password);
    Console.WriteLine($"\nsuccessfully logged in user: {cognitoManager.GetUserAuthInfo().UserId}");

    LambdaInvoker lambdaInvoker = new LambdaInvoker(region, cognitoManager.GetCredentials());
    
    // Update function payload as needed here
    var functionPayload = $"{{\"user_id\": \"{cognitoManager.GetUserAuthInfo().UserId}\", \"username\": \"{username}\", \"gold\": 10, \"reputation\": 0, \"cards\": {{\"slash\": 5, \"block\": 5 }} }}";
    Console.WriteLine($"\ninvoking GetUser lambda with payload #{functionPayload}");
    
    var responsePayload = await lambdaInvoker.InvokeLambda("deck-consultant-create-user", functionPayload);
    Console.WriteLine($"lamdba responded with #{responsePayload}");
    
    // Update function payload or remove second function call
    functionPayload = $"{{\"user_id\": \"{cognitoManager.GetUserAuthInfo().UserId}\"}}";
    Console.WriteLine($"\ninvoking GetUser lambda with payload #{functionPayload}");
    
    responsePayload = await lambdaInvoker.InvokeLambda("deck-consultant-get-user", functionPayload);
    Console.WriteLine($"lamdba responded with #{responsePayload}");
    
    cognitoManager.SignOut();
}
else
{
    Console.WriteLine("\nEnter email:");
    String email = Console.ReadLine();
    await cognitoManager.Signup(username, email, password);
    Console.WriteLine($"\nSuccessfully signed up user: {cognitoManager.GetUserAuthInfo().UserId}");
    Console.WriteLine("Check email to verify before trying to log in");
}