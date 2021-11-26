# Setup CircleCI in your own repository

To set up the CI, you will need:

- Setup your project in CircleCI
- Create the Environment Variable for the Unity License and an AWS S3 Bucket

## Setup your project in CircleCI

Log in with your user in CircleCI.

## Unity License Environment Variable

To generate the unity license content, you need a `Unity Activation File`.

This file must be created from the computer that will use Unity (in this case, the CI).

And then you must use that file to apply your Unity License (https://license.unity3d.com/manual)

### Pre-created Activation File

CircleCI uses the same Hardware ID for every run, so you can use the pre-created file: [resources/circleci-unity-activation-file-v2020.3.0f1.alf](resources/circleci-unity-activation-file-v2020.3.0f1.alf)

### Create Activation File

(TODO: Change the pipeline to detect when the UNITY LICENSE is empty and create this activation file)
Execute in a CircleCI Machine the following command:

```
bash -c '$UNITY_PATH/Editor/Unity -quit -nographics -logFile /tmp/unity-createManualActivationFile.log -batchmode -createManualActivationFile'
```

### Generate Unity License Content

After having the `circleci-unity-activation-file-v2020.3.0f1.alf` you must activate it with the following steps:
- Enter https://license.unity3d.com/manual with your Unity Credentials
- Upload the `circleci-unity-activation-file-v2020.3.0f1.alf`
- Download the `Unity_v2020.x.ulf`
- Convert to base64, suggested command: `cat Unity_v2020.x.ulf | base64`
- Create a CircleCI Environment Variable with the name of `DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64` and the base64 output

And you will be able to build with CircleCI.

### Generate Unity License Content

After having the `circleci-unity-activation-file-v2020.3.0f1.alf` you must activate it with the following steps:
- Enter https://license.unity3d.com/manual with your Unity Credentials
- Upload the `circleci-unity-activation-file-v2020.3.0f1.alf`
- Download the `Unity_v2020.x.ulf`
- Convert to base64, suggested command: `cat Unity_v2020.x.ulf | base64`
- Create a CircleCI Environment Variable with the name of `DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64` and the base64 output

And you will be able to build with CircleCI.

## AWS S3 Bucket

Create a AWS S3 Bucket (Public)
Create a IAM User with `Access key - Programmatic access` and the following policy:

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

Set up the following Environment Variables in CircleCI with the credentials generated above:

- `AWS_ACCESS_KEY_ID`
- `AWS_SECRET_ACCESS_KEY`
- `S3_BUCKET`