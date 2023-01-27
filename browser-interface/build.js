#!/usr/bin/env node
const { build, cliopts } = require('estrella')
const { readFileSync, writeFileSync, copyFileSync } = require('fs')
const fs = require('fs')
const path = require('path')
const glob = require('glob')
const { generatedFiles } = require('./package.json')
const { mkdir } = require('fs/promises')

const builtIns = {
  crypto: require.resolve('crypto-browserify'),
  stream: require.resolve('stream-browserify'),
  buffer: require.resolve('./node_modules/buffer/index.js')
}

const PROD = !!process.env.CI

console.log(`production: ${PROD}`)
process.env.BUILD_PATH = path.resolve(
  process.env.BUILD_PATH || path.resolve(__dirname, '../unity-renderer/Builds/unity')
)
const DIST_PATH = path.resolve(__dirname, './static')

async function main() {
  await copyBuiltFiles()
  await createPackageJson()
  await compileJs()
}

// This function copies the built files from unity into the target folder
async function copyBuiltFiles() {
  await mkdir(DIST_PATH, { recursive: true })

  const streamingAssetsDistPath = path.resolve(DIST_PATH, "StreamingAssets")
  await mkdir(streamingAssetsDistPath, { recursive: true })
  const streamingAssetsPath = path.resolve(process.env.BUILD_PATH, "StreamingAssets")
  for (let file of glob.sync("**/*", { cwd: streamingAssetsPath, absolute: true })) {
    copyFile(file, path.resolve(streamingAssetsDistPath, file.replace(streamingAssetsPath + "/", "./")))
  }

  const basePath = path.resolve(process.env.BUILD_PATH, 'Build')

  for (let file of glob.sync('**/*', { cwd: basePath, absolute: true })) {
    copyFile(file, path.resolve(DIST_PATH, file.replace(basePath + '/', './')))
  }
}

/**
 * @returns the resolved absolute path
 */
function ensureFileExists(root, file) {
  const x = path.resolve(root, file.replace(/^\//, ''))

  if (!fs.existsSync(x)) {
    throw new Error(`${x} does not exist`)
  }

  return x
}

function copyFile(from, to) {
  console.log(`> copying ${from} to ${to}`)

  if (!fs.existsSync(from)) {
    throw new Error(`${from} does not exist`)
  }

  // if it is not a file, remove it to avoid conflict with symbolic links
  if (fs.existsSync(to)) {
    const type = fs.lstatSync(to)
    if (!type.isFile()) {
      fs.removeSync(to)
    }
  }

  copyFileSync(from, to)

  if (!fs.existsSync(to)) {
    throw new Error(`${to} does not exist`)
  }
}

async function createPackageJson() {
  console.log('> writing package.json')

  const time = process.env.CIRCLE_SHA1 ? new Date()
    .toISOString()
    .replace(/(\..*$)/g, '')
    .replace(/([^\dT])/g, '')
    .replace('T', '') : '20200220202020'

  const shortCommitHash = (process.env.CIRCLE_SHA1 || '').substring(0, 7)

  writeFileSync(
    path.resolve(DIST_PATH, 'package.json'),
    JSON.stringify(
      {
        name: '@dcl/explorer',
        main: 'index.js',
        typings: 'index.d.ts',
        version: `1.0.${process.env.CIRCLE_BUILD_NUM || '0-development'}-${time}.commit-${shortCommitHash}`,
        tag: process.env.CIRCLE_TAG,
        commit: process.env.CIRCLE_SHA1,
        branch: process.env.CIRCLE_BRANCH,
        author: 'Decentraland Contributors',
        license: 'Apache-2.0',
        publishConfig: {
          access: 'public'
        },
        repository: {
          type: 'git',
          url: 'https://github.com/decentraland/unity-renderer.git'
        }
      },
      null,
      2
    )
  )
}

const workerLoader = () => {
  return {
    name: 'worker-loader',
    setup(plugin) {
      plugin.onResolve({ filter: /(.+)-webworker(?:\.dev)?\.js$/ }, (args) => {
        return { path: args.path, namespace: 'workerUrl' }
      })
      plugin.onLoad({ filter: /(.+)-webworker(?:\.dev)?\.js$/, namespace: 'workerUrl' }, async (args) => {
        const dest = require.resolve(args.path)
        return { contents: `export default ${JSON.stringify(readFileSync(dest).toString())};` }
      })
    }
  }
}

const nodeBuiltIns = () => {
  const include = Object.keys(builtIns)
  if (!include.length) {
    throw new Error('Must specify at least one built-in module')
  }
  const filter = RegExp(`^(${include.join('|')})$`)
  return {
    name: 'node-builtins',
    setup(build) {
      build.onResolve({ filter }, (arg) => ({
        path: builtIns[arg.path]
      }))
    }
  }
}

const commonOptions = {
  bundle: true,
  minify: !cliopts.watch,
  sourcemap: cliopts.watch ? 'both' : undefined,
  sourceRoot: path.resolve('./packages'),
  sourcesContent: !!cliopts.watch,
  treeShaking: true,
  plugins: [nodeBuiltIns(), workerLoader()]
}

function createWorker(entry, outfile) {
  return build({
    ...commonOptions,
    entry,
    outfile,
    tsconfig: path.join(path.dirname(entry), 'tsconfig.json'),
    inject: ['packages/entryPoints/inject.js']
  })
}

async function compileJs() {
  ensureFileExists(DIST_PATH, 'unity.loader.js')
  const injectUnityPath = path.resolve(DIST_PATH, 'unity.loader.js')

  for (let file of Object.values(generatedFiles)) {
    ensureFileExists(DIST_PATH, file)
  }

  if (!process.env.BUNDLES_ONLY) {
    createWorker('packages/gif-processor/worker.ts', 'static/gif-processor/worker.js.txt')
    createWorker('packages/voice-chat-codec/worker.ts', 'static/voice-chat-codec/worker.js.txt')
    createWorker(
      'packages/voice-chat-codec/audioWorkletProcessors.ts',
      'static/voice-chat-codec/audioWorkletProcessors.js.txt'
    )
    createWorker('packages/ui/decentraland-ui.scene.ts', 'static/systems/decentraland-ui.scene.js.txt')
  }

  if (!process.env.ESSENTIALS_ONLY) {
    build({
      ...commonOptions,
      entry: 'packages/entryPoints/index.ts',
      outfile: 'static/index.js',
      tsconfig: 'packages/entryPoints/tsconfig.json',
      inject: ['packages/entryPoints/inject.js'],
      banner: {js: readFileSync(injectUnityPath).toString() }
    })

    build({
      ...commonOptions,
      debug: true,
      clear: true,
      minify: false,
      sourcemap: 'both',
      entry: 'test/index.ts',
      outfile: 'test/out/index.js',
      tsconfig: 'test/tsconfig.json',
      inject: ['packages/entryPoints/inject.js']
    })
  }

  // Run a local web server with livereload when -watch is set
  cliopts.watch && require('./runTestServer')
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
