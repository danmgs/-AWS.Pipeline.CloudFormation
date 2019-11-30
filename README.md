# AWS.Pipeline.CloudFormation

A project to demo AWS CodePipeline with cloudformation templates deployment.

![alt capture](https://github.com/danmgs/AWS.Pipeline.CloudFormation/blob/master/img/Code_Pipeline_Diagram.svg)

## Package and deploy

You can run each scripts in the **/cloudformation/** directory separately.

You can build packaged all nested templates into one, via command :

```
# create your S3 bucket (must be global unique name)
aws s3 mb s3://YOUR_BUCKET_NAME

# package to one cfn template
aws cloudformation package --template-file pipeline-parent-stack.cfn.yml --output-template packaged-s3-pipeline-parent-stack.cfn.yml --s3-bucket YOUR_BUCKET_NAME
```

For instance here, this results in one template named **packaged-s3-pipeline-parent-stack.cfn.yml**, to be deployed in Cloud Formation.

## Useful commands

To run under EC2 instance :

```
# check EC2 setup

sudo service codedeploy-agent status
sudo service --status-all
```

