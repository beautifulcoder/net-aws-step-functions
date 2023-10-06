using System.Net;
using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Moq;
using Xunit;

namespace Aws.Api.ResumeUploader.Tests;

public class FunctionsTest
{
  private readonly Mock<IAmazonS3> _s3Client;
  private readonly Mock<IAmazonStepFunctions> _sfnClient;
  private readonly Mock<IAmazonSQS> _sqsClient;

  private readonly Functions _functions;

  public FunctionsTest()
  {
    _s3Client = new Mock<IAmazonS3>();
    _sfnClient = new Mock<IAmazonStepFunctions>();
    _sqsClient = new Mock<IAmazonSQS>();

    _functions = new Functions(
      _s3Client.Object,
      _sfnClient.Object,
      _sqsClient.Object);
  }

  [Fact]
  public async Task PostReturns201()
  {
    // arrange
    var context = new TestLambdaContext();
    const string fileContent = "TWFueSBoYW5kcyBtYWtlIGxpZ2h0IHdvcmsu";
    const string fileName = "test-file-name";

    // act
    var response = await _functions.Post(fileContent, fileName, context);

    // assert
    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    _s3Client.Verify(m => m.PutObjectAsync(
      It.IsAny<PutObjectRequest>(),
      CancellationToken.None));
    _sfnClient.Verify(m => m.StartExecutionAsync(
      It.IsAny<StartExecutionRequest>(),
      CancellationToken.None));
  }

  [Fact]
  public async Task GetReturns200()
  {
    // arrange
    var context = new TestLambdaContext();

    _sqsClient
      .Setup(m => m.ReceiveMessageAsync(
        It.IsAny<ReceiveMessageRequest>(),
        CancellationToken.None))
      .ReturnsAsync(new ReceiveMessageResponse
      {
        Messages = new List<Message>
        {
          new() {Body = "stuff"}
        }
      });

    // act
    var response = await _functions.Get(context);

    // assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }

  [Fact]
  public async Task DeleteReturns202()
  {
    // arrange
    var context = new TestLambdaContext();

    // act
    var response = await _functions.Delete(context);

    // assert
    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
    _sqsClient
      .Verify(m => m.PurgeQueueAsync(
        It.IsAny<PurgeQueueRequest>(),
        CancellationToken.None));
  }
}
