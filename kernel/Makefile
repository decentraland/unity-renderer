# General setup

NODE = node
COMPILER = $(NODE) --max-old-space-size=4096 node_modules/.bin/decentraland-compiler
CWD = $(shell pwd)

# Remove default Makefile rules

.SUFFIXES:

# SDK Files

SOURCE_SUPPORT_TS_FILES := $(wildcard scripts/*.ts)
COMPILED_SUPPORT_JS_FILES := $(subst .ts,.js,$(SOURCE_SUPPORT_TS_FILES))

ECS_CONFIG_DEPENDENCIES := packages/decentraland-ecs/package.json packages/decentraland-ecs/tsconfig.json

DECENTRALAND_ECS_SOURCES := $(wildcard packages/decentraland-ecs/src/**/*.ts)
DECENTRALAND_ECS_COMPILED_FILES := packages/decentraland-ecs/dist/src/index.js

DECENTRALAND_ECS_TYPEDEF_FILE := packages/decentraland-ecs/types/dcl/index.d.ts

SCENE_SYSTEM_SOURCES := $(wildcard static/systems/**/*.ts)
SCENE_SYSTEM := static/systems/scene.system.js
DECENTRALAND_LOADER := static/loader/lifecycle/worker.js
GIF_PROCESSOR := static/gif-processor/worker.js
INTERNAL_SCENES := static/systems/decentraland-ui.scene.js
BUILD_ECS := packages/build-ecs/index.js
VOICE_CHAT_CODEC_WORKER := static/voice-chat-codec/worker.js static/voice-chat-codec/audioWorkletProcessors.js

config-contracts:
	@echo 'Updating contracts...'
	@node ./scripts/updateContracts.js
	@echo 'Contracts updated!'

scripts/%.js: $(SOURCE_SUPPORT_TS_FILES) scripts/tsconfig.json config-contracts
	@npx tsc --build scripts/tsconfig.json

packages/build-ecs/%.js: packages/build-ecs/%.ts packages/build-ecs/tsconfig.json
	@npx tsc --build packages/build-ecs/tsconfig.json
	@chmod +x packages/build-ecs/index.js

static/loader/lifecycle/worker.js: packages/decentraland-loader/**/*.ts
	@$(COMPILER) targets/engine/loader.json

static/gif-processor/worker.js: packages/gif-processor/*.ts
	@$(COMPILER) targets/engine/gif-processor.json

static/voice-chat-codec/worker.js: packages/voice-chat-codec/*.ts
	@$(COMPILER) targets/engine/voice-chat-codec.json

static/default-profile/contents:
	@node ./static/default-profile/download_all.js

static/systems/scene.system.js: $(DECENTRALAND_ECS_TYPEDEF_FILE) $(SCENE_SYSTEM_SOURCES) packages/scene-system/scene.system.ts
	@$(COMPILER) targets/engine/scene-system.json

static/systems/decentraland-ui.scene.js: $(SCENE_SYSTEM) packages/ui/tsconfig.json packages/ui/decentraland-ui.scene.ts
	@$(COMPILER) targets/engine/internal-scenes.json

packages/decentraland-ecs/dist/src/index.js: $(DECENTRALAND_ECS_SOURCES) $(ECS_CONFIG_DEPENDENCIES)
	@$(COMPILER) targets/engine/ecs.json

packages/decentraland-ecs/types/dcl/index.d.ts: $(COMPILED_SUPPORT_JS_FILES) $(DECENTRALAND_ECS_COMPILED_FILES) $(ECS_CONFIG_DEPENDENCIES)
	@cd $(PWD)/packages/decentraland-ecs; $(PWD)/node_modules/.bin/api-extractor run --typescript-compiler-folder "$(PWD)/node_modules/typescript" --local
	@node ./scripts/buildEcsTypes.js
	@npx prettier --write 'packages/decentraland-ecs/types/dcl/index.d.ts'
	@touch $(DECENTRALAND_ECS_TYPEDEF_FILE)

docs: $(DECENTRALAND_ECS_TYPEDEF_FILE) ## Generate `decentraland-ecs` documentation using @microsoft/api-documenter
	@cd $(PWD)/packages/decentraland-ecs; $(PWD)/node_modules/.bin/api-documenter markdown -i types/dcl -o docs
	@echo "\nAPI Documentation was generated in the packages/decentraland-ecs/docs folder"

RENDERER_BUILD := static/unity/Build/unity.data.unityweb static/unity/Build/unity.wasm.framework.unityweb static/unity/Build/unity.wasm.code.unityweb

static/unity/Build/%.unityweb: package.json package-lock.json
	@npm install
	@cp node_modules/decentraland-renderer/*.unityweb static/unity/Build/

empty-parcels:
	cd static/loader/empty-scenes && node generate_all.js

build-essentials: $(COMPILED_SUPPORT_JS_FILES) dcl-amd $(DECENTRALAND_ECS_TYPEDEF_FILE) $(BUILD_ECS) $(SCENE_SYSTEM) $(INTERNAL_SCENES) $(DECENTRALAND_LOADER) $(GIF_PROCESSOR) $(VOICE_CHAT_CODEC_WORKER) $(RENDERER_BUILD) empty-parcels generate-mocks ## Build the basic required files for the explorer

# Hellmap scenes

HELLMAP_SOURCE_FILES := $(wildcard public/hell-map/*/game.ts)
HELLMAP_GAMEJS_FILES := $(subst .ts,.js,$(HELLMAP_SOURCE_FILES))

public/hell-map/%/game.js: $(SCENE_SYSTEM) public/hell-map/%/game.ts
	@$(COMPILER) targets/scenes/hell-map.json

# Test scenes

TEST_SCENES_SOURCE_FILES := $(wildcard public/test-scenes/*/game.ts)
TEST_SCENES_GAMEJS_FILES := $(subst .ts,.js,$(TEST_SCENES_SOURCE_FILES))

public/test-scenes/%/game.js: $(SCENE_SYSTEM) public/test-scenes/%/game.ts
	@$(COMPILER) targets/scenes/test-scenes.json

TEST_ECS_SCENE_SOURCES := $(wildcard public/ecs-scenes/*/game.ts)
TEST_ECS_SCENE_GAMEJS_FILES := $(subst .ts,.js,$(TEST_ECS_SCENE_SOURCES))

watch-only-test-scenes:
	@$(COMPILER) targets/scenes/test-scenes.json --watch

# ECS scenes

ECS_LIBRARY := public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper/eth-wrapper.js

public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper/eth-wrapper.js: public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper/eth-wrapper.ts $(BUILD_ECS)
	$(BUILD_ECS) -p public/ecs-scenes/-200.-30-libraries/node_modules/eth-wrapper

public/ecs-scenes/%/game.js: $(ECS_LIBRARY) $(SCENE_SYSTEM) public/ecs-scenes/%/game.ts
	@node scripts/buildECSprojects.js

# All scenes together

ecs-scenes: $(TEST_ECS_SCENE_GAMEJS_FILES) $(BUILD_ECS)
	$(MAKE) generate-mocks

test-scenes: $(TEST_SCENES_GAMEJS_FILES) $(HELLMAP_GAMEJS_FILES) $(TEST_ECS_SCENE_GAMEJS_FILES) ## Build the test scenes
	$(MAKE) generate-mocks

# Entry points

static/dist/%.js: build-essentials packages/entryPoints/%.ts
	@$(COMPILER) $(word 2,$^)

# Release

DIST_ENTRYPOINTS := static/dist/editor.js static/dist/preview.js static/dist/website.js
DIST_STATIC_FILES := static/export.html static/preview.html static/fonts static/images static/unity static/default-profile/contents
DIST_PACKAGE_JSON := packages/decentraland-ecs/package.json

build-deploy: $(DIST_ENTRYPOINTS) $(DIST_STATIC_FILES) $(SCENE_SYSTEM) $(INTERNAL_SCENES) ## Build all the entrypoints needed for a deployment
	@node ./scripts/replaceVersion.js

build-release: $(DIST_ENTRYPOINTS) $(DIST_STATIC_FILES) $(DIST_PACKAGE_JSON) ## Build all the entrypoints and run the `scripts/prepareDist` script
	@node ./scripts/prepareDist.js

publish: | build-release update-bundled-dependency ## Release a new version, using the `scripts/npmPublish` script
	@cd $(PWD)/packages/decentraland-ecs; node $(PWD)/scripts/npmPublish.js
	@cd $(PWD)/packages/build-ecs; node $(PWD)/scripts/npmPublish.js
	@cd $(PWD)/static; node $(PWD)/scripts/npmPublish.js

update-bundled-dependency: # updates static/package.json devDependencies to match the bundled decentraland-renderer version
	@cd $(PWD)/static; npm install --save-dev --save-bundle $(shell npm list --depth 0 | grep -Po --color=never "decentraland-renderer.*")

# Testing

TEST_SOURCE_FILES := $(wildcard test/**/*.ts)

test/out/index.js: build-essentials $(TEST_SOURCE_FILES)
	@$(COMPILER) ./targets/test.json

test: build-essentials test-scenes test/out/index.js ## Run all the tests
	$(MAKE) generate-mocks
	@node scripts/runTestServer.js

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

test-ci: # Run the tests (for use in the continuous integration environment)
	@SINGLE_RUN=true NODE_ENV=production $(MAKE) test
	@node_modules/.bin/nyc report --temp-directory ./test/tmp --reporter=html --reporter=lcov --reporter=text

generate-images: ## Generate the screenshots to run the visual diff validation tests
	@docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		-e GENERATE_NEW_IMAGES=true \
		circleci/node:10-browsers \
			make test

public/local-ipfs/mappings:
	@rm -rf ./public/local-ipfs
	@node ./scripts/createMockJson.js

generate-mocks: ./scripts/createMockJson.js public/local-ipfs/mappings ## Build a fake "IPFS" index of all the test scene mappings
	@rm -rf ./public/local-ipfs
	@node ./scripts/createMockJson.js

PARCEL_SCENE_JSONS := $(wildcard public/test-scenes/*/scene.json)
ECS_SCENE_JSONS := $(wildcard public/ecs-scenes/*/scene.json)
HELLMAP_SCENE_JSONS := $(wildcard public/hell-map/*/scene.json)
SCENE_JSONS := $(PARCEL_SCENE_JSONS) $(ECS_SCENE_JSONS) $(HELLMAP_SCENE_JSONS)

# CLI

dcl-amd:
	cd $(CWD)/packages/decentraland-amd; npm install; make build
	mkdir -p packages/decentraland-ecs/artifacts
	ln -sf $(CWD)/packages/decentraland-amd/dist/amd.js packages/decentraland-ecs/artifacts/amd.js
	ln -sf $(CWD)/packages/decentraland-amd/dist/amd.min.js packages/decentraland-ecs/artifacts/amd.min.js
	ln -sf $(CWD)/packages/decentraland-amd/dist/amd.min.js.map packages/decentraland-ecs/artifacts/amd.min.js.map

npm-link: build-essentials static/dist/preview.js static/dist/editor.js dcl-amd ## Run `npm link` to develop local scenes against this project
	rm -rf packages/decentraland-ecs/artifacts || true
	mkdir packages/decentraland-ecs/artifacts
	ln -sf $(CWD)/packages/decentraland-amd/dist/amd.js packages/decentraland-ecs/artifacts/amd.js
	ln -sf $(CWD)/packages/decentraland-amd/dist/amd.min.js packages/decentraland-ecs/artifacts/amd.min.js
	ln -sf $(CWD)/packages/decentraland-amd/dist/amd.min.js.map packages/decentraland-ecs/artifacts/amd.min.js.map
	ln -sf $(CWD)/packages/build-ecs/index.js packages/decentraland-ecs/artifacts/build-ecs.js
	ln -sf $(CWD)/static/dist/preview.js packages/decentraland-ecs/artifacts/preview.js
	ln -sf $(CWD)/static/dist/unityPreview.js packages/decentraland-ecs/artifacts/unityPreview.js
	ln -sf $(CWD)/static/dist/editor.js packages/decentraland-ecs/artifacts/editor.js
	ln -sf $(CWD)/static/preview.html packages/decentraland-ecs/artifacts/preview.html
	ln -sf $(CWD)/static/fonts packages/decentraland-ecs/artifacts/fonts
	ln -sf $(CWD)/static/images packages/decentraland-ecs/artifacts/images
	ln -sf $(CWD)/static/unity packages/decentraland-ecs/artifacts/unity
	ln -sf $(CWD)/static/default-profile packages/decentraland-ecs/artifacts/default-profile
	ln -sf $(CWD)/static/unity-preview.html packages/decentraland-ecs/artifacts/unity-preview.html
	cd packages/decentraland-ecs; npm link
	cd static; npm link

watch-builder: build-essentials static/dist/editor.js ## Watch the files required for hacking with the builder
	@node_modules/.bin/concurrently \
		-n "ecs,scene-system,internal-scenes,loader,preview,unity,builder,server" \
			"$(COMPILER) targets/engine/ecs.json --watch" \
			"$(COMPILER) targets/engine/scene-system.json --watch" \
			"$(COMPILER) targets/engine/internal-scenes.json --watch" \
			"$(COMPILER) targets/engine/loader.json --watch" \
			"$(COMPILER) targets/entryPoints/editor.json --watch" \
			"node ./scripts/runTestServer.js --keep-open"

watch-cli: build-essentials ## Watch the files required for building the CLI
	@node_modules/.bin/concurrently \
		-n "ecs,scene-system,internal-scenes,loader,preview,website,server" \
			"$(COMPILER) targets/engine/ecs.json --watch" \
			"$(COMPILER) targets/engine/scene-system.json --watch" \
			"$(COMPILER) targets/engine/internal-scenes.json --watch" \
			"$(COMPILER) targets/engine/loader.json --watch" \
			"$(COMPILER) targets/entryPoints/preview.json --watch" \
			"$(COMPILER) targets/entryPoints/website.json --watch" \
			"node ./scripts/runTestServer.js --keep-open"

# Aesthetics

lint: ## Validate correct formatting and circular dependencies
	@node_modules/.bin/madge packages/entryPoints/website.ts --circular --warning
	@node_modules/.bin/madge packages --orphans --extensions ts --exclude '.+\.d.ts|.+/dist\/.+'
	@node_modules/.bin/tslint --project tsconfig.json

lint-fix: ## Fix bad formatting on all .ts and .tsx files
	@node_modules/.bin/tslint --project tsconfig.json --fix
	@node_modules/.bin/prettier --write 'packages/**/*.{ts,tsx}'

# Development

watch: $(SOME_MAPPINGS) build-essentials static/dist/website.js ## Watch the files required for hacking the explorer
	cd ../website && npm run start &
	cd $(PWD);
	@NODE_ENV=development npx concurrently \
		-n "ecs,scene-system,internal-scenes,loader,basic-scenes,website,test,simulator,server" \
			"$(COMPILER) targets/engine/ecs.json --watch" \
			"$(COMPILER) targets/engine/scene-system.json --watch" \
			"$(COMPILER) targets/engine/internal-scenes.json --watch" \
			"$(COMPILER) targets/engine/loader.json --watch" \
			"$(COMPILER) targets/scenes/basic-scenes.json --watch" \
			"$(COMPILER) targets/entryPoints/website.json --watch" \
			"$(COMPILER) targets/test.json --watch" \
			"node ./scripts/runPathSimulator.js" \
			"node ./scripts/runTestServer.js --keep-open"

update-renderer: ## Update the renderer to the latest version
	@rm -rf node_modules/decentraland-renderer
	@rm -f static/unity/Build/*.unityweb
	@npm install decentraland-renderer@latest

fetchSceneContents: scripts/fetchSceneContents.js
	@node ./scripts/fetchSceneContents.js

clean: ## Clean all generated files
	@$(COMPILER) targets/clean.json

BRANCH := $(shell git rev-parse --abbrev-ref HEAD)

update-renderer-branch: ## Update the renderer using the files generated by CI for the current branch
	@curl --output static/unity/Build/unity.data.unityweb https://play.decentraland.zone/branch/$(BRANCH)/unity/Build/unity.data.unityweb
	@curl --output static/unity/Build/unity.wasm.code.unityweb https://play.decentraland.zone/branch/$(BRANCH)/unity/Build/unity.wasm.code.unityweb
	@curl --output static/unity/Build/unity.wasm.framework.unityweb https://play.decentraland.zone/branch/$(BRANCH)/unity/Build/unity.wasm.framework.unityweb

# Makefile

.PHONY: help docs clean watch watch-builder watch-cli lint lint-fix generate-images test-ci test-docker update build-essentials build-deploy build-release update-renderer config-contracts update-renderer-branch
.DEFAULT_GOAL := help
help:
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}'
	@echo "\nYou probably want to run 'make watch' or 'make test-scenes watch' to build all the test scenes and run the local comms server."
