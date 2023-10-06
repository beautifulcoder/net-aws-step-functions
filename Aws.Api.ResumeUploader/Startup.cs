using Amazon.Lambda.Annotations;
using Amazon.S3;
using Amazon.SQS;
using Amazon.StepFunctions;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Api.ResumeUploader;

[LambdaStartup]
public class Startup
{
  public void ConfigureServices(IServiceCollection services)
  {
    services.AddAWSService<IAmazonS3>();
    services.AddAWSService<IAmazonStepFunctions>();
    services.AddAWSService<IAmazonSQS>();
  }
}
