"use strict";
// tslint:disable:no-console
// this file uses console.log because it is a helper
Object.defineProperty(exports, "__esModule", { value: true });
const glob = require("glob");
const path = require("path");
const path_1 = require("path");
const child_process_1 = require("child_process");
const folders = glob
    .sync(path.resolve(__dirname, '../public/ecs-scenes/*/game.ts'), { absolute: true })
    .map($ => path_1.dirname($));
const PWD = process.cwd();
const env = Object.assign(Object.assign({}, process.env), { AMD_PATH: `${PWD}/node_modules/dcl-amd/dist/amd.js`, ECS_PACKAGE_JSON: `${PWD}/packages/decentraland-ecs/package.json`, ECS_PATH: `${PWD}/packages/decentraland-ecs/dist/src/index.js` });
folders.map($ => {
    child_process_1.spawn('node', [`${PWD}/packages/build-ecs/index.js`, ...process.argv], {
        cwd: $,
        env,
        stdio: 'inherit'
    }).ref();
});
