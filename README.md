# AWS.Pipeline.CloudFormation
 AWS.Pipeline.CloudFormation

A project to demo AWS CodeDeploy.

## <span style="color:green">Useful commands</span>


```
aws s3 mb s3://YOUR_BUCKET_NAME

aws cloudformation package --template-file parent-codedeploy-ec2-setup.cfn.yml --output-template packaged-s3-parent-codedeploy-ec2-setup.cfn.yaml --s3-bucket YOUR_BUCKET_NAME
```

```
sudo service codedeploy-agent status
```
