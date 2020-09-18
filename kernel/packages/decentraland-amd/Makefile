build:
	node_modules/.bin/tsc -p tsconfig.json
	node_modules/.bin/uglifyjs --mangle --comments some --source-map -o dist/amd.js dist/amd.js

watch:
	node_modules/.bin/tsc -p tsconfig.json
	node_modules/.bin/tsc -p tsconfig.json --watch & node_modules/.bin/mocha --watch

test: build
	node_modules/.bin/mocha

.PHONY: build
