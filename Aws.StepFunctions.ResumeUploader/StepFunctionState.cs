namespace Aws.StepFunctions.ResumeUploader;

public class StepFunctionState
{
  public string FileName { get; set; } = string.Empty;

  public string StoredFileUrl { get; set; } = string.Empty;

  public string? GithubProfileUrl { get; set; }
}
