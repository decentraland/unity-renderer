// tslint:disable-next-line:no-commented-out-code
// tslint:disable:no-console

import path = require('path')
import { readFileSync, writeFileSync } from 'fs-extra'
import { ensureFileExists, copyFile } from './_utils'

const root = path.resolve(__dirname, '../packages/decentraland-ecs')

const original = ensureFileExists(root, '/dist/index.d.ts')

copyFile(original, root + '/types/dcl/index.d.ts')

const dtsFile = ensureFileExists(root, '/types/dcl/index.d.ts')
{
  const content = readFileSync(dtsFile).toString()

  writeFileSync(dtsFile, content.replace(/^export /gm, ''))

  if (content.match(/\bimport\b/)) {
    throw new Error(`The file ${dtsFile} contains imports:\n${content}`)
  }

  if (content.includes('/// <ref')) {
    throw new Error(`The file ${dtsFile} contains '/// <ref':\n${content}`)
  }
}
