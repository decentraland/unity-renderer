FROM gableroux/unity3d:2019.1.14f1

RUN apt update && apt upgrade -y && apt install -y git curl unzip
RUN mkdir -p ~/.ssh
RUN ssh-keyscan -H github.com >> ~/.ssh/known_hosts
RUN curl -s https://packagecloud.io/install/repositories/github/git-lfs/script.deb.sh | bash
RUN apt update && apt install -y git-lfs=2.8.0
RUN apt install -y curl software-properties-common
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash -
RUN apt install -y nodejs
