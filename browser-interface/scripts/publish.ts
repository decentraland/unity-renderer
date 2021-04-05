import { exec } from "child_process"
import { readFileSync } from "fs"
import { resolve } from "path"
import { ensureFileExists } from "./utils"

const DIST_ROOT = resolve(__dirname, "../dist")

async function main() {
  await checkFiles()
  if (process.env.CIRCLE_BRANCH == "master") {
    await publish(["latest"], "public", DIST_ROOT)
  }
}

async function checkFiles() {
  const packageJson = JSON.parse(readFileSync(resolve(DIST_ROOT, "./package.json")).toString())
  console.log("> will publish:\n" + JSON.stringify(packageJson, null, 2))
  console.assert(packageJson.main, "package.json must contain main file")
  console.assert(packageJson.typings, "package.json must contain typings file")
  ensureFileExists(DIST_ROOT, packageJson.main)
  ensureFileExists(DIST_ROOT, packageJson.typings)
  ensureFileExists(DIST_ROOT, "UnityLoader.js")
}

export async function publish(npmTags: string[], access: string, workingDirectory: string): Promise<string> {
  const args: string[] = []

  if (access) {
    args.push("--access", access)
  }

  for (let tag of npmTags) {
    args.push("--tag", JSON.stringify(tag))
  }

  return execute(`npm publish ` + args.join(" "), workingDirectory)
}

async function execute(command: string, workingDirectory: string): Promise<string> {
  return new Promise<string>((onSuccess, onError) => {
    exec(command, { cwd: workingDirectory }, (error, stdout, stderr) => {
      stdout.trim().length && console.log("stdout:\n" + stdout.replace(/\n/g, "\n  "))
      stderr.trim().length && console.error("stderr:\n" + stderr.replace(/\n/g, "\n  "))

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
