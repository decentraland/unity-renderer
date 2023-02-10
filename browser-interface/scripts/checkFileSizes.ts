import * as fs from 'fs'
import { stat } from 'fs/promises'
import * as glob from 'glob'
import path from 'path'
import * as zlib from 'zlib'

async function main() {
  console.log(`Checking file sizes...`)
  await checkFileSizes()
}

async function checkFileSizes() {
  const checks: Promise<any>[] = []
  const basePath = path.resolve(process.env.BUILD_PATH!, 'Build')
  for (const file of glob.sync('**/*', { cwd: basePath, absolute: true })) {
    checks.push(checkFileSize(file))
  }
  const streamingPath = path.resolve(process.env.BUILD_PATH!, 'StreamingAssets')

  for (const file of glob.sync('**/*', { cwd: streamingPath, absolute: true })) {
    checks.push(checkFileSize(file))
  }

  return Promise.all(checks)
}

const MAX_FILE_SIZE = 42_000_000 // rougly 42mb https://www.notion.so/Cache-unity-data-br-on-explore-4382b0cb78184973af415943f708cba1

async function checkFileSize(file: string) {
  const stats = await stat(file)
  if (stats.size > MAX_FILE_SIZE) {
    console.error(
      `Warning, the file ${file} exceeds the maximum cacheable file (uncompressed) size: ${(
        stats.size /
        1024 /
        1024
      ).toFixed(2)}MB`
    )
    const length = await getBrotliSize(file)
    if (length > MAX_FILE_SIZE) {
      console.error(`The file ${file} exceeds the maximum cacheable file size: ${(length / 1024 / 1024).toFixed(2)}MB`)
      process.exitCode = 1
      throw new Error(`${file} exceeds ${(length / 1024 / 1024).toFixed(2)}MB`)
    } else {
      console.log(`The file ${file} has a compressed file size of: ${(length / 1024 / 1024).toFixed(2)}MB`)
    }
  }
  return
}

async function getBrotliSize(filePath: string): Promise<number> {
  const file = fs.createReadStream(filePath, {
    encoding: 'binary'
  })

  let dataLen = 0
  const compress = zlib.createBrotliCompress({
    chunkSize: 1024 * 1024,
    maxOutputLength: MAX_FILE_SIZE
  })
  compress.on('data', (chunk) => {
    dataLen += chunk.length
  })
  file.pipe(compress)

  return new Promise((resolve, reject) => {
    compress.on('end', () => resolve(dataLen))
    compress.on('error', reject)
  })
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
