NODE = @node
COMPILER = $(NODE) --max-old-space-size=4096 node_modules/.bin/decentraland-compiler
PARALLEL_COMPILER = node --max-old-space-size=4096 node_modules/.bin/decentraland-compiler

DCL_PROJECT=../scenes/test

GREEN=\n\033[1;34m
RED=\n\033[1;31m
RESET=\033[0m

clean:
	@echo "$(GREEN)===================== Cleaning project =====================$(RESET)"
	$(COMPILER) build.clean.json

build: | clean compile
	@echo "$(GREEN)===================== Compilation Done =====================$(RESET)"

compile: | build-test-scenes generate-mocks compile-entry-points

compile-entry-points: | build-sdk
	@echo "$(GREEN)================== Compiling entry points ==================$(RESET)"
	$(COMPILER) build.entryPoints.json

compile-dev:
	NODE_ENV=development $(MAKE) compile

build-sdk: build-support
	@echo "$(GREEN)======================= Building SDK =======================$(RESET)"
	$(COMPILER) build.sdk.json
	cd $(PWD)/packages/decentraland-ecs; $(PWD)/node_modules/.bin/api-extractor run --typescript-compiler-folder "$(PWD)/node_modules/typescript" --local
	node ./scripts/buildEcsTypes.js
	@echo "$(GREEN)======================= Updating docs ======================$(RESET)"
	cd $(PWD)/packages/decentraland-ecs; $(PWD)/node_modules/.bin/api-documenter markdown -i types/dcl -o docs
	cd $(PWD)/packages/decentraland-ecs; $(PWD)/node_modules/.bin/api-documenter yaml -i types/dcl -o docs-yaml

build-support:
	@echo "$(GREEN)================== Building support files ==================$(RESET)"
	$(COMPILER) build.support.json

build-test-scenes: build-sdk
	@echo "$(GREEN)=================== Building test scenes ===================$(RESET)"
	$(COMPILER) build.test-scenes.json
	node scripts/buildECSprojects.js

export-preview: | clean compile-entry-points
	@echo "$(GREEN)============== Exporting Preview File to CLI ===============$(RESET)"
	cp static/dist/preview.js ${DCL_PROJECT}/node_modules/decentraland-ecs/artifacts/

publish:
	@echo "$(GREEN)=================== getting release ready ==================$(RESET)"
	$(NODE) ./scripts/prepareDist.js

	@echo "$(GREEN)===================== decentraland-ecs =====================$(RESET)"
	@cd $(PWD)/packages/decentraland-ecs; node $(PWD)/scripts/npmPublish.js

	@echo "$(GREEN)======================== build-ecs =========================$(RESET)"
	@cd $(PWD)/packages/build-ecs; node $(PWD)/scripts/npmPublish.js

test: compile-dev
	@echo "$(GREEN)================= Running development tests ================$(RESET)"
	node scripts/test.js

test-docker:
	docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		circleci/node:10-browsers \
	 		node ./scripts/createMockJson.js
	docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		circleci/node:10-browsers \
	 		node ./scripts/test.js

test-ci:
	NODE_ENV=production $(MAKE) compile
	@echo "$(GREEN)================= Running production tests =================$(RESET)"
	@echo "$(RED)Skipped!$(RESET)"
	# SINGLE_RUN=true node ./scripts/test.js
	# node_modules/.bin/nyc report --temp-directory ./test/tmp --reporter=html --reporter=lcov --reporter=text

generate-images-local: compile-dev
	@echo "$(GREEN)================== Generating test images ==================$(RESET)"
	node scripts/test.js

generate-images:
	docker run \
		-it \
		--rm \
		--name node \
		-v "$(PWD):/usr/src/app" \
		-w /usr/src/app \
		-e SINGLE_RUN=true \
		-e GENERATE_NEW_IMAGES=true \
		-p 8080:8080 \
		circleci/node:10-browsers \
			make generate-images-local
	$(MAKE) generate-mocks

generate-mocks:
	$(NODE) ./scripts/createMockJson.js

lint:
	node_modules/.bin/madge packages/entryPoints/index.ts --circular --warning
	node_modules/.bin/madge packages --orphans --extensions ts --exclude '.+\.d.ts|.+/dist\/.+'
	node_modules/.bin/tslint --project tsconfig.json

lint-fix:
	node_modules/.bin/tslint --project tsconfig.json --fix

watch: compile-dev
	@echo "$(GREEN)=================== Watching file changes ==================$(RESET)"
	$(MAKE) only-watch

only-watch:
	@node_modules/.bin/concurrently \
		-n "sdk,test-scenes,entryPoints,ecs-builder,server" \
			"$(PARALLEL_COMPILER) build.sdk.json --watch" \
			"$(PARALLEL_COMPILER) build.test-scenes.json --watch" \
			"$(PARALLEL_COMPILER) build.entryPoints.json --watch" \
			"node ./scripts/buildECSprojects.js --watch" \
			"node ./scripts/test.js --keep-open"

dev-watch:
	@node_modules/.bin/concurrently \
		-n "sdk,entryPoints,ecs-builder,server" \
			"$(PARALLEL_COMPILER) build.entryPoints.json --watch" \
			"node ./scripts/test.js --keep-open"
