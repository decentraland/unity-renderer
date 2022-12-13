import * as path from 'path'
import { glob } from 'glob'
import { cleanGeneratedCode, execute, isWin, nodeModulesPath, normalizePath, protocolPath, protocPath, workingDirectory } from './helpers'
import { readFileSync, writeFileSync } from 'fs'
import * as fse from 'fs-extra'
import * as fs from 'node:fs'

const rendererProtocolOutputPath = path.resolve(
  __dirname,
  '../../unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/RPC/GeneratedCode/',
)

const tempProtocolPath = normalizePath(
  path.resolve(__dirname, '../temp-components/'),
)

const rendererProtocolInputPath = normalizePath(
  path.resolve(tempProtocolPath, 'decentraland/'),
)

async function main() {
  if (fs.existsSync(tempProtocolPath)) {
    fs.rmSync(tempProtocolPath, { recursive: true })
  }
  fse.copySync(protocolPath, tempProtocolPath, {
    overwrite: true,
  })

  await execute(`${protocPath} --version`, workingDirectory)

  await buildRendererProtocol()

  fs.rmSync(tempProtocolPath, { recursive: true })
}

function fixEngineInterface() {
  const engineInterfaceProtoPath = normalizePath(
    path.resolve(tempProtocolPath, 'decentraland/renderer/engine_interface.proto'),
  )
  const content = readFileSync(engineInterfaceProtoPath).toString()

  const newContent = content
    .replace('// option csharp_namespace = "DCL.Interface";', 'option csharp_namespace = "DCL.Interface";')

  writeFileSync(engineInterfaceProtoPath, newContent)
}

async function buildRendererProtocol() {
  console.log('Building Renderer Protocol...')
  cleanGeneratedCode(rendererProtocolOutputPath)

  fixEngineInterface()

  const files = glob.sync(rendererProtocolInputPath + '/**/*.proto')

  const protoFiles = files.join(' ')

  const ext = isWin ? 'cmd' : 'js'

  let command = `${protocPath}`
  command += ` --csharp_out "${rendererProtocolOutputPath}"`
  command += ` --csharp_opt=file_extension=.gen.cs`
  command += ` --plugin=protoc-gen-dclunity=${nodeModulesPath}/protoc-gen-dclunity/dist/index.${ext}`
  command += ` --dclunity_out "${rendererProtocolOutputPath}"`
  command += ` --proto_path "${tempProtocolPath}"`
  command += ` ${protoFiles}`

  fixEngineInterface()

  await execute(command, workingDirectory)
  console.log('Building Renderer Protocol... Done!')
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
