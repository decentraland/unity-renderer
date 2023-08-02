#!/usr/bin/env node
const { build, cliopts } = require('estrella')
const { readFileSync, writeFileSync, copyFileSync, mkdirSync } = require('fs')
const fs = require('fs')
const path = require('path')
const glob = require('glob')
const { mkdir } = require('fs/promises')
const { exec } = require('node:child_process');
const fse = require('fs-extra');


const builtIns = {
  crypto: require.resolve('crypto-browserify'),
  stream: require.resolve('stream-browserify'),
  buffer: require.resolve('./node_modules/buffer/index.js')
}

const PROD = !!process.env.CI

console.log(`production: ${PROD}`)
process.env.BUILD_PATH = path.resolve(
  process.env.BUILD_PATH || path.resolve(__dirname, '../Builds/unity')
)
const DIST_PATH = path.resolve(__dirname, './static')

async function main() {
  await buildRendererProtocol()
  await copyBuiltFiles()
  await createPackageJson()
  await compileJs()
}

// This function copies the built files from unity into the target folder
async function copyBuiltFiles() {
  await mkdir(DIST_PATH, { recursive: true })

  const basePath = path.resolve(process.env.BUILD_PATH, 'Build')

  for (const file of glob.sync('**/*', { cwd: basePath, absolute: true })) {
    copyFile(file, path.resolve(DIST_PATH, file.replace(basePath + '/', './')))
  }

  const streamingPath = path.resolve(process.env.BUILD_PATH, 'StreamingAssets')
  const streamingDistPath = path.resolve(DIST_PATH, 'StreamingAssets')

  await mkdir(streamingDistPath, { recursive: true })

  for (const file of glob.sync('**/*', { cwd: streamingPath, absolute: true })) {
    copyFile(file, path.resolve(streamingDistPath, file.replace(streamingPath + '/', './')))
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

  const type = fs.lstatSync(from)
  if (type.isFile()) {
    copyFileSync(from, to)
  } else {
    mkdirSync(to, { recursive: true })
  }

  if (!fs.existsSync(to)) {
    throw new Error(`${to} does not exist`)
  }
}

async function execute(
  command,
  workingDirectory,
) {
  return new Promise((onSuccess, onError) => {
    exec(command, { cwd: workingDirectory }, (error, stdout, stderr) => {
      stdout.trim().length &&
        console.log('stdout:\n' + stdout.replace(/\n/g, '\n  '))
      stderr.trim().length &&
        console.error('stderr:\n' + stderr.replace(/\n/g, '\n  '))

      if (error) {
        onError(stderr)
      } else {
        onSuccess(stdout)
      }
    })
  })
}

async function buildRendererProtocol() {
  console.log('> Building renderer protocol')
  try {
    fse.removeSync('packages/shared/protocol/')
    fse.mkdirSync('packages/shared/protocol/')

    // Merge renderer-protocol to @dcl/protocol into a single folder
    // `dereference: true` to avoid error when `@dcl/protocol` is linked
    fse.copySync('./node_modules/@dcl/protocol', './protocol-temp/', { overwrite: false, dereference: true })
    fse.copySync('../renderer-protocol/', './protocol-temp/', { overwrite: false })

    // Generate the protocol files
    const protocolPathPublic = path.resolve(__dirname, './protocol-temp/public')
    const protocolPathProto = path.resolve(__dirname, './protocol-temp/proto')

    // Get all files to generate
    const files = glob.sync('**/*.proto', { cwd: protocolPathPublic, absolute: true }).join(' ')

    // Prepare the command
    const command = `
    node_modules/.bin/protoc \
    --plugin=./node_modules/.bin/protoc-gen-ts_proto \
    --ts_proto_opt=esModuleInterop=true,returnObservable=false,outputServices=generic-definitions,fileSuffix=.gen,oneof=unions \
    --ts_proto_out="packages/shared/protocol/" \
    --proto_path=${protocolPathPublic} \
    --proto_path=${protocolPathProto} \
    ${files};
    `
    console.log(`> Building renderer protocol - ${command}`)

    await execute(command, __dirname)
  } finally {
    if (fse.existsSync('protocol-temp/'))
      fse.removeSync('protocol-temp/')
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
  sourcemap: 'external',
  sourceRoot: path.resolve('./packages'),
  sourcesContent: true,
  treeShaking: true,
  plugins: [nodeBuiltIns(), workerLoader()]
}

function createWorker(entry, outfile) {
  return build({
    ...commonOptions,
    entry,
    outfile,
    tsconfig: path.join(path.dirname(entry), 'tsconfig.json'),
    inject: ['packages/entryPoints/inject.js'],
    sourcemap: true
  })
}

async function compileJs() {
  const injectUnityPath = path.resolve(__dirname, 'static', 'unity.loader.js')

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
      banner: {js: readFileSync(injectUnityPath).toString() },
      sourcemap: true
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
