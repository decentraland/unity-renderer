
# Setup CircleCI in your own repository

This guide helps you set up your own continuous integration system in your fork for running tests, as well as creating test builds for each of the commits that are made to the repository. 

For this, you will need to perform the following steps:
- Create your CircleCI account and set up the repository.
- Create a Unity license
- Set up your own S3 bucket to upload test versions and test coverage results. 


## 1) Setup your project in CircleCI

Creating a CircleCI account is free, however the free plan machines are not powerful enough to run the full pipeline. 
You will have to upgrade to at least the performance plan. The performance plan gives you 25,000 credits per $15, in addition to the 30,000 included in the base plan, which allows for ~70 builds per month. 
Use your github account to login and set up a new project pointing to your `unity-renderer` fork.

## 2) Unity License Environment Variable

To generate the Unity license content, you need a `Unity Activation File`. If you don't have an Unity account, you will need to create one first, and the go to the [manual license activation](https://license.unity3d.com/manual) to generate the license. 
This requires an `.alf` file containing information of the computer that will run the CI. To obtain this file, you can either:

A) Download [this pregenerated file](resources/circleci-unity-activation-file-v2020.3.0f1.alf) from the repo, as CircleCI uses the same Hardware ID for every run. 

B) Create your own activation file, running in a CircleCI Machine (Use `Rerun with SSH` in CircleCI) the following command:
```
bash -c '$UNITY_PATH/Editor/Unity -quit -nographics -logFile /tmp/unity-createManualActivationFile.log -batchmode -createManualActivationFile'
```
Once you upload the file, the licensing system will ask you to define the type of license you want to generate. In this case, the free personal license is more than enough.

The system will now generate an `.ulf` file containing your license. The content of this file must be converted to base64 (`cat Unity_v2020.x.ulf | base64` or you can also use [this site](https://www.base64encode.org/)) and add it to CircleCI as an environment variable of the project named `DEVELOPERS_UNITY_LICENSE_CONTENT_2020_3_BASE64`.

## 3) Setting up AWS S3 Bucket
You will need to create a bucket in S3 with public privileges in order to upload the CI content. For that:
1) Create an AWS S3 Bucket, with all public access.
2) Create a new policy with the following content:
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
3) Create an IAM User with `Access key - Programmatic access` and attach the policy created before.

Set up the following Environment Variables in CircleCI with the credentials generated above:

- `AWS_ACCESS_KEY_ID`=IAM User generated key id
- `AWS_SECRET_ACCESS_KEY`=IAM User generated secret access key
- `S3_BUCKET`=S3 Bucket name

## 4) Run the CI 
After any commit to any of the branches in your fork, the CI will trigger. In order to test the builds successfully, you must inject the generated artifact using the `renderer` parameter as follows:
https://play.decentraland.org/?renderer=S3_BUCKET_URL/BRANCH_NAME
