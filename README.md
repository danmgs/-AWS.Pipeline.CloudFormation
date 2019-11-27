# AWS.Pipeline.CloudFormation
 AWS.Pipeline.CloudFormation

A project to demo AWS CodePipeline.

## <span style="color:green">Useful commands</span>


```
aws s3 mb s3://YOUR_BUCKET_NAME

aws cloudformation package --template-file pipeline-parent-stack.cfn.yml --output-template packaged-s3-pipeline-parent-stack.cfn.yml --s3-bucket YOUR_BUCKET_NAME
```

```
sudo service codedeploy-agent status
```

