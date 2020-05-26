#!/usr/bin/env node

// This started as something different as it is right now. It became gulp.

import * as webpack from 'webpack'
import { ConcatSource } from 'webpack-sources'
import * as globPkg from 'glob'
import * as rimraf from 'rimraf'
import * as fs from 'fs'
import { resolve, parse as parsePath, dirname, basename, relative } from 'path'
const TsconfigPathsPlugin = require('tsconfig-paths-webpack-plugin')
import { spawn } from 'child_process'
import { tmpdir } from 'os'
import ProgressBar = require('progress')
import chalk from 'chalk'

const packageJson = JSON.parse(fs.readFileSync(require.resolve('../package.json')).toString())
const isWatching = process.argv.some($ => $ === '--watch')
const instrumentCoverage = process.argv.some($ => $ === '--coverage') || process.env.NODE_ENV === 'coverage'
const isProduction = process.env.NODE_ENV !== 'development' && !isWatching && !instrumentCoverage
const webWorkerTransport = resolve(__dirname, '../lib/common/transports/WebWorker')

const entryPointWebWorker = (filename: string) => `
import { WebWorkerTransport } from ${JSON.stringify(webWorkerTransport)}
const imported = require(${JSON.stringify(filename)})

if (imported && imported.__esModule && imported['default']) {
  new imported['default'](WebWorkerTransport(self))
}
`

console.log('decentraland-compiler version: ' + chalk.green(packageJson.version))

export function findConfigFile(baseDir: string, configFileName: string): string | null {
  let configFilePath = resolve(baseDir, configFileName)

  if (fs.existsSync(configFilePath)) {
    return configFilePath
  }

  if (baseDir.length === dirname(baseDir).length) {
    return null
  }

  return findConfigFile(resolve(baseDir, '../'), configFileName)
}

export interface ICompilerOptions {
  files: string[]
  outDir: string
  tsconfig: string
  target?: 'web' | 'webworker' | 'node' | 'esm' | 'this'
  library?: string
  coverage?: boolean
  rootFolder: string
  fileName?: string
  globalObject?: string
}

class ESModulePlugin {
  constructor(
    public options: {
      exportedMember: string
    }
  ) {}

  apply(compiler: webpack.Compiler) {
    compiler.hooks.compilation.tap('ESModulePlugin', compilation => {
      compilation.hooks.afterOptimizeChunkAssets.tap('ESModulePlugin', chunks => {
        for (const chunk of chunks) {
          if (!chunk.canBeInitial()) {
            continue
          }

          for (const file of chunk.files) {
            compilation.assets[file] = new ConcatSource(
              compilation.assets[file],
              '\n',
              `export default ${this.options.exportedMember};`
            )
          }
        }
      })
    })
  }
}

export async function compile(opt: ICompilerOptions) {
  return new Promise<webpack.Stats>((onSuccess, onError) => {
    let entry: webpack.Entry | string[] = opt.files

    const extensions = ['.ts', '.tsx', '.js', '.json']

    if (opt.target === 'webworker') {
      entry = entry.map($ => {
        const file = resolve(tmpdir(), Math.random().toString() + '.WebWorker.js')
        fs.writeFileSync(file, entryPointWebWorker($))
        return file
      })
    }

    entry = entry.reduce(
      (obj, $, $$) => {
        let name = relative(opt.rootFolder, opt.files[$$])
        extensions.forEach($ => {
          if (name.endsWith($)) {
            name = name.substr(0, name.length - $.length)
          }
        })
        let target = name
        if (target.endsWith('.js')) {
          target = target.substr(0, target.length - 3)
        }
        obj[target] = $
        return obj
      },
      {} as webpack.Entry
    )

    console.log(
      [
        `     files:`,
        ...Object.keys(entry).map(
          ($, $$) => `            (root)/${relative(opt.rootFolder, opt.files[$$])} -> (outDir)/${$}.js`
        )
      ].join('\n')
    )

    const libraryName = opt.library || undefined
    const plugins = [ProgressBarPlugin({})]
    let libraryTarget: any = 'umd'
    let target: webpack.Configuration['target'] = 'web'

    if (opt.target === 'esm') {
      target = 'web'
    } else if (opt.target === 'this') {
      target = 'webworker'
    } else {
      target = opt.target
    }

    if (opt.target === 'this') {
      libraryTarget = 'this'
    } else if (opt.target === 'webworker') {
      libraryTarget = 'this'
    } else if (opt.target === 'esm') {
      libraryTarget = 'var'
      if (libraryName) {
        plugins.push(new ESModulePlugin({ exportedMember: libraryName }))
      }
    }

    const options: webpack.Configuration = {
      entry,
      mode: isProduction ? 'production' : 'development',
      optimization: {
        nodeEnv: isProduction ? 'production' : 'development',
        namedModules: !isProduction,
        minimize: isProduction
      },
      output: {
        filename: opt.fileName,
        path: opt.outDir,
        libraryTarget
      },

      resolve: {
        // Add '.ts' and '.tsx' as resolvable extensions.
        extensions,
        plugins: [new TsconfigPathsPlugin({ configFile: opt.tsconfig })]
      },
      watch: isWatching,
      module: {
        rules: [
          {
            test: /\.(jpe?g|png|gif|svg)$/i,
            use: [
              {
                loader: require.resolve('url-loader'),
                options: {
                  limit: 512000
                }
              }
            ]
          },
          // All files with a '.ts' or '.tsx' extension will be handled by 'awesome-typescript-loader'.
          {
            test: /\.tsx?$/,
            loader: require.resolve('ts-loader'),
            options: {
              configFile: opt.tsconfig
            }
          }
        ]
      },
      target,
      plugins
    }

    if (opt.coverage) {
      // tslint:disable-next-line:semicolon
      ;(options.module as any).rules.push({
        test: /\.[jt]sx?$/,
        use: {
          loader: 'istanbul-instrumenter-loader',
          options: { esModules: true, sourceMaps: true }
        },
        enforce: 'post',
        exclude: /node_modules|\.spec\.js$/
      })
    }

    if (libraryName) {
      options.output!.library = libraryName
    }

    if (opt.globalObject) {
      options.output!.globalObject = opt.globalObject
    }

    const compiler = webpack(options)

    if (!isWatching) {
      compiler.run((err, stats) => {
        if (err) {
          onError(err)
        } else {
          onSuccess(stats)
        }
      })
    } else {
      compiler.watch({ ignored: /node_modules/, aggregateTimeout: 1000 }, (err, stats) => {
        if (stats.hasErrors() || stats.hasWarnings()) {
          console.log(
            stats.toString({
              colors: true,
              errors: true,
              warnings: true
            })
          )
        } else {
          console.log('OK ' + opt.outDir)
        }

        if (!err) {
          onSuccess(stats)
        }
      })
    }
  })
}

export async function tsc(tsconfig: string) {
  const tscLocation = require.resolve('typescript/lib/tsc')

  console.log(
    `
    Executing "tsc -p ${basename(tsconfig)}" in ${dirname(tsconfig)}
  `.trim()
  )

  const args = [tscLocation, '-p', basename(tsconfig)]

  if (isWatching) {
    args.push('--watch')
  }

  const childProcess = spawn('node', args, {
    cwd: dirname(tsconfig)
  })

  if (isWatching) {
    return true
  }

  let resolve: (x: any) => any = a => void 0
  let reject: (x: any) => any = a => void 0

  const semaphore = new Promise((ok, err) => {
    resolve = ok
    reject = err
  })

  childProcess.stdout.on('data', data => {
    console.log(`tsc stdout: ${data}`)
  })

  childProcess.stderr.on('data', data => {
    console.log(`tsc stderr: ${data}`)
  })

  childProcess.on('close', exitCode => {
    if (exitCode) {
      reject(exitCode)
    } else {
      resolve(exitCode)
    }
  })

  await semaphore
}

export async function processFile(opt: {
  file?: string
  files?: string[]
  outFile?: string
  watch?: boolean
  target?: string
  coverage?: boolean
  library?: string
  fileName?: string
  globalObject?: string
}) {
  const baseFiles = (opt.file && [opt.file]) || (opt.files && opt.files[0]) || []

  if (!baseFiles.length) {
    throw new Error(`Unable to find a file to compile`)
  }

  const baseFile = baseFiles[0]

  if (baseFile.endsWith('.json')) {
    return processJson(baseFile)
  }

  const parsed = parsePath(baseFile)

  const configFile = findConfigFile(dirname(baseFile), 'tsconfig.json')

  if (!configFile) {
    throw new Error(`Unable to find a tsconfig.json file for ${opt.file}`)
  }

  const rootFolder = dirname(configFile)

  const parsedTsConfig = require(configFile)

  let outFile = opt.outFile
    ? resolve(process.cwd(), opt.outFile)
    : parsedTsConfig.compilerOptions.outFile
    ? resolve(dirname(configFile), parsedTsConfig.compilerOptions.outFile)
    : parsed.name + '.js'

  const outDir = parsedTsConfig.compilerOptions.outDir
    ? resolve(dirname(configFile), parsedTsConfig.compilerOptions.outDir)
    : dirname(outFile)

  if (outFile.startsWith(outDir)) {
    outFile = outFile.replace(outDir + '/', '')
  }

  const coverage = !isWatching && (opt.coverage || instrumentCoverage)

  const options: ICompilerOptions = {
    files: opt.files || [opt.file as string],
    outDir,
    tsconfig: configFile,
    coverage: coverage,
    target: (opt.target as any) || 'web',
    rootFolder,
    fileName: opt.fileName,
    library: opt.library,
    globalObject: opt.globalObject
  }

  console.log(`
      root: ${options.rootFolder}
    outDir: ${options.outDir}
   options: { coverage: ${coverage}, production: ${isProduction}, watch: ${isWatching} }`)

  const result = await compile(options)

  if (result.hasErrors()) {
    throw new Error(
      result.toString({
        assets: true,
        colors: true,
        entrypoints: true,
        env: true,
        errors: true,
        publicPath: true
      })
    )
  }

  if (result.hasWarnings()) {
    console.log(
      result.toString({
        assets: true,
        colors: true,
        entrypoints: true,
        env: true,
        errors: true,
        publicPath: true
      })
    )
  }
}

export async function glob(path: string) {
  return new Promise<string[]>((onSuccess, onFailure) => {
    globPkg(path, { absolute: true }, (err, values) => {
      if (err) {
        onFailure(err)
      } else {
        onSuccess(values)
      }
    })
  })
}

export async function cli(args: string[]) {
  const files = await glob(process.argv[2])

  await Promise.all(files.map($ => processFile({ file: $, outFile: args[3] })))
}

export async function processJson(file: string) {
  const config: any[] = require(file)

  if (!config || !(config instanceof Array)) {
    throw new Error(`Config file ${file} is not a valid sequence of steps`)
  }

  if (config.length === 0) {
    throw new Error(`Config file ${file} describes no compilation steps`)
  }

  for (let i = 0; i < config.length; i++) {
    const $ = config[i]

    if ($.kind === 'RM') {
      if (!isWatching) {
        // delete a folder
        console.log(
          `
          Deleting folder: ${$.path}
        `.trim()
        )
        rimraf.sync($.path)
      }
    } else if ($.kind === 'Webpack') {
      // compile TS
      const files = await glob($.file)

      await processFile({ ...$, files })
    } else if ($.kind === 'TSC') {
      if (!$.config) {
        throw new Error(`Missing config in: ${JSON.stringify($, null, 2)}`)
      }

      await tsc($.config)
    } else {
      console.error(`Unknown compilation step ${JSON.stringify($, null, 2)}`)
    }
  }
}

cli(process.argv)
  .then(() => {
    if (isWatching) {
      console.log('The compiler is watching file changes...')
      process.stdin.resume()
    }
  })
  .catch(err => {
    console.error(err)
    process.exit(1)
  })

process.on('unhandledRejection', e => {
  throw e
})

function ProgressBarPlugin(options: any) {
  options = options || {}

  let stream = options.stream || process.stderr
  let enabled = stream && stream.isTTY

  if (!enabled) {
    return function() {
      // stub
    }
  }

  let barLeft = chalk.bold('[')
  let barRight = chalk.bold(']')
  let preamble = chalk.cyan.bold('  build ') + barLeft
  let barFormat = options.format || preamble + ':bar' + barRight + chalk.green.bold(' :percent')
  let summary = options.summary !== false
  let summaryContent = options.summaryContent
  let customSummary = options.customSummary

  delete options.format
  delete options.total
  delete options.summary
  delete options.summaryContent
  delete options.customSummary

  let barOptions = Object.assign(
    {
      complete: '=',
      incomplete: ' ',
      width: 20,
      total: 100,
      clear: true
    },
    options
  )

  let bar = new ProgressBar(barFormat, barOptions)

  let running = false
  let startTime: Date = new Date()
  let lastPercent = 0

  return new webpack.ProgressPlugin(function(percent, msg) {
    if (!running && lastPercent !== 0 && !customSummary) {
      stream.write('\n')
    }

    let newPercent = Math.ceil(percent * barOptions.width)

    if (lastPercent !== newPercent) {
      bar.update(percent, {
        msg: msg
      })
      lastPercent = newPercent
    }

    if (!running) {
      running = true
      startTime = new Date()
      lastPercent = 0
    } else if (percent === 1) {
      let now = new Date()
      let buildTime = (now.getTime() - startTime.getTime()) / 1000 + 's'

      bar.terminate()

      if (summary) {
        stream.write(chalk.green.bold('Build completed in ' + buildTime + '\n\n'))
      } else if (summaryContent) {
        stream.write('    ' + summaryContent + '(' + buildTime + ')\n\n')
      }

      if (customSummary) {
        customSummary(buildTime)
      }

      running = false
    }
  })
}
