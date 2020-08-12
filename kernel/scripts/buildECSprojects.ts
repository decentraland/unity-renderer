// tslint:disable:no-console
// this file uses console.log because it is a helper

import glob = require('glob')
import path = require('path')
import { dirname } from 'path'
import { spawn } from 'child_process'

const folders = glob
  .sync(path.resolve(__dirname, '../public/ecs-scenes/*/game.ts'), { absolute: true })
  .map(($) => dirname($))

const PWD = process.cwd()

const env = {
  ...process.env,
  AMD_PATH: `${PWD}/packages/decentraland-amd/dist/amd.js`,
  ECS_PACKAGE_JSON: `${PWD}/packages/decentraland-ecs/package.json`,
  ECS_PATH: `${PWD}/packages/decentraland-ecs/dist/src/index.js`
}

folders.map(($) => {
  spawn('node', [`${PWD}/packages/build-ecs/index.js`, ...process.argv], {
    cwd: $,
    env,
    stdio: 'inherit'
  }).ref()
})
