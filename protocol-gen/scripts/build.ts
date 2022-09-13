import * as path from 'path'
import { exec } from 'node:child_process'
import * as fs from 'node:fs'
import { glob } from 'glob'
const PROD = !!process.env.CI

console.log(`production: ${PROD}`)

const isWin = process.platform === 'win32'
const workingDirectory = __dirname
const nodeModulesPath = path.resolve(__dirname, '../node_modules/')
const protocPath = path.resolve(nodeModulesPath, '.bin/protoc')
const protocolPath = path.resolve(nodeModulesPath, '@dcl/protocol')

const rendererProtocolOutputPath = path.resolve(
  __dirname,
  '../../unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/',
)

async function main() {
  await execute(`${protocPath} --version`, workingDirectory)

  await buildRendererProtocol()
}

function normalizePath(path: string) {
  if (isWin) {
    return path.replace(/\\/g, '/')
  } else {
    return path
  }
}

function cleanGeneratedCode(path: string) {
  const files = glob.sync(normalizePath(`${path}/**/*.gen.cs`))

  for (const file of files) {
    fs.unlinkSync(file)
  }
}

async function buildRendererProtocol() {
  cleanGeneratedCode(rendererProtocolOutputPath)

  const rendererProtocolInputPath = normalizePath(
    path.resolve(protocolPath, 'renderer-protocol/**/*.proto'),
  )

  const protoFiles = glob.sync(rendererProtocolInputPath).join(' ')

  const ext = isWin ? 'cmd' : 'js'

  let command = `${protocPath}`
  command += ` --csharp_out "${rendererProtocolOutputPath}"`
  command += ` --csharp_opt=file_extension=.gen.cs`
  command += ` --plugin=protoc-gen-dclunity=${nodeModulesPath}/protoc-gen-dclunity/dist/index.${ext}`
  command += ` --dclunity_out "${rendererProtocolOutputPath}"`
  command += ` --proto_path "${protocolPath}/renderer-protocol"`
  command += ` ${protoFiles}`

  await execute(command, workingDirectory)
}

async function execute(
  command: string,
  workingDirectory: string,
): Promise<string> {
  return new Promise<string>((onSuccess, onError) => {
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

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
