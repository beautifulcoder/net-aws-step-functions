# .NET Step Functions

This starter project consists of:

* serverless.template - An AWS CloudFormation template file for declaring your Serverless functions and other AWS resources
* state-machine.json -The definition of the Step Function state machine.
* LambdaFunctions.cs - This class contains the Lambda functions that the Step Function state machine will call.
* StepFunctionState.cs - This class represent the state of the step function executions between Lambda function calls.
* aws-lambda-tools-defaults.json - default argument settings for use with the command line deployment tools for AWS

This sample code has a resume uploader built using step functions. Each lambda function represents a step in the workflow. Then, results are placed in a SQS queue for asynchronous consumption. 

You may also have a test project depending on the options selected.

### Test State Machine

Once the project is deployed you can test it with the Step Functions in the web console https://console.aws.amazon.com/states/home. Select the newly created state machine and then click the "New Execution" button. Enter the initial JSON document for the input to the execution which will be serialized in to the State object. This project will look for a "Name" property to use in its execution. Here is an example input JSON.

{
    "FileName" : "ExampleResume.pdf"
}

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
  dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
  dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
  cd "Aws.StepFunctions.ResumeUploader/test/Aws.StepFunctions.ResumeUploader.Tests"
  dotnet test
```

Deploy application
```
  cd "Aws.StepFunctions.ResumeUploader/src/Aws.StepFunctions.ResumeUploader"
  dotnet lambda deploy-function --function-name upload-resume-step --function-handler Aws.StepFunctions.ResumeUploader::Aws.StepFunctions.ResumeUploader.LambdaFunctions::UploadResume
  dotnet lambda deploy-function --function-name look-for-github-profile-step --function-handler Aws.StepFunctions.ResumeUploader::Aws.StepFunctions.ResumeUploader.LambdaFunct
ions::LookForGithubProfile
  dotnet lambda deploy-function --function-name on-failed-to-upload-step --function-handler Aws.StepFunctions.ResumeUploader::Aws.StepFunctions.ResumeUploader.LambdaFunctions
::OnFailedToUpload
```
