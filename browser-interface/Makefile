# General setup
NODE = node

CWD = $(shell pwd)
# Remove default Makefile rules

.SUFFIXES:

# SDK Files
EMPTY_SCENES := public/empty-scenes/common

build: empty-parcels
	echo 'declare module "env" {}' > node_modules/env.d.ts
	echo 'declare module "dcl" {}' > node_modules/dcl.d.ts
# Prevent error on `estrella` compatibility with typescript 4.9+ & @types/node 18.0+
	sed ${SED_OPTION} 's/interface FileWriteOptionsObj .*/interface FileWriteOptionsObj {/g' node_modules/estrella/estrella.d.ts || true
	ESSENTIALS_ONLY=true node ./build.js
	BUNDLES_ONLY=true node ./build.js

empty-parcels:
	cd public/empty-scenes/common && (node generate_all.js || true)
	mkdir -p static/loader/empty-scenes || true
	rm -rf static/loader/empty-scenes/*
	cp $(EMPTY_SCENES)/mappings.json static/loader/empty-scenes/mappings.json
	cp -R $(EMPTY_SCENES)/contents static/loader/empty-scenes/contents

SED_OPTION = -i
ifeq ($(shell uname),Darwin)
SED_OPTION= -i ""
endif



# Entry points
static/index.js:
	@node ./build.js

# Testing

TEST_SOURCE_FILES := $(wildcard test/**/*.ts)

test: build ## Run all the tests
	@node ./runTestServer.js

test-ci: touch-local # Run the tests (for use in the continuous integration environment)
	@SINGLE_RUN=true NODE_ENV=production $(MAKE) test

npm-link: build ## Run `npm link` to develop local scenes against this project
	cd static; npm link

# Aesthetics

lint: ## Validate correct formatting and circular dependencies
	@node_modules/.bin/madge packages/entryPoints/index.ts --circular --warning
	@node_modules/.bin/madge packages --orphans --extensions ts --exclude '.+\.d.ts|.+/dist\/.+'
	@node_modules/.bin/eslint . --ext .ts

lint-fix: ## Fix bad formatting on all .ts and .tsx files
	@node_modules/.bin/eslint . --ext .ts --fix

# Development

# touch local creates "stub" files to mock the wasm build of unity and not break the build
touch-local:
	touch static/unity.framework.js
	touch static/unity.loader.js
	touch static/unity.data
	touch static/unity.wasm

build-unity-local:
	npm i --no-save @dcl/explorer@next
	cp node_modules/\@dcl/explorer/unity* static/
	cp -r node_modules/\@dcl/explorer/StreamingAssets static/
watch: touch-local build ## Watch the files required for hacking the explorer
	@NODE_ENV=development node ./build.js -watch

# Makefile

.PHONY: help docs clean watch watch-builder watch-cli lint lint-fix test-ci update build madge
.DEFAULT_GOAL := help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo "\nYou probably want to run 'make watch' to build all the test scenes and run the local comms server."
