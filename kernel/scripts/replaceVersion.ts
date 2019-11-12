#!/usr/bin/env node
// tslint:disable:no-console

import fs = require('fs-extra')
import path = require('path')
import { readFileSync, writeFileSync } from 'fs-extra'
import { execSync } from 'child_process'

const root = path.resolve(__dirname, '..')
const commitHash = execSync('git rev-parse HEAD')
  .toString()
  .trim()

function replaceVersion(placeholder: string) {
  const targetIndexHtml = path.resolve(root, 'static/index.html')

  if (!fs.existsSync(targetIndexHtml)) {
    throw new Error(`${targetIndexHtml} does not exist`)
  }

  const version = process.env.CIRCLE_TAG || process.env.TRAVIS_TAG || commitHash

  console.log(`> replace '${placeholder}' -> '${version}' in html`)
  {
    let content = readFileSync(targetIndexHtml).toString()

    if (!content.includes(`${placeholder}`)) {
      throw new Error(`index.html is dirty and does\'t contain the text '${placeholder}'`)
    }

    content = content.replace(new RegExp(placeholder, 'g'), version)

    writeFileSync(targetIndexHtml, content)
  }
}

// tslint:disable-next-line:semicolon
;(async function() {
  replaceVersion('EXPLORER_VERSION')
})().catch(e => {
  // tslint:disable-next-line:no-console
  console.error(e)
  process.exit(1)
})
