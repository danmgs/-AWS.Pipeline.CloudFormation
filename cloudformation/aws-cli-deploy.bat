@ECHO OFF
SETLOCAL

REM ** Configure here your settings ******************************************
SET YOUR_BUCKET_NAME=xxx
SET YOUR_AWS_PROFILE=xxx
SET YOUR_AWS_REGION=xxx
SET YOUR_PACKAGED_STACK_TEMPLATE=packaged-s3-pipeline-parent-stack.cfn.yml
SET YOUR_STACK_NAME=myappdemo
REM **************************************************************************

aws s3 mb s3://%YOUR_BUCKET_NAME%

REM ** Clean before new uploads
aws s3 rm --recursive s3://%YOUR_BUCKET_NAME%

ECHO Push any key to package nested stacks
PAUSE

REM ** We create the cloudformation template
aws cloudformation package --template-file pipeline-parent-stack.cfn.yml --output-template %YOUR_PACKAGED_STACK_TEMPLATE% --s3-bucket %YOUR_BUCKET_NAME%

ECHO Push any key to create stack
SET /P RES=Do you want do create the stack (y/[n])?
IF /I "%RES%" NEQ "y" GOTO END

aws cloudformation create-stack --stack-name %YOUR_STACK_NAME% --template-body file://%YOUR_PACKAGED_STACK_TEMPLATE% --parameters file://parameters.json --profile %YOUR_AWS_PROFILE% --region %YOUR_AWS_REGION% --capabilities CAPABILITY_NAMED_IAM

PAUSE

:END
endlocal


