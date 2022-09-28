import * as path from 'path'
import { glob } from 'glob'
import { cleanGeneratedCode, execute, isWin, nodeModulesPath, normalizePath, protocolPath, protocPath, workingDirectory } from './helpers'

const rendererProtocolOutputPath = path.resolve(
  __dirname,
  '../../unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/',
)

async function main() {
  await execute(`${protocPath} --version`, workingDirectory)

  await buildRendererProtocol()
}

async function buildRendererProtocol() {
  console.log('Building Renderer Protocol...')
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
  console.log('Building Renderer Protocol... Done!')
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
