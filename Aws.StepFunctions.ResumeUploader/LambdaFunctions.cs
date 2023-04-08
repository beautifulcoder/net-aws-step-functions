using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Textract;
using Amazon.Textract.Model;
using S3Object = Amazon.Textract.Model.S3Object;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace Aws.StepFunctions.ResumeUploader;

public class LambdaFunctions
{
  private readonly IAmazonS3 _s3Client;
  private readonly IAmazonTextract _textractClient;

  private const string S3BucketName = "resume-uploader-upload";

  public LambdaFunctions()
  {
    _s3Client = new AmazonS3Client(RegionEndpoint.USEast1);
    _textractClient = new AmazonTextractClient(RegionEndpoint.USEast1);
  }

  public LambdaFunctions(IAmazonS3 s3Client, IAmazonTextract textractClient)
  {
    _s3Client = s3Client;
    _textractClient = textractClient;
  }

  public Task<StepFunctionState> UploadResume(
    StepFunctionState state,
    ILambdaContext context)
  {
    state.StoredFileUrl = _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
    {
      BucketName = S3BucketName,
      Key = state.FileName,
      Expires = DateTime.UtcNow.AddDays(1)
    });

    return Task.FromResult(state);
  }

  public async Task<StepFunctionState> LookForGithubProfile(
    StepFunctionState state,
    ILambdaContext context)
  {
    var detectResponse = await _textractClient.DetectDocumentTextAsync(
      new DetectDocumentTextRequest
      {
        Document = new Document
        {
          S3Object = new S3Object
          {
            Bucket = S3BucketName,
            Name = state.FileName
          }
        }
      });

    state.GithubProfileUrl = detectResponse
      .Blocks
      .FirstOrDefault(x =>
        x.BlockType == BlockType.WORD && x.Text.Contains("github.com"))
      ?.Text;

    return state;
  }

  public Task<StepFunctionState> OnFailedToUpload(
    StepFunctionState state,
    ILambdaContext context)
  {
    LambdaLogger.Log("A PDF resume upload to S3 Failed!");

    return Task.FromResult(state);
  }
}
