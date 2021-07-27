import { exec } from "child_process"
import { readFileSync } from "fs"
import { resolve } from "path"
import { ensureFileExists } from "./utils"
import fetch from "node-fetch";
import semver = require("semver");

const DIST_ROOT = resolve(__dirname, "../dist")

async function main() {
  await checkFiles()
  if (process.env.CIRCLE_BRANCH == "master") {
    await publish(["latest"], "public", DIST_ROOT)
  }

  const version = getVersion(DIST_ROOT)
  const pkgName = (await execute(`npm info . name`, workingDirectory)).trim();
  triggerPipeline(pkgName, version, `latest`)
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

async function getVersion(workingDirectory: string) {
  const json = JSON.parse(readFileSync(workingDirectory + "/package.json", "utf8"));

  let pkgJsonVersion = json.version;
  if (!pkgJsonVersion) pkgJsonVersion = "0.0.0";

  const version = semver.parse(pkgJsonVersion.trim());

  if (!version) {
    throw new Error("Unable to parse semver from " + pkgJsonVersion);
  }

  return `${version.major}.${version.minor}.${version.patch}`;
}

async function triggerPipeline(
  packageName: string,
  packageTag: string,
  packageVersion: string
) {
  const GITLAB_STATIC_PIPELINE_TOKEN = process.env.GITLAB_TOKEN
  const GITLAB_STATIC_PIPELINE_URL = process.env.GITLAB_PIPELINE_URL

  if (!GITLAB_STATIC_PIPELINE_URL) return;

  const body = new FormData();
  if (GITLAB_STATIC_PIPELINE_TOKEN) {
    body.append("token", GITLAB_STATIC_PIPELINE_TOKEN);
  }
  body.append("ref", "master");
  body.append("variables[PACKAGE_NAME]", packageName);
  body.append("variables[PACKAGE_DIST_TAG]", packageTag);
  body.append("variables[PACKAGE_VERSION]", packageVersion);
  body.append("variables[REPO]", "unity-renderer");
  body.append("variables[REPO_OWNER]", "decentraland");
  body.append("variables[COMMIT]", process.env.CIRCLE_SHA1);

  try {
    const r = await fetch(GITLAB_STATIC_PIPELINE_URL, {
      body,
      method: "POST",
    });

    if (r.ok) {
      console.info(`Status: ${r.status}`);
    } else {
      console.error(`Error triggering pipeline. status: ${r.status}`);
    }
  } catch (e) {
    console.error(`Error triggering pipeline. Unhandled error.`);
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
