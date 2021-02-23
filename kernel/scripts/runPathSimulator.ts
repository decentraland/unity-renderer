// tslint:disable:no-console
import express = require('express')
import WebSocket = require('ws')
import http = require('http')

const url = require('url')

// ******************************************************************************************************* //
// ***** Movements ***** //
// ******************************************************************************************************* //

const Sense = {
  POSITIVE: step => (current, target) => {
    const next = current + step
    return next >= target ? target : next
  },
  NEGATIVE: step => (current, target) => {
    const next = current - step
    return next <= target ? target : next
  }
}

function sequence(...moves) {
  return function*() {
    for (const move of moves) {
      yield* move()
    }
  }
}

function loop(...moves) {
  return function*() {
    while (true) {
      for (const move of moves) {
        yield* move()
      }
    }
  }
}

function logOut() {
  return function*() {
    return { x: 16 * 1000, y: 1000, z: 16 * 1000 }
  }
}

function walkTo(x: number, z: number) {
  return function*() {
    let position = yield

    const moveTowardsTargetX = (x > position.x ? Sense.POSITIVE : Sense.NEGATIVE)(2)
    const moveTowardsTargetZ = (z > position.z ? Sense.POSITIVE : Sense.NEGATIVE)(2)

    while (position.x !== x || position.z !== z) {
      position = yield { x: moveTowardsTargetX(position.x, x), y: position.y, z: moveTowardsTargetZ(position.z, z) }
    }
  }
}

function walk(x: number, z: number) {
  return function*() {
    let position = yield

    const target = { x: x + position.x, z: z + position.z }

    yield* walkTo(target.x, target.z)()
  }
}

// ******************************************************************************************************* //
// ***** Paths ***** //
// ******************************************************************************************************* //

const path = (name: string) => {
  switch (name) {
    case '/loop-p21':
      return loop(walkTo(320, 320), walkTo(384, 320), walkTo(384, 384), walkTo(320, 384))
    case '/rloop-p21':
      return loop(walkTo(320, 320), walkTo(320, 384), walkTo(384, 384), walkTo(384, 320))
    case '/loop':
      return loop(walk(20, 0), walk(0, 20), walk(-20, 0), walk(0, -20))
    case '/rloop':
      return loop(walk(0, 20), walk(20, 0), walk(0, -20), walk(-20, 0))
    case '/walk':
      return sequence(
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        walkTo(320, 384),
        walkTo(384, 320),
        logOut()
      )
  }
}

// ******************************************************************************************************* //
// ***** Engine simulator server ***** //
// ******************************************************************************************************* //

type Event =
  | 'Reset'
  | 'DeactivateRendering'
  | 'SetDebug'
  | 'CreateGlobalScene'
  | 'ConfigureMinimapHUD'
  | 'ConfigureAvatarHUD'
  | 'ConfigureAvatarEditorHUD'
  | 'ConfigureSettingsHUD'
  | 'UpdateMinimapSceneInformation'
  | 'SetRotation'
  | 'ConfigureNotificationHUD'
  | 'SendSceneMessage'
  | 'LoadParcelScenes'
  | 'AddWearableToCatalog'
  | 'Teleport'
  | 'ActivateRendering'
  | 'UnloadScene'
  | 'LoadProfile'
  | string

type Message = {
  type: Event
  payload?: string
}

const port = process.env.PORT || 5001

const app = express()

const server = http.createServer(app)
const wss = new WebSocket.Server({ server })

let connectionCounter = 0

wss.on('connection', function connection(ws, req) {
  const patho = url.parse(req.url, true).pathname

  let currentPosition
  const alias = ++connectionCounter

  ws.on('message', message => {
    const data: Message = JSON.parse(message as string)

    switch (data.type) {
      case 'Reset':
      case 'SetDebug':
      case 'CreateGlobalScene':
      case 'ConfigureMinimapHUD':
      case 'ConfigureAvatarHUD':
      case 'ConfigureNotificationHUD':
      case 'ConfigureAvatarEditorHUD':
      case 'ConfigureSettingsHUD':
      case 'UpdateMinimapSceneInformation':
      case 'SetRotation':
      case 'SendSceneMessage':
      case 'AddWearableToCatalog':
      case 'RemoveWearablesFromCatalog':
      case 'UnloadScene':
      case 'LoadProfile': {
        // ignore
        break
      }
      case 'LoadParcelScenes': {
        // delay + answer scene ready message
        const sceneId = JSON.parse(data.payload).id
        console.log(`${alias}: loading parcel ${sceneId}`)
        const response = {
          type: 'ControlEvent',
          payload: JSON.stringify({ eventType: 'SceneReady', payload: { sceneId } })
        }
        setTimeout(() => {
          console.log(`${alias}: scene ready ${sceneId}`)
          ws.send(JSON.stringify(response))
        }, 200)
        break
      }
      case 'Teleport': {
        const teleport = JSON.parse(data.payload)
        currentPosition = teleport
        console.log(`${alias}: teleporting to ${JSON.stringify(teleport)}`)
        break
      }
      case 'DeactivateRendering': {
        // pause sending positions
        console.log(`${alias}: deactivated rendering`)
        break
      }
      case 'ActivateRendering': {
        // resume sending positions + send ack
        console.log(`${alias}: activated rendering`)
        const response = {
          type: 'ControlEvent',
          payload: JSON.stringify({ eventType: 'ActivateRenderingACK' })
        }
        ws.send(JSON.stringify(response))

        console.log(`${alias}: path to be used ${patho}`)
        resume(currentPosition, path(patho), ws, alias).catch(console.log)
        break
      }
      default: {
        console.log(`${alias}: Unknown message type ${data.type}`)
        break
      }
    }
  })

  ws.on('close', () => {
    // closing
  })
})

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
}

async function resume(position: any, path: () => Generator, ws: WebSocket, alias: number) {
  const iterator = path()

  let current = position
  let done = false
  while (!done) {
    let next = iterator.next(current)
    current = next.value || current
    done = next.done

    if (current) {
      const response = {
        type: 'ReportPosition',
        payload: JSON.stringify({ position: current, rotation: { x: 0, y: 0, z: 0, w: 0 }, playerHeight: 2 })
      }
      ws.send(JSON.stringify(response))
    }

    await sleep(200)
  }
}

server.listen(port, () => {
  console.info('==>     Path simulator listening on port %s.', port)
})
