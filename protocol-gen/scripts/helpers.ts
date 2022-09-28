import * as path from 'path'
import * as fs from 'node:fs'
import { exec } from 'node:child_process'
import { glob } from 'glob'

export const PROD = !!process.env.CI
export const isWin = process.platform === 'win32'
export const workingDirectory = __dirname
export const nodeModulesPath = path.resolve(__dirname, '../node_modules/')
export const protocPath = path.resolve(nodeModulesPath, '.bin/protoc')
export const protocolPath = path.resolve(nodeModulesPath, '@dcl/protocol')

export function normalizePath(path: string) {
  if (isWin) {
    return path.replace(/\\/g, '/')
  } else {
    return path
  }
}

export function cleanGeneratedCode(path: string) {
  const files = glob.sync(normalizePath(`${path}/**/*.gen.cs`))

  for (const file of files) {
    fs.unlinkSync(file)
  }
}

export async function execute(
  command: string,
  workingDirectory: string,
): Promise<string> {
  return new Promise<string>((onSuccess, onError) => {
    exec(command, { cwd: workingDirectory }, (error, stdout, stderr) => {
      stdout.trim().length &&
        console.log('stdout:\n' + stdout.replace(/\n/g, '\n  '))
      stderr.trim().length &&
        console.error('stderr:\n' + stderr.replace(/\n/g, '\n  '))

      if (error) {
        onError(stderr)
      } else {
        onSuccess(stdout)
      }
    })
  })
}

export const camelToSnakeCase = (text: string) =>
  text.replace(/[A-Z]/g, (letter) => `_${letter.toLowerCase()}`).slice(1)
