// tslint:disable-next-line:no-commented-out-code
// tslint:disable:no-console

import path = require('path')
import { readFileSync, writeFileSync } from 'fs-extra'
import { ensureFileExists, copyFile } from './_utils'

const root = path.resolve(__dirname, '../packages/decentraland-ecs')

const original = ensureFileExists(root, '/dist/index.d.ts')

copyFile(original, root + '/types/dcl/index.d.ts')

const dtsFile = ensureFileExists(root, '/types/dcl/index.d.ts')

console.log(`> remove '^export ' from ${dtsFile}`)
{
  const content = readFileSync(dtsFile).toString()

  writeFileSync(dtsFile, content.replace(/^export /gm, ''))

  console.log(`> ${dtsFile} must have no /^import /`)

  if (content.match(/^import /)) {
    throw new Error(`The file ${dtsFile} contains imports:\n${content}`)
  }

  console.log(`> ${dtsFile} must have no '/// <ref'`)

  if (content.includes('/// <ref')) {
    throw new Error(`The file ${dtsFile} contains '/// <ref':\n${content}`)
  }
}
