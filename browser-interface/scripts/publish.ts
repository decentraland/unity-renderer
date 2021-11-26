import { exec } from "child_process"
import { readFileSync } from "fs"
import { resolve } from "path"
import { ensureFileExists } from "./utils"
import fetch from "node-fetch"
import FormData from "form-data"

const DIST_ROOT = resolve(__dirname, "../dist")

async function main() {
  await checkFiles()

  if (!process.env.GITLAB_PIPELINE_URL) {
    console.log("GITLAB_PIPELINE_URL not present. Skipping CDN pipeline trigger")
    return;
  }

  if (!process.env.GITLAB_TOKEN) {
    console.log("GITLAB_STATIC_PIPELINE_TOKEN not present. Skipping CDN pipeline trigger")
    return;
  }

  if (!process.env.CIRCLE_SHA1) {
    console.log("CIRCLE_SHA1 not present. Skipping CDN pipeline trigger")
    return;
  }

  if (!process.env.NPM_TOKEN) {
    console.log("NPM_TOKEN not present. Skipping CDN pipeline trigger")
    return;
  }

  const { version, name } = await getPackageJson(DIST_ROOT)

  if (process.env.CIRCLE_BRANCH == "master") {
    await publish(["latest"], "public", DIST_ROOT)
    // inform cdn-pipeline about new version
    await triggerPipeline(name, version)
  }
}

async function checkFiles() {
  const packageJson = JSON.parse(readFileSync(resolve(DIST_ROOT, "./package.json")).toString())
  console.log("> will publish:\n" + JSON.stringify(packageJson, null, 2))
  console.assert(packageJson.main, "package.json must contain main file")
  console.assert(packageJson.typings, "package.json must contain typings file")
  ensureFileExists(DIST_ROOT, packageJson.main)
  ensureFileExists(DIST_ROOT, packageJson.typings)
  ensureFileExists(DIST_ROOT, "unity.loader.js")
}

async function getPackageJson(workingDirectory: string) {
  return JSON.parse(readFileSync(workingDirectory + "/package.json", "utf8"))
}

async function triggerPipeline(packageName: string, packageVersion: string) {
  const body = new FormData()
  body.append("token", GITLAB_STATIC_PIPELINE_TOKEN)
  body.append("ref", "master")
  body.append("variables[PACKAGE_NAME]", packageName)
  body.append("variables[PACKAGE_VERSION]", packageVersion)
  body.append("variables[REPO]", "unity-renderer")
  body.append("variables[REPO_OWNER]", "decentraland")
  body.append("variables[COMMIT]", process.env.CIRCLE_SHA1 as string)

  try {
    const r = await fetch(GITLAB_STATIC_PIPELINE_URL, {
      body,
      method: "POST",
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
