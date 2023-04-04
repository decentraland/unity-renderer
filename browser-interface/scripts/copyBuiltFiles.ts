import { mkdir } from 'fs/promises'
import { copyFile, ensureEqualFiles } from './utils'
import * as glob from 'glob'
import path from 'path'
import fs from "fs";

const DIST_PATH = path.resolve(__dirname, '../static')

async function main() {
    console.log(`Copying unity build files...`)
    await copyBuiltFiles()
}

// This function copies the built files from unity into the target folder
async function copyBuiltFiles() {
  await mkdir(DIST_PATH, { recursive: true })

  const basePath = path.resolve(process.env.BUILD_PATH!, 'Build')
  try {
    ensureEqualFiles(path.resolve(basePath, 'unity.loader.js'), path.resolve(DIST_PATH, 'unity.loader.js'))
  } catch (e) {
    console.log(`
      unity.loader.js is checked out in the repository, as it seldom changes, to avoid coupling
      the build step of 'browser-interface' with 'unity-renderer'. If the file changes, make sure
      to update the version stored on \`static/unity.loader.js\` (this is frequently the case when
      updating the unity version).
   `)
    throw e
  }

  for (const file of glob.sync('**/*', { cwd: basePath, absolute: true })) {
    copyFile(file, path.resolve(DIST_PATH, file.replace(basePath + '/', './')))
  }

  const webGLStreamingAssetsPath = path.resolve(process.env.BUILD_PATH!, 'StreamingAssets/aa/WebGL')

  if (!fs.existsSync(webGLStreamingAssetsPath)) {
    throw new Error(`Streaming Assets folder for WebGL does not exist! Please check that they are built correctly`)
  }

  const streamingPath = path.resolve(process.env.BUILD_PATH!, 'StreamingAssets')
  const streamingDistPath = path.resolve(DIST_PATH, 'StreamingAssets')

  for (const file of glob.sync('**/*', { cwd: streamingPath, absolute: true })) {
    copyFile(file, path.resolve(streamingDistPath, file.replace(streamingPath + '/', './')))
  }
}

main().catch((err) => {
    console.error(err)
    process.exit(1)
})
  