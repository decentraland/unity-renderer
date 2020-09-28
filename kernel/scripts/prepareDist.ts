#!/usr/bin/env node
// tslint:disable:no-console

import fs = require('fs-extra')
import path = require('path')
import { readFileSync, writeFileSync } from 'fs-extra'
import { execSync } from 'child_process'
import { copyFile } from './_utils'

const root = path.resolve(__dirname, '..')
const commitHash = execSync('git rev-parse HEAD')
  .toString()
  .trim()
const md5File = require('md5-file/promise')

async function copyIndex(filename: string) {
  let md5 = ''

  let newFileName = `${filename}.js`

  console.log(`> copy ${filename}.js to ${filename}.<hash>.js`)
  {
    const src = path.resolve(root, `static/dist/${filename}.js`)

    if (!fs.existsSync(src)) {
      throw new Error(`${src} does not exist`)
    }

    md5 = await md5File(src)

    newFileName = `${filename}.${md5}.js`

    const dst = path.resolve(root, `static/dist/${newFileName}`)

    await fs.copy(src, dst)

    if (!fs.existsSync(dst)) {
      throw new Error(`${dst} does not exist`)
    }
  }

  const targetIndexHtml = path.resolve(root, 'static/index.html')

  if (!fs.existsSync(targetIndexHtml)) {
    throw new Error(`${targetIndexHtml} does not exist`)
  }

  console.log(`> replace ${filename}.js -> ${newFileName} in html`)
  {
    let content = readFileSync(targetIndexHtml).toString()

    if (!content.includes(`${filename}.js`)) {
      throw new Error(`index.html is dirty and does\'t contain the text "${filename}.js"`)
    }

    content = content.replace(new RegExp(filename + '.(S+.)?js'), newFileName)
    content = content.replace(/\s*<!--(.+)-->/, '')
    content = content + `\n\n<!-- ${new Date().toISOString()} commit: ${commitHash} -->`

    writeFileSync(targetIndexHtml, content)
  }
}

async function injectDependencies(folder: string, dependencies: string[], devDependency = false) {
  console.log(`> update ${folder}/package.json (injecting dependencies)`)
  {
    const file = path.resolve(root, `${folder}/package.json`)
    const packageJson = JSON.parse(readFileSync(file).toString())
    const localPackageJson = JSON.parse(readFileSync(path.resolve(root, `package.json`)).toString())

    const deps = new Set(dependencies)

    const target = devDependency ? 'devDependencies' : 'dependencies'

    packageJson[target] = packageJson[target] || {}

    deps.forEach(dep => {
      if (localPackageJson.dependencies[dep]) {
        packageJson[target][dep] = localPackageJson.dependencies[dep]
        deps.delete(dep)
        console.log(`  using dependency: ${dep}@${packageJson[target][dep]}`)
      }
    })

    deps.forEach(dep => {
      if (localPackageJson.devDependencies[dep]) {
        packageJson[target][dep] = localPackageJson.devDependencies[dep]
        deps.delete(dep)
        console.log(`  using devDependency: ${dep}@${packageJson[target][dep]}`)
      }
    })

    if (deps.size) {
      throw new Error(`Missing dependencies "${Array.from(deps).join('", "')}"`)
    }

    writeFileSync(file, JSON.stringify(packageJson, null, 2))
  }
}

async function prepareDecentralandECS(folder: string) {
  copyFile(path.resolve(root, `packages/decentraland-amd/dist/amd.js`), path.resolve(root, `${folder}/artifacts/amd.js`))
  copyFile(path.resolve(root, `packages/decentraland-amd/dist/amd.min.js`), path.resolve(root, `${folder}/artifacts/amd.min.js`))
  copyFile(path.resolve(root, `packages/decentraland-amd/dist/amd.min.js.map`), path.resolve(root, `${folder}/artifacts/amd.min.js.map`))
  copyFile(path.resolve(root, `packages/build-ecs/index.js`), path.resolve(root, `${folder}/artifacts/build-ecs.js`))

  copyFile(path.resolve(root, `static/dist/editor.js`), path.resolve(root, `packages/decentraland-ecs/artifacts/editor.js`))

  copyFile(path.resolve(root, `static/dist/preview.js`), path.resolve(root, `${folder}/artifacts/preview.js`))
  copyFile(path.resolve(root, `static/preview.html`), path.resolve(root, `${folder}/artifacts/preview.html`))

  copyFile(path.resolve(root, `static/export.html`), path.resolve(root, `${folder}/artifacts/export.html`))

  // static resources
  copyFile(path.resolve(root, `static/fonts`), path.resolve(root, `${folder}/artifacts/fonts`))
  copyFile(path.resolve(root, `static/images`), path.resolve(root, `${folder}/artifacts/images`))
  copyFile(path.resolve(root, `static/models`), path.resolve(root, `${folder}/artifacts/models`))

  // unity
  copyFile(path.resolve(root, `static/unity`), path.resolve(root, `${folder}/artifacts/unity`))

  await fs.copy(path.resolve(root, `static/default-profile`), path.resolve(root, `${folder}/artifacts/default-profile`))
  await validatePackage(folder)

  // build-ecs is embeded inside decentraland-ecs, therefore, it needs the same dependencies
  await injectDependencies(folder, ['typescript', 'terser'], false)
}

async function validatePackage(folder: string) {
  console.log(`> update ${folder}/package.json commit`)
  {
    const file = path.resolve(root, `${folder}/package.json`)
    const packageJson = JSON.parse(readFileSync(file).toString())

    packageJson.commit = commitHash

    console.log(`  commit: ${commitHash}`)
    writeFileSync(file, JSON.stringify(packageJson, null, 2))
  }

  console.log(`> ensure ${folder}/lib exists`)
  {
    if (!fs.pathExists(path.resolve(root, `${folder}/lib`))) {
      throw new Error(`${folder}/lib folder does not exist`)
    }
  }

  console.log(`> ensure ${folder}/artifacts/preview.js exists`)
  {
    if (!fs.existsSync(path.resolve(root, `${folder}/artifacts/preview.js`))) {
      throw new Error(`${folder}/artifacts/preview.js does not exist`)
    }
  }

  console.log(`> ensure ${folder}/artifacts/amd.js exists`)
  {
    if (!fs.existsSync(path.resolve(root, `${folder}/artifacts/amd.js`))) {
      throw new Error(`${folder}/artifacts/amd.js does not exist`)
    }
  }

  console.log(`> ensure ${folder}/artifacts/preview.html exists`)
  {
    if (!fs.existsSync(path.resolve(root, `${folder}/artifacts/preview.html`))) {
      throw new Error(`${folder}/artifacts/preview.html does not exist`)
    }
  }
}

// tslint:disable-next-line:semicolon
;(async function() {
  await copyIndex('unity')
  await prepareDecentralandECS('packages/decentraland-ecs')
  await injectDependencies('packages/build-ecs', ['typescript', 'terser'], false)
})().catch(e => {
  // tslint:disable-next-line:no-console
  console.error(e)
  process.exit(1)
})
