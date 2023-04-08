using Xunit;
using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using Amazon.Textract.Model;
using Moq;

namespace Aws.StepFunctions.ResumeUploader.Tests;

public class LambdaFunctionsTests
{
  private readonly Mock<IAmazonS3> _s3Client;
  private readonly Mock<IAmazonTextract> _textractClient;

  private readonly TestLambdaContext _context;
  private readonly LambdaFunctions _functions;

  private StepFunctionState _state;

  public LambdaFunctionsTests()
  {
    _s3Client = new Mock<IAmazonS3>();
    _textractClient = new Mock<IAmazonTextract>();
    _context = new TestLambdaContext();

    _state = new StepFunctionState
    {
      FileName = "-- uploaded resume --"
    };

    _functions = new LambdaFunctions(_s3Client.Object, _textractClient.Object);
  }

  [Fact]
  public async Task UploadResume()
  {
    // arrange
    _s3Client
      .Setup(m => m.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
      .Returns("-- upload url --");

    // act
    _state = await _functions.UploadResume(_state, _context);

    // assert
    Assert.Equal("-- upload url --", _state.StoredFileUrl);
  }

  [Fact]
  public async Task LookForGithubProfile()
  {
    // arrange
    _textractClient
      .Setup(m => m.DetectDocumentTextAsync(It.IsAny<DetectDocumentTextRequest>(), default))
      .ReturnsAsync(new DetectDocumentTextResponse
      {
        Blocks = new List<Block>
        {
          new()
          {
            BlockType = BlockType.WORD,
            Text = "https://github.com/beautifulcoder"
          }
        }
      });

    // act
    _state = await _functions.LookForGithubProfile(_state, _context);

    // assert
    Assert.Equal("https://github.com/beautifulcoder", _state.GithubProfileUrl);
  }

  [Fact] public async Task OnFailedToUpload() =>
    Assert.NotNull(await _functions.OnFailedToUpload(_state, _context));
}
