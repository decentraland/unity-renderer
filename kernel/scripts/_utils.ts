// tslint:disable:no-console
import fs = require('fs-extra')
import path = require('path')

/**
 * @returns the resolved absolute path
 */
export function ensureFileExists(root: string, file: string) {
  const x = path.resolve(root, file.replace(/^\//, ''))

  if (!fs.existsSync(x)) {
    throw new Error(`${x} does not exist`)
  }

  return x
}

export function copyFile(from: string, to: string) {
  console.log(`> copying ${from} to ${to}`)

  if (!fs.existsSync(from)) {
    throw new Error(`${from} does not exist`)
  }

  fs.copySync(from, to)

  if (!fs.existsSync(to)) {
    throw new Error(`${to} does not exist`)
  }
}
