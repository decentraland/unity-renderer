#!/usr/bin/env node

// We use esnext in dcl-sdk, webpack and rollup handle it natively but not Node.js

const traceur = require('traceur')

// replace node.js require by traceur's
traceur.require.makeDefault(
  function(filename: string) {
    // don't transpile our dependencies, just our app
    return filename.indexOf('node_modules') === -1
  },
  {
    asyncFunctions: true,
    asyncGenerators: true
  }
)

import { resolve } from 'path'
import * as http from 'http'
import * as express from 'express'
import titere = require('titere')
import fs = require('fs')

const WS = require('../test/server/_testWebSocketServer')

const keepOpen = process.argv.some($ => $ === '--keep-open')
const app = express()
const port = process.env.PORT || 3000
const server = http.createServer(app)

WS.initializeWebSocketTester(server)

// serve build.html
app.get('/', function(req, res) {
  res.sendFile(resolve(__dirname, '../test/index.html'))
})

console.log(resolve(__dirname, '../node_modules'))

app.use('/test', express.static(resolve(__dirname, '../test')))
app.use('/node_modules', express.static(resolve(__dirname, '../node_modules')))

server.listen(port, function(error: any) {
  if (error) {
    console.error(error)
    process.exit(1)
  } else {
    console.info('==> 🌎  Listening on port %s. Open up http://localhost:%s/ in your browser.', port, port)

    const options: titere.Options = {
      file: `http://localhost:${port}`,
      visible: keepOpen,
      height: 600,
      width: 800,
      timeout: 5 * 60 * 1000,
      args: ['--no-sandbox', '--disable-setuid-sandbox']
    }

    titere
      .run(options)
      .then(result => {
        if (result.coverage) {
          fs.writeFileSync('test/out/out.json', JSON.stringify(result.coverage))
        } else {
          console.error('Coverage data not found')
          process.exit(1)
        }
        if (!keepOpen) process.exit(result.result.stats.failures)
      })
      .catch((err: Error) => {
        console.error(err.message || JSON.stringify(err))
        console.dir(err)
        if (!keepOpen) process.exit(1)
      })
  }
})

server.on('error', e => console.log(e))
