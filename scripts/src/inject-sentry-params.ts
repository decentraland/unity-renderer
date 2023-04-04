import * as fs from 'node:fs'

async function main() {
  if (!process.env.CIRCLE_BRANCH) throw new Error('env var CIRCLE_BRANCH not set!')
  if (!process.env.CIRCLE_SHA1) throw new Error('env var CIRCLE_SHA1 not set!')
  if (!process.env.SENTRY_DSN) throw new Error('env var SENTRY_DSN not set!')
  if (!process.env.SENTRY_AUTH_TOKEN) throw new Error('env var SENTRY_AUTH not set!')

  // Inject sentry environment parameters

  const fileToEdit =
    '../unity-renderer/Assets/Resources/Sentry/SentryOptions.asset'
  const fileContents = fs.readFileSync(fileToEdit).toString()

  let rendererEnvironment = 'editor'

  if (process.env.CIRCLE_BRANCH == 'main') {
    rendererEnvironment = 'production'
  } else if (process.env.CIRCLE_BRANCH == 'dev') {
    rendererEnvironment = 'development'
  } else {
    rendererEnvironment = `branch`
  }

  const searchParams = {
    dsn: 'https://dsn.dsn/',
    env: '<ENVIRONMENT>',
    release: '<RELEASE>',
  }

  for (const key in searchParams) {
    const value = (searchParams as any)[key]
    if (!fileContents.includes(value))
      throw new Error(
        `the file ${fileToEdit} does not contain any ${value} string to replace`,
      )
  }

  fs.writeFileSync(
    fileToEdit,
    fileContents
      .replace(searchParams.dsn, process.env.SENTRY_DSN)
      .replace(searchParams.env, rendererEnvironment)
      .replace(searchParams.release, process.env.CIRCLE_SHA1)
  )

  // Inject CLI parameters for sources/symbols
  const cliSearchParams = {
    auth: '<AUTH_TOKEN>',
  }

  const cliFileToEdit =
    '../unity-renderer/Assets/Plugins/Sentry/SentryCliOptions.asset'
  const cliFileContents = fs.readFileSync(cliFileToEdit).toString()

  for (const key in cliSearchParams) {
    const value = (cliSearchParams as any)[key]
    if (!cliFileContents.includes(value))
      throw new Error(
        `the file ${cliFileToEdit} does not contain any ${value} string to replace`,
      )
  }

  fs.writeFileSync(
    cliFileToEdit,
    cliFileContents.replace(
      cliSearchParams.auth,
      process.env.SENTRY_AUTH_TOKEN,
    ),
  )
}

main().catch((err) => {
  console.error(err)
  process.exit(1)
})
