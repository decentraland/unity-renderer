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

/**
 * @returns the resolved absolute path
 */
export function ensureEqualFiles(first: string, second: string) {
  console.log(`> checking that ${first} and ${second} are equal...`)
  if (!fs.existsSync(first)) {
    throw new Error(`${first} does not exist`)
  }
  if (!fs.existsSync(second)) {
    throw new Error(`${second} does not exist`)
  }
  // TODO: async these
  const contentsFirst = fs.readFileSync(first)
  const contentsSecond = fs.readFileSync(second)
  if (contentsFirst.byteLength !== contentsSecond.byteLength) {
    throw new Error(`Files ${first} and ${second} are of different length`)
  }
  const length = contentsFirst.byteLength
  let line = 0
  const endLine = '\n'.charCodeAt(0)
  for (let i = 0; i < length; i++) {
    if (contentsFirst[i] === endLine)
      line += 1
    if (contentsFirst[i] !== contentsSecond[i])
      throw new Error(`Files ${first} and ${second} are different, on line ${line}, char ${i}`)
  }

  return true
}

export function copyFile(from: string, to: string) {
  console.log(`> copying ${from} to ${to}`)

  if (!fs.existsSync(from)) {
    throw new Error(`${from} does not exist`)
  }

  // if it is not a file, remove it to avoid conflict with symbolic links
  if (fs.existsSync(to)) {
    const type = fs.lstatSync(to)
    if (!type.isFile()) {
      fs.removeSync(to)
    }
  }

  fs.copySync(from, to)

  if (!fs.existsSync(to)) {
    throw new Error(`${to} does not exist`)
  }
}
