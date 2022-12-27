#!/usr/bin/env node
const { build, cliopts } = require('estrella')
const { readFileSync } = require('fs')
const path = require('path')

const builtIns = {
  crypto: require.resolve('crypto-browserify'),
  stream: require.resolve('stream-browserify'),
  buffer: require.resolve('./node_modules/buffer/index.js')
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
    inject: ['packages/entryPoints/inject.js']
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
