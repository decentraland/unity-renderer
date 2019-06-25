// tslint:disable:no-console

import { resolve } from 'path'
// a import fetch = require('node-fetch')
import express = require('express')
import multer = require('multer')
import path = require('path')
import fs = require('fs')
import mkdirp = require('mkdirp')
import * as BlinkDiff from 'blink-diff'
import titere = require('titere')
import WebSocket = require('ws')
import http = require('http')
import proto = require('../packages/shared/comms/proto/broker')

// defines if we should run headless tests and exit (true) or keep the server on (false)
const singleRun = !(process.env.SINGLE_RUN === 'true')

// defines if we should replace the base images
const shouldGenerateNewImages = process.env.GENERATE_NEW_IMAGES === 'true'

const port = process.env.PORT || 8080

const resultsDir = path.resolve(__dirname, '../test/results')
const tmpDir = path.resolve(__dirname, '../test/tmp')
const diffDir = path.resolve(__dirname, '../test/diff')
const storage = multer.diskStorage({
  destination: function(req, file, cb) {
    const destination = path.resolve(tmpDir, file.fieldname)
    cb(null, destination)
  },
  filename: function(req, file, cb) {
    cb(null, file.originalname)
  }
})

const upload = multer({ storage })
const app = express()

const server = http.createServer(app)
const wss = new WebSocket.Server({ server })

const connections = new Set<WebSocket>()
const topicsPerConnection = new WeakMap<WebSocket, Set<string>>()
let connectionCounter = 0

function getTopicList(socket: WebSocket): Set<string> {
  let set = topicsPerConnection.get(socket)
  if (!set) {
    set = new Set()
    topicsPerConnection.set(socket, set)
  }
  return set
}

wss.on('connection', function connection(ws) {
  connections.add(ws)
  const alias = ++connectionCounter

  ws.on('message', message => {
    const data = message as Buffer
    const msgType = proto.CoordinatorMessage.deserializeBinary(data).getType()

    if (msgType === proto.MessageType.PING) {
      ws.send(data)
    } else if (msgType === proto.MessageType.TOPIC) {
      const topicMessage = proto.TopicMessage.deserializeBinary(data)

      const topic = topicMessage.getTopic()

      const dataMessage = new proto.DataMessage()
      dataMessage.setType(proto.MessageType.DATA)
      dataMessage.setFromAlias(alias)
      dataMessage.setBody(topicMessage.getBody_asU8())

      const topicData = dataMessage.serializeBinary()

      // Reliable/unreliable data
      connections.forEach($ => {
        if (ws !== $) {
          if (getTopicList($).has(topic)) {
            $.send(topicData)
          }
        }
      })
    } else if (msgType === proto.MessageType.TOPIC_SUBSCRIPTION) {
      const topicMessage = proto.TopicSubscriptionMessage.deserializeBinary(data)
      const rawTopics = topicMessage.getTopics()
      const topics = Buffer.from(rawTopics).toString('utf8')
      const set = getTopicList(ws)

      set.clear()
      topics.split(/\s+/g).forEach($ => set.add($))
    }
  })

  ws.on('close', () => connections.delete(ws))

  setTimeout(() => {
    const welcome = new proto.WelcomeMessage()
    welcome.setType(proto.MessageType.WELCOME)
    welcome.setAlias(alias)
    const data = welcome.serializeBinary()

    ws.send(data)
  }, 100)
})

function getFile(files: any): Express.Multer.File {
  return files[Object.keys(files)[0]]
}

function checkDiff(imageAPath: string, imageBPath: string, threshold: number, diffOutputPath: string): Promise<number> {
  return new Promise((resolve, reject) => {
    mkdirp.sync(diffDir)

    const diff = new BlinkDiff({
      imageAPath,
      imageBPath,
      thresholdType: BlinkDiff.THRESHOLD_PERCENT,
      threshold,
      imageOutputLimit: BlinkDiff.OUTPUT_DIFFERENT,
      imageOutputPath: diffOutputPath
    })

    diff.run(function(error, result) {
      if (error) {
        reject(error)
      } else {
        if (diff.hasPassed(result.code)) {
          resolve(result.differences)
        } else {
          reject(new Error(`Found ${result.differences} differences`))
        }
      }
    })
  })
}

/// --- SIDE EFFECTS ---
{
  mkdirp.sync('test/tmp')

  app.use(require('cors')())

  app.get('/test', (req, res) => {
    res.writeHead(200, 'OK', {
      'Content-Type': 'text/html'
    })

    res.write(`<!DOCTYPE html>
      <html>

      <head>
        <title>Mocha Tests</title>
        <meta charset="utf-8">
        <link rel="stylesheet" href="/node_modules/mocha/mocha.css">
      </head>

      <body>
        <div id="mocha"></div>
        <script>console.log('test html loaded')</script>
        <script src="/node_modules/mocha/mocha.js"></script>
        <script>mocha.setup('bdd');</script>
        <script src="/test/out/index.js"></script>
      </body>

      </html>
    `)

    res.end()
  })

  app.use(
    '/@/artifacts/preview.js',
    express.static(resolve(__dirname, '../static/dist/preview.js'), {
      setHeaders: res => {
        res.setHeader('Content-Type', 'application/javascript')
      }
    })
  )

  app.use('/@', express.static(resolve(__dirname, '../packages/decentraland-ecs')))

  app.use(
    '/preview.html',
    express.static(resolve(__dirname, '../static/preview.html'), {
      setHeaders: res => {
        res.setHeader('Content-Type', 'text/html')
      }
    })
  )

  function getAllParcelIdsBetween(coords: { x1: string; x2: string; y1: string; y2: string }) {
    const result = []
    try {
      const x1 = parseInt(coords.x1, 10)
      const x2 = parseInt(coords.x2, 10)
      const y1 = parseInt(coords.y1, 10)
      const y2 = parseInt(coords.y2, 10)
      for (let i = x1; i <= x2; i++) {
        for (let j = y1; j <= y2; j++) {
          result.push(`${i},${j}`)
        }
      }
      return result
    } catch (e) {
      throw new TypeError('')
    }
  }

  function readAllJsonFiles(filenames: string[]) {
    return Promise.all(
      filenames.map(value => {
        return new Promise((res, reject) => {
          return fs.readFile(value, (err, data) => {
            if (err) {
              return res(null)
            }
            return res(JSON.parse(data.toString()))
          })
        })
      })
    )
  }

  const sceneEndpoint = async (req, res) => {
    const coords = getAllParcelIdsBetween(req.query)
    const fileData = await readAllJsonFiles(
      coords.map(coord => resolve(__dirname, path.join('..', 'public', 'local-ipfs', 'scene_mapping', coord)))
    )
    const length = coords.length
    const result = []
    for (let i = 0; i < length; i++) {
      if (!fileData[i]) continue
      result.push(fileData[i])
    }
    return res.json({ data: result })
  }
  app.use('/scenes', sceneEndpoint)
  app.use('/local-ipfs/scenes', sceneEndpoint)

  const parcelInfoEndpoint = async (req, res) => {
    const cids = req.query.cids.split(',') as string[]
    const fileData = await readAllJsonFiles(
      cids.map(_ => resolve(__dirname, path.join('..', 'public', 'local-ipfs', 'parcel_info', _)))
    )
    return res.json({
      data: fileData
        .filter($ => !!$)
        .map(($: any) => ({
          root_cid: $.root_cid,
          publisher: $.publisher,
          content: $
        }))
    })
  }
  app.use('/local-ipfs/parcel_info', parcelInfoEndpoint)
  app.use('/parcel_info', parcelInfoEndpoint)

  app.use('/test', express.static(resolve(__dirname, '../test')))
  app.use('/node_modules', express.static(resolve(__dirname, '../node_modules')))
  app.use('/test-parcels', express.static(path.resolve(__dirname, '../public/test-parcels')))
  app.use('/ecs-parcels', express.static(path.resolve(__dirname, '../public/ecs-parcels')))
  app.use('/local-ipfs', express.static(path.resolve(__dirname, '../public/local-ipfs')))

  app.use(express.static(path.resolve(__dirname, '..', 'static')))

  app.post('/upload', upload.any(), function(req, res) {
    const threshold = 0.01

    const file = getFile(req.files)

    const tmpPath = path.resolve(file.destination, file.filename)
    const resultPath = path.resolve(resultsDir, req.query.path)
    const outputDiffFile = path.resolve(diffDir, req.query.path)

    mkdirp.sync(path.dirname(tmpPath))
    mkdirp.sync(path.dirname(resultPath))

    console.log(`      uploading to: ${tmpPath}`)
    console.log(`       target file: ${resultPath}`)
    console.log(`       output diff: ${outputDiffFile}`)

    // if the file to compare does not exist and we are uploading a new file
    if ((shouldGenerateNewImages && fs.existsSync(tmpPath)) || !fs.existsSync(resultPath)) {
      // move it to the final path
      fs.renameSync(tmpPath, resultPath)
      res.writeHead(201)
      res.end()
      return
    }

    // make sure the directory where we store the differences exists
    mkdirp.sync(path.dirname(outputDiffFile))

    const promise = checkDiff(resultPath, tmpPath, threshold, outputDiffFile)

    promise
      .then($ => {
        console.log(`       differences: ${$}`)
        res.writeHead(200)
        res.end()
      })
      .catch(e => {
        console.log(`    generating img: ${shouldGenerateNewImages} `)
        console.log(`             error: ${e} `)
        if (shouldGenerateNewImages) {
          // If the diff fails, it means images are different enough to be
          // commited as a test result to the repo.

          if (fs.existsSync(tmpPath)) {
            fs.renameSync(tmpPath, resultPath)
            console.log(`                mv: ${tmpPath} -> ${resultPath}`)
          }

          res.writeHead(201)
          res.end()
        } else {
          res.writeHead(500, e.message)
          res.end()
        }
      })
  })

  server.listen(port, function() {
    console.info('==>     Listening on port %s. Open up http://localhost:%s/test to run tests', port, port)
    console.info('                              Open up http://localhost:%s/ to test the client.', port)

    const options: titere.Options = {
      file: `http://localhost:${port}/test`,
      visible: true,
      height: 600,
      width: 800,
      timeout: 5 * 60 * 1000,
      args: ['--no-sandbox', '--disable-setuid-sandbox', '--debug-devtools-frontend', '--js-flags="--expose-gc"']
    }

    if (!singleRun) {
      titere
        .run(options)
        .then(result => {
          if (result.coverage) {
            fs.writeFileSync('test/tmp/out.json', JSON.stringify(result.coverage))
          }
          process.exit(result.result.stats.failures)
        })
        .catch((err: Error) => {
          console.error(err.message || JSON.stringify(err))
          console.dir(err)
          process.exit(1)
        })
    }
  })
}
