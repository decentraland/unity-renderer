import * as path from 'path'
import { glob } from 'glob'
import { cleanGeneratedCode, execute, normalizePath, protocolPath, protocPath, workingDirectory } from './helpers'

const bffOutputPath = path.resolve(
    __dirname,
    '../../unity-renderer/Assets/Scripts/MainScripts/DCL/WorldRuntime/KernelCommunication/BFF/GeneratedCode/',
)

async function main() {
    await execute(`${protocPath} --version`, workingDirectory)

    await buildBFF()
}


async function buildBFF() {
    console.log('Building BFF...')
    cleanGeneratedCode(bffOutputPath)

    const bffInputPath = normalizePath(
        path.resolve(protocolPath, 'decentraland/bff/http_endpoints.proto'),
    )

    const googleProtoFiles = glob
        .sync(
            normalizePath(path.resolve(protocolPath, 'google/**/*.proto')),
        )
        .join(' ')
    
    const protoFiles = glob.sync(bffInputPath).join(' ')

    let command = `${protocPath}`
    command += ` --csharp_out "${bffOutputPath}"`
    command += ` --csharp_opt=file_extension=.gen.cs`
    command += ` --proto_path "${protocolPath}/google"`
    command += ` --proto_path "${protocolPath}/decentraland/bff"`
    command += ` ${googleProtoFiles} ${protoFiles}`

    console.log('Building BFF... Done!', googleProtoFiles)
    
    await execute(command, workingDirectory)
    console.log('Building BFF... Done!', googleProtoFiles)
}

main().catch((err) => {
    console.error(err)
    process.exit(1)
})
