# General setup
NODE = node
CONCURRENTLY = node_modules/.bin/concurrently

CWD = $(shell pwd)
# Remove default Makefile rules

.SUFFIXES:

# SDK Files
EMPTY_SCENES := public/empty-scenes/common

empty-parcels:
	cd public/empty-scenes/common && (node generate_all.js || true)
	mkdir -p static/loader/empty-scenes || true
	rm -rf static/loader/empty-scenes/*
	cp $(EMPTY_SCENES)/mappings.json static/loader/empty-scenes/mappings.json
	cp -R $(EMPTY_SCENES)/contents static/loader/empty-scenes/contents

build-essentials: empty-parcels
	echo 'declare module "env" {}' > node_modules/env.d.ts
	echo 'declare module "dcl" {}' > node_modules/dcl.d.ts
	ESSENTIALS_ONLY=true node ./build.js
	BUNDLES_ONLY=true node ./build.js

# Entry points
static/index.js:
	@node ./build.js

# Release

DIST_ENTRYPOINTS := static/index.js
DIST_STATIC_FILES := static/export.html static/preview.html

build-deploy: $(DIST_ENTRYPOINTS) $(DIST_STATIC_FILES) ## Build all the entrypoints needed for a deployment

build-release: $(DIST_ENTRYPOINTS) $(DIST_STATIC_FILES) $(DIST_PACKAGE_JSON)

# Testing

TEST_SOURCE_FILES := $(wildcard test/**/*.ts)

test: build-essentials ## Run all the tests
	@node ./runTestServer.js

test-docker: ## Run all the tests using a docker container
	@docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		-p 8080:8080 \
		circleci/node:10-browsers \
			make test

test-ci: touch-local # Run the tests (for use in the continuous integration environment)
	@SINGLE_RUN=true NODE_ENV=production $(MAKE) test

npm-link: build-essentials ## Run `npm link` to develop local scenes against this project
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

watch: touch-local build-essentials static/index.js ## Watch the files required for hacking the explorer
	@NODE_ENV=development ./build.js -watch

update-renderer:  ## Update the renderer
	npm install @dcl/unity-renderer@latest

# Makefile

.PHONY: help docs clean watch watch-builder watch-cli lint lint-fix test-ci test-docker update build-essentials build-deploy build-release update-renderer madge
.DEFAULT_GOAL := help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo "\nYou probably want to run 'make watch' to build all the test scenes and run the local comms server."
