const fs = require('fs')

if(!process.env.CIRCLE_BRANCH) throw new Error('env var CIRCLE_BRANCH not set!')
if(!process.env.CIRCLE_SHA1) throw new Error('env var CIRCLE_SHA1 not set!')
if(!process.env.SENTRY_DSN) throw new Error('env var SENTRY_DSN not set!')

const fileToEdit = "unity-renderer/Assets/Scripts/MainScripts/DCL/Configuration/SentryConfiguration.cs"
const fileContents = fs.readFileSync(fileToEdit).toString()

let rendererEnvironment = 'editor'

if(process.env.CIRCLE_BRANCH == 'main') {
  rendererEnvironment = 'production'
} else if(process.env.CIRCLE_BRANCH == 'dev') {
  rendererEnvironment = 'development'
} else {
  rendererEnvironment = `branch/${process.env.CIRCLE_BRANCH || 'unknown'}`
}

const searchParams = {
  dsn: '"Dsn"',
  env: '"editor"',
  release: '"Release"'
}

for (const key in searchParams) {
  if (!fileContents.includes(searchParams[key])) throw new Error(`the file ${fileToEdit} does not contain any ${searchParams[key]} string to replace`)
}

fs.writeFileSync(fileToEdit, fileContents
  .replace(searchParams.dsn, JSON.stringify(process.env.SENTRY_DSN))
  .replace(searchParams.env, JSON.stringify(rendererEnvironment))
  .replace(searchParams.release, JSON.stringify(process.env.CIRCLE_SHA1))
)
