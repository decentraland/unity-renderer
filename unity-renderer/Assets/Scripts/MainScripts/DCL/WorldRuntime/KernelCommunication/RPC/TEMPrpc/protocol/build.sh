#!/bin/bash

# replace this with your own protoc
protoc \
		--csharp_out="$(PWD)/protocol" \
		--csharp_opt=file_extension=.gen.cs \
		-I="$(PWD)/protocol" \
		"$(PWD)/protocol/index.proto"