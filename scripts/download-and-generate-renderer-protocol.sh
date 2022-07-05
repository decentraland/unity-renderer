#!/usr/bin/env bash
set -e

install_proto()
{
    PROTOC_PATH=node_modules/.bin/protobuf/bin/protoc
    if [[ ! -f "$PROTOC_PATH" ]]; then # Check if is instaled
        PROTOBUF_VERSION=3.20.1

        if [[ "$OSTYPE" == "darwin"* ]]; then
            PROTOBUF_ZIP=protoc-${PROTOBUF_VERSION}-osx-x86_64.zip
        else
            PROTOBUF_ZIP=protoc-${PROTOBUF_VERSION}-linux-x86_64.zip
        fi
        curl -OL https://github.com/protocolbuffers/protobuf/releases/download/v${PROTOBUF_VERSION}/${PROTOBUF_ZIP}
        mkdir -p node_modules/.bin/protobuf
        unzip -o ${PROTOBUF_ZIP} -d node_modules/.bin/protobuf
        rm ${PROTOBUF_ZIP}

        chmod +x "${PROTOC_PATH}"
    fi
    echo "${PROTOC_PATH}" # Return protoc path
}

mkdir -p temp && cd temp
PROTOC=$(install_proto)
npm install @dcl/protocol@next
npm install protoc-gen-dclunity@next

RENDERER_PROTOCOL_PATH=node_modules/@dcl/protocol/renderer-protocol/
CODEGEN_PLUGIN_PATH=node_modules/protoc-gen-dclunity/dist/index.js
OUTPUT_PATH=unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/
mkdir -p ${OUTPUT_PATH}


PROTOS=$(find "${RENDERER_PROTOCOL_PATH}" -name '*.proto' -print)

echo "Generating: "
echo ${PROTOS}

PROTOC \
    --plugin=protoc-gen-dclunity="${CODEGEN_PLUGIN_PATH}" \
    --dclunity_out="${OUTPUT_PATH}" \
    --csharp_out="${OUTPUT_PATH}" \
    --csharp_opt=file_extension=.gen.cs \
    "${PROTOS}"