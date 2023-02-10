import { exec } from 'child_process'
import { mkdir, readFile } from 'fs/promises'
import * as glob from 'glob'
import path, { resolve } from 'path'
import { fetch, FormData } from 'undici'
import { copyFile, ensureEqualFiles, ensureFileExists } from './utils'

const DIST_ROOT = resolve(__dirname, '../static')

async function main() {
  await copyBuiltFiles()
  await checkFiles()

  if (!process.env.GITLAB_PIPELINE_URL) {
    console.log('GITLAB_PIPELINE_URL not present. Skipping CDN pipeline trigger')
    return
  }

  if (!process.env.GITLAB_TOKEN) {
    console.log('GITLAB_TOKEN not present. Skipping CDN pipeline trigger')
    return
  }

  if (!process.env.CIRCLE_SHA1) {
    console.log('CIRCLE_SHA1 not present. Skipping CDN pipeline trigger')
    return
  }

  if (!process.env.NPM_TOKEN) {
    console.log('NPM_TOKEN not present. Skipping CDN pipeline trigger')
    return
  }

  const { version, name } = await getPackageJson(DIST_ROOT)

  if (process.env.CIRCLE_BRANCH == 'dev') {
    const tag = 'next'
    await publish([tag], 'public', DIST_ROOT)
    // inform cdn-pipeline about new version
    await triggerPipeline(name, version, tag)
  } else if (process.env.CIRCLE_BRANCH == 'main') {
    const tag = 'latest'
    await publish([tag], 'public', DIST_ROOT)
    // inform cdn-pipeline about new version
    await triggerPipeline(name, version, tag)
  }
  console.log(`Publish complete!`)
}

async function checkFiles() {
  const packageJson = JSON.parse(await readFile(resolve(DIST_ROOT, './package.json'), 'utf-8'))
  console.log('> will publish:\n' + JSON.stringify(packageJson, null, 2))
  console.assert(packageJson.main, 'package.json must contain main file')
  ensureFileExists(DIST_ROOT, packageJson.main)
  ensureFileExists(DIST_ROOT, 'unity.loader.js')
  ensureFileExists(DIST_ROOT, 'unity.data')
  ensureFileExists(DIST_ROOT, 'unity.wasm')
  ensureFileExists(DIST_ROOT, 'unity.framework.js')
  ensureFileExists(DIST_ROOT, 'index.html')
  ensureFileExists(DIST_ROOT, 'preview.html')
}

const DIST_PATH = path.resolve(__dirname, '../static')
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

  const streamingPath = path.resolve(process.env.BUILD_PATH!, 'StreamingAssets')
  const streamingDistPath = path.resolve(DIST_PATH, 'StreamingAssets')

  for (const file of glob.sync('**/*', { cwd: streamingPath, absolute: true })) {
    copyFile(file, path.resolve(streamingDistPath, file.replace(streamingPath + '/', './')))
  }
}

async function getPackageJson(workingDirectory: string) {
  return JSON.parse(await readFile(workingDirectory + '/package.json', 'utf8'))
}

async function triggerPipeline(packageName: string, packageVersion: string, npmTag: string) {
  const GITLAB_STATIC_PIPELINE_TOKEN = process.env.GITLAB_TOKEN!
  const GITLAB_STATIC_PIPELINE_URL = process.env.GITLAB_PIPELINE_URL!

  const body = new FormData()
  body.append('token', GITLAB_STATIC_PIPELINE_TOKEN)
  body.append('ref', 'master')
  body.append('variables[PACKAGE_NAME]', packageName)
  body.append('variables[PACKAGE_VERSION]', packageVersion)
  body.append('variables[PACKAGE_TAG]', npmTag)
  body.append('variables[REPO]', 'unity-renderer')
  body.append('variables[REPO_OWNER]', 'decentraland')
  body.append('variables[COMMIT]', process.env.CIRCLE_SHA1 as string)

  try {
    const r = await fetch(GITLAB_STATIC_PIPELINE_URL, {
      body,
      method: 'POST'
    })

    if (r.ok) {
      console.info(`Status: ${r.status}`)
    } else {
      throw new Error(`Error triggering pipeline. status: ${r.status}`)
    }
  } catch (e) {
    throw new Error(`Error triggering pipeline. Unhandled error.`)
  }
}

export async function publish(npmTags: string[], access: string, workingDirectory: string): Promise<string> {
  const args: string[] = []

  if (access) {
    args.push('--access', access)
  }

  for (let tag of npmTags) {
    args.push('--tag', JSON.stringify(tag))
  }

  return execute(`npm publish ` + args.join(' '), workingDirectory)
}

async function execute(command: string, workingDirectory: string): Promise<string> {
  return new Promise<string>((onSuccess, onError) => {
    exec(command, { cwd: workingDirectory }, (error, stdout, stderr) => {
      stdout.trim().length && console.log('stdout:\n' + stdout.replace(/\n/g, '\n  '))
      stderr.trim().length && console.error('stderr:\n' + stderr.replace(/\n/g, '\n  '))

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
