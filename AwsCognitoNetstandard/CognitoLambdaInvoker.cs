
namespace AwsCognitoNetstandard;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.CognitoIdentity;

public class CognitoLambdaInvoker
{
    private AmazonLambdaClient _lamdbaClient;
    
    public CognitoLambdaInvoker(String region, CognitoAWSCredentials credentials)
    {
        _lamdbaClient = new AmazonLambdaClient(credentials, Amazon.RegionEndpoint.GetBySystemName(region));
    }

    public async Task<string> InvokeLambda(String functionName, String paylaod = "")
    {
        InvokeRequest invokeRequest = new InvokeRequest
        {
            FunctionName = functionName,
            Payload = paylaod,
            InvocationType = InvocationType.RequestResponse
        };
        
        InvokeResponse response = await _lamdbaClient.InvokeAsync(invokeRequest);
        MemoryStream stream = response.Payload;
        string returnValue = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        return returnValue;
    }
}