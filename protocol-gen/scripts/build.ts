import * as path from 'path'
import { exec } from 'node:child_process'
import * as fs from 'node:fs'
import { glob } from 'glob'
const PROD = !!process.env.CI


console.log(`production: ${PROD}`)

const isWin = process.platform === "win32";
const workingDirectory = __dirname
const nodeModulesPath = path.resolve(__dirname, '../node_modules/')
const protocPath = path.resolve(nodeModulesPath, '.bin/protoc')
const protocolPath = path.resolve(nodeModulesPath, '@dcl/protocol')

/*const componentsOutputPath = path.resolve(
  __dirname,
  '../../unity-renderer/Assets/DCLPlugins/ECS7/ProtocolBuffers/Generated/PBFiles/',
)*/

const rendererProtocolOutputPath = path.resolve(
  __dirname,
  '../../unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/',
)

async function main() {
  await execute(`${protocPath} --version`, workingDirectory)

  await buildRendererProtocol()
  //await buildComponents()
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
  const rendererProtocolInputPath = normalizePath(path.resolve(protocolPath, 'renderer-protocol/**/*.proto'))
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

/*

protoc
  --csharp_out "/Users/mateomiccino/decentraland/unity-renderer/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/" --csharp_opt=file_extension=.gen.cs
  --plugin=protoc-gen-dclunity=/Users/mateomiccino/decentraland/unity-renderer/unity-renderer/Assets/protoc-gen-dclunity-1.0.0-20220629020644.commit-02bd8f6/package/dist/index.js
  --dclunity_out "/Users/mateomiccino/decentraland/unity-renderer/unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/" --proto_path "/Users/mateomiccino/decentraland/unity-renderer/unity-renderer/Assets/@dcl-protocol-1.0.0-2569677750.commit-6ce832a/package/renderer-protocol/" "/Users/mateomiccino/decentraland/unity-renderer/unity-renderer/Assets/@dcl-protocol-1.0.0-2569677750.commit-6ce832a/package/renderer-protocol/RendererProtocol.proto"

// TODO: Builds components
async function preProcessComponents() {
  const input = `${protocolPath}/ecs/components/`
  //const output = `${protocolPath}/ecs/components/*.proto`

  const files = fs.readdirSync(input)

  for (const file of files) {
    
    console.log(input + file)
  }
}

async function buildComponents() {
  await preProcessComponents()

  let command = `${protocPath}`
  command += ` --csharp_out "${componentsOutputPath}"`
  command += ` --proto_path "${protocolPath}/ecs/components"`
  command += ` ${protocolPath}/ecs/components/*.proto`

  await execute(command, workingDirectory)
}*/

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
