#!/usr/bin/env bash

source ci-setup.sh

echo "Uploading Benchmark Report for $PROJECT_PATH"

wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

apt-get update; \
apt-get install -y apt-transport-https && \
apt-get update && \
apt-get install -y dotnet-sdk-3.1

PROJECT_PATH="$(pwd)/unity-renderer"
cd ${PROJECT_PATH}/unity-renderer/
mkdir -p reporter && cd reporter
wget https://github.com/Unity-Technologies/PerformanceBenchmarkReporter/releases/download/1.2.0/UnityPerformanceBenchmarkReporter_1_2_0.zip -O UnityPerformanceBenchmarkReporter.zip
unzip UnityPerformanceBenchmarkReporter.zip
rm UnityPerformanceBenchmarkReporter.zip

cp ../benchmark-results.xml .
dotnet UnityPerformanceBenchmarkReporter.dll --results=benchmark-results.xml --reportdirpath=output
mkdir -p output

cp benchmark-results.xml output/UnityPerformanceBenchmark/
cd output/UnityPerformanceBenchmark/
mv UnityPerformanceBenchmark*.html index.html
aws s3 sync ./ "s3://${S3_BUCKET}/branch-benchmark/${CIRCLE_BRANCH}" --acl public-read
