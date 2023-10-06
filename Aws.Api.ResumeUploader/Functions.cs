using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Aws.Api.ResumeUploader;

public class Functions
{
  private readonly IAmazonS3 _s3Client;
  private readonly IAmazonStepFunctions _sfnClient;
  private readonly IAmazonSQS _sqsClient;

  private const string S3BucketName = "resume-uploader-upload";
  private const string StateMachineArn = "<state-machine-arn>";
  private const string SqsUrl = "<sqs-url>";

  public Functions(
    IAmazonS3 s3Client,
    IAmazonStepFunctions sfnClient,
    IAmazonSQS sqsClient)
  {
    _s3Client = s3Client;
    _sfnClient = sfnClient;
    _sqsClient = sqsClient;
  }

  [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 1024, Timeout = 5)]
  [RestApi(LambdaHttpMethod.Get, "/")]
  public async Task<IHttpResult> Get(ILambdaContext context)
  {
    var result = await _sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
    {
      QueueUrl = SqsUrl
    });

    return HttpResults.Ok(result.Messages.Select(m => m.Body));
  }

  [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 1024, Timeout = 5)]
  [RestApi(LambdaHttpMethod.Post, "/")]
  public async Task<IHttpResult> Post(
    [FromBody] string fileContent,
    [FromQuery] string fileName,
    ILambdaContext context)
  {
    var byteArray = Convert.FromBase64String(fileContent);
    using var inputStream = new MemoryStream(byteArray);

    await _s3Client.PutObjectAsync(new PutObjectRequest
    {
      BucketName = S3BucketName,
      Key = fileName,
      InputStream = inputStream
    });

    await _sfnClient.StartExecutionAsync(new StartExecutionRequest
    {
      Input = JsonSerializer.Serialize(new {storedFileName = fileName}),
      StateMachineArn = StateMachineArn
    });

    return HttpResults.Created();
  }

  [LambdaFunction(Policies = "AWSLambdaBasicExecutionRole", MemorySize = 1024, Timeout = 5)]
  [RestApi(LambdaHttpMethod.Delete, "/")]
  public async Task<IHttpResult> Delete(ILambdaContext context)
  {
    await _sqsClient.PurgeQueueAsync(new PurgeQueueRequest
    {
      QueueUrl = SqsUrl
    });

    return HttpResults.Accepted();
  }
}
