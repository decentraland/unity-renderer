FROM unityci/editor:2021.3.14f1-webgl-1

# Install dependencies zip
RUN apt-get update && apt-get install -y zip jq gpg xvfb

# Clean cache
RUN apt-get clean && rm -rf /var/lib/apt/lists/*
