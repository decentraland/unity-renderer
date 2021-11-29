# Setup CircleCI in your own repository

To set up the CI, you will need:

- Setup your project in CircleCI
- Create the Environment Variable for the Unity License and an AWS S3 Bucket
- `Performance Plan` in CircleCI because you need an `XLarge` computer to build Unity

## Setup your project in CircleCI

Log in with your GitHub user in CircleCI.

Set up the `unity-renderer` project.

## Unity License Environment Variable

To generate the unity license content, you need a `Unity Activation File`.

This file must be created from the computer that will use Unity (in this case, the CI).

And then you must use that file to apply your Unity License (https://license.unity3d.com/manual)

### Pre-created Activation File

CircleCI uses the same Hardware ID for every run, so you can use the pre-created file: [resources/circleci-unity-activation-file-v2020.3.0f1.alf](resources/circleci-unity-activation-file-v2020.3.0f1.alf)

### Create Activation File in CircleCI

If you can not use the pre-created activation file, you should follow this step.

Execute in a CircleCI Machine (Use `Rerun with SSH` in CircleCI) the following command:

```
bash -c '$UNITY_PATH/Editor/Unity -quit -nographics -logFile /tmp/unity-createManualActivationFile.log -batchmode -createManualActivationFile'
```

And the generated file it's the activation file for the following step.

### Generate Unity License Content

After having the `circleci-unity-activation-file-v2020.3.0f1.alf` you must activate it with the following steps:
- Enter https://license.unity3d.com/manual with your Unity Credentials
- Upload the `circleci-unity-activation-file-v2020.3.0f1.alf`
- Download the `Unity_v2020.x.ulf`
- Convert to base64, suggested command: `cat Unity_v2020.x.ulf | base64`
- Create a CircleCI Environment Variable with the name of `DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64` and the base64 output

And you will be able to build with CircleCI.

## AWS S3 Bucket

Create an AWS S3 Bucket (with all public access, or you can use CloudFront)
Create an IAM User with `Access key - Programmatic access` and the following policy:

```
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Action": [
                "s3:*",
                "s3-object-lambda:*"
            ],
            "Resource": [
                "arn:aws:s3:::YOUR_BUCKET",
                "arn:aws:s3:::YOUR_BUCKET/*"
            ]
        }
    ]
}
```
(replace YOUR_BUCKET with your bucket name)

Set up the following Environment Variables in CircleCI with the credentials generated above:

- `AWS_ACCESS_KEY_ID`=IAM User generated key id
- `AWS_SECRET_ACCESS_KEY`=IAM User generated secret access key
- `S3_BUCKET`=S3 Bucket name