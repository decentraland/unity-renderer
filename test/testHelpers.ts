import { dirname, basename } from 'path'

import * as BABYLON from 'babylonjs'
import { future } from 'fp-future'
import { domReadyFuture } from 'engine'

import { setSize, scene, initDCL, onWindowResize, engineMicroQueue } from 'engine/renderer'
import { initKeyboard } from 'engine/renderer/input'
import { reposition } from 'engine/renderer/ambientLights'

import { start } from 'engine/dcl'

import { sleep, untilNextFrame } from 'atomicHelpers/sleep'
import { expect } from 'chai'
import { SceneDataDownloadManager } from 'decentraland-loader/lifecycle/controllers/download'
import { bodyReadyFuture } from 'engine/renderer/init'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { loadParcelScene, stopParcelSceneWorker } from 'shared/world/parcelSceneManager'
import { WebGLParcelScene } from 'engine/dcl/WebGLParcelScene'
import { ILandToLoadableParcelScene } from 'shared/types'
import { SceneWorker } from 'shared/world/SceneWorker'
import { MemoryTransport } from 'decentraland-rpc'

import GamekitScene from '../packages/scene-system/scene.system'
import { gridToWorld } from 'atomicHelpers/parcelScenePositions'
import { BasicShape } from 'engine/components/disposableComponents/DisposableComponent'
import { initHudSystem } from 'engine/dcl/widgets/ui'
import { AVATAR_OBSERVABLE } from 'decentraland-ecs/src/decentraland/Types'
import { deleteUnusedTextures } from 'engine/renderer/monkeyLoader'

const port = process.env.PORT || 8080

export type PlayerCamera = {
  lookAt: [number, number, number]
  from: [number, number, number]
}

declare var gc: any
declare var it: any
declare var describe: any

let count = 0

/**
 * This method should be located directly inside a `describe` body.
 * NOT INSIDE A `it`
 * @param ms milliseconds to wait
 */
export function wait(ms: number) {
  it(`wait ${ms}ms #${count++}`, async function(this: any) {
    this.timeout(ms + 1000)
    await sleep(ms)
    await untilNextFrame()
  })
}

function filterBabylonMaterials(material: BABYLON.Material) {
  return !['colorShader'].includes(material.name)
}
function filterBabylonTextures(texture: BABYLON.BaseTexture) {
  return !['HighlightLayerMainRTT', 'GlowLayerBlurRTT', 'GlowLayerBlurRTT2', 'VerifiedBadge'].includes(texture.name)
}

/**
 * This method should be located directly inside a `describe` body.
 * NOT INSIDE A `it`
 * @param name name of the file to save
 * @param path folder
 */
export function saveScreenshot(name: string, opts: PlayerCamera | null = null, skip: boolean = false) {
  const testFn = skip ? it.skip : it
  testFn(`save the screenshot ${name} #${count++}`, async function(this: any) {
    // tslint:disable-next-line:no-console
    this.timeout(20000)
    const canvas = await domReadyFuture

    await untilNextFrame()

    await positionCamera(opts)

    await untilNextFrame()

    const resolvable = future<Response>()

    canvas.toBlob(function(blob) {
      if (!blob || blob.size === 0) {
        resolvable.reject(new Error('Could not generate a valid screenshot'))
        return
      }

      const fd = new FormData()
      const baseName = basename(name)
      fd.append(dirname(name), blob, baseName)

      fetch(`http://${location.hostname}:${port}/upload?path=${name}`, {
        method: 'post',
        mode: 'cors',
        body: fd
      })
        .then($ => {
          if ($.ok) {
            resolvable.resolve($)
          } else {
            resolvable.reject(new Error($.statusText))
          }
        })
        .catch(e => {
          resolvable.reject(e)
        })
    }, 'image/png')

    await resolvable
  })
}

function getSceneNumbers() {
  return {
    transformNodes: scene.transformNodes.slice(),
    rootNodes: scene.rootNodes.slice(),
    sounds: (scene.sounds && scene.sounds.slice()) || [],
    skeletons: scene.skeletons.slice(),
    animatables: scene.animatables.slice(),
    animationGroups: scene.animationGroups.slice(),
    animations: scene.animations.slice(),
    geometries: scene.geometries.slice(),
    materials: scene.materials.slice(),
    meshes: scene.meshes.slice(),
    textures: scene.textures.slice()
  }
}

async function initHud() {
  initKeyboard()
  const canvas = initDCL()
  const body = await bodyReadyFuture
  if (!canvas.parentElement) {
    body.appendChild(canvas)
  }
  // Test output will be 800x600
  setSize(800, 600)
  onWindowResize()

  await untilNextFrame()

  let attempts = 0

  while (!scene) {
    if (attempts++ > 10) throw new Error('!scene')
    await sleep(300)
  }

  const hudScene = await initHudSystem()
  const system = await hudScene.worker!.system
  const socialController = system.getAPIInstance(EngineAPI)

  attempts = 0

  while (!(AVATAR_OBSERVABLE in socialController.subscribedEvents)) {
    if (attempts++ > 10) throw new Error(AVATAR_OBSERVABLE + ' not subscribed')
    await sleep(300)
  }

  await untilNextFrame()

  return hudScene
}

let hud = initHud()

/**
 * Enables visual tests
 * This method should be located directly inside a `describe` body.
 * NOT INSIDE A `it`
 */
export function enableVisualTests(name: string, cb: (root: BABYLON.TransformNode) => void) {
  describe(name, () => {
    let root = new BABYLON.TransformNode('rootForTests')

    scene.removeTransformNode(root)

    let initialNumbers: ReturnType<typeof getSceneNumbers> | null = null

    it('Cleans the scene and append a new root', async () => {
      while (true) {
        let previousRoot = scene.getTransformNodeByName('rootForTests')
        if (previousRoot && previousRoot !== root) {
          previousRoot.setParent(null)
          previousRoot.dispose()
        } else {
          break
        }
      }

      await hud

      initialNumbers = getSceneNumbers()

      scene.addTransformNode(root)

      if (typeof gc === 'function') {
        gc()
      }
    })

    it('start the renderer', async () => {
      await hud
    })

    try {
      cb(root)
    } catch (e) {
      if (e) {
        it('failed during test descriptor', () => {
          throw e
        })
      }
    }

    it('cleans up the scene', async () => {
      await untilNextFrame()

      scene.removeTransformNode(root)

      root.setParent(null)
      root.dispose()

      deleteUnusedTextures()

      engineMicroQueue.flushMicroTaskQueue()
      engineMicroQueue.flushTaskQueue()

      await untilNextFrame()

      if (typeof gc === 'function') {
        gc()
      }
    })

    it('the number of elements in the scene should remain the same as the begin', () => {
      let errors = []
      const actual = getSceneNumbers()

      for (let i in actual) {
        if (i === 'materials') {
          actual[i] = actual[i].filter(filterBabylonMaterials)
          initialNumbers![i] = initialNumbers![i].filter(filterBabylonMaterials)
        }

        if (i === 'textures') {
          actual[i] = actual[i].filter(filterBabylonTextures)
          initialNumbers![i] = initialNumbers![i].filter(filterBabylonTextures)
        }

        if (i === 'rootNodes') {
          actual[i] = actual[i].filter($ => $.name !== 'rootForTests')
          initialNumbers![i] = initialNumbers![i].filter($ => $.name !== 'rootForTests')
        }

        if ((actual as any)[i].length !== (initialNumbers as any)[i].length) {
          errors.push(`${i}: actual ${(actual as any)[i].length} != ${(initialNumbers as any)[i].length} expected`)

          // tslint:disable-next-line:no-console
          console.log(
            'extra in actual ' + i,
            (actual as any)[i].filter(($: any) => !(initialNumbers as any)[i].includes($))
          )
          // tslint:disable-next-line:no-console
          console.log(
            'missing in actual ' + i,
            (initialNumbers as any)[i].filter(($: any) => !(actual as any)[i].includes($))
          )
        }
      }

      if (errors.length) {
        throw new Error(errors.join('\n'))
      }
    })
  })
}

export async function waitToBeLoaded(entity: BaseEntity) {
  await sleep(1000)

  while (true) {
    const children = entity.childEntities()

    if (children.length == 0) {
      await sleep(100)
      continue
    }

    const loadingEntity = entity.getLoadingEntity()

    if (loadingEntity) {
      await sleep(100)
      continue
    }

    break
  }
}

export async function waitForMesh(entity: BaseEntity) {
  while (true) {
    const children = entity.getObject3D(BasicShape.nameInEntity)

    if (children) return

    await sleep(100)
  }
}

const downloader = new SceneDataDownloadManager({ contentServer: `http://localhost:${port}/local-ipfs` })

/**
 * It loads and attach a test parcel into the testing scene
 */
export function loadTestParcel(
  name: string,
  x: number,
  y: number,
  cb: (
    root: BABYLON.TransformNode,
    webGLParcelScene: Promise<WebGLParcelScene>,
    parcelSceneFuture: Promise<SceneWorker>
  ) => void,
  skip: boolean = false
) {
  enableVisualTests(name, root => {
    const _parcelScene = future<SceneWorker>()
    const _glParcelScene = future<WebGLParcelScene>()
    let context: SharedSceneContext
    const testFn = skip ? it.skip : it
    testFn(`loads the test scene at ${x},${y}`, async function(this: any) {
      const origY = scene.activeCamera!.position.y
      gridToWorld(x, y, scene.activeCamera!.position)
      scene.activeCamera!.position.y = origY
      this.timeout(15000)
      const land = await downloader.getParcelData(`${x},${y}`)
      let webGLParcelScene: WebGLParcelScene
      if (land) {
        webGLParcelScene = new WebGLParcelScene(ILandToLoadableParcelScene(land))

        _glParcelScene.resolve(webGLParcelScene)

        const parcelSceneWorker = loadParcelScene(webGLParcelScene)

        context = webGLParcelScene.context
        context.rootEntity.onDisposeObservable.add(() => {
          stopParcelSceneWorker(parcelSceneWorker)
        })

        _parcelScene.resolve(parcelSceneWorker)
      } else {
        _parcelScene.reject(new Error(`unknown mock at ${x},${y}`))
      }

      const parcelScene = await _parcelScene
      const system = await parcelScene.system

      while (!system.isEnabled) {
        await sleep(100)
      }

      system.getAPIInstance(EngineAPI)

      await sleep(100)

      await waitToBeLoaded(webGLParcelScene!.context.rootEntity)
    })
    try {
      cb(root, _glParcelScene, _parcelScene)
    } catch (e) {
      if (e) {
        testFn('failed during test descriptor', () => {
          throw e
        })
      }
    }
    testFn('cleans up the parcelScene', async () => {
      const parcelScene = await _parcelScene
      parcelScene.dispose()

      expect(context.isDisposed()).to.eq(true, 'context is disposed')
    })
  })
}

function LookAtRef(camera: BABYLON.TargetCamera, target: BABYLON.Vector3, ref: BABYLON.Quaternion) {
  const result = BABYLON.Matrix.Zero()
  BABYLON.Matrix.LookAtLHToRef(camera.position, target, new BABYLON.Vector3(0, 1, 0), result)
  result.invert()
  BABYLON.Quaternion.FromRotationMatrixToRef(result, ref)
}

export async function positionCamera(opts: PlayerCamera | null = null) {
  await untilNextFrame()

  const camera = scene.activeCamera as BABYLON.FreeCamera

  if (opts) {
    gridToWorld(opts.from[0], opts.from[2], camera.position)
    camera.position.y = opts.from[1]

    if (!camera.rotationQuaternion) {
      camera.rotationQuaternion = BABYLON.Quaternion.Identity()
    }

    const at = new BABYLON.Vector3()
    gridToWorld(opts.lookAt[0], opts.lookAt[2], at)
    at.y = opts.lookAt[1]

    LookAtRef(camera, at, camera.rotationQuaternion)
    camera._getViewMatrix()
    camera.update()
  }

  await untilNextFrame()

  if (opts) {
    gridToWorld(opts.from[0], opts.from[2], camera.position)
    camera.position.y = opts.from[1]
  }

  reposition()
}

export function testScene(
  x: number,
  y: number,
  cb: (promises: {
    ensureNoErrors: () => void
    sceneHost: GamekitScene
    parcelScenePromise: Promise<WebGLParcelScene>
    root: BABYLON.TransformNode
    logs: any[]
  }) => void,
  manualUpdate = false
) {
  enableVisualTests(`testScene ${x},${y}`, root => {
    const transport = MemoryTransport()

    const parcelScenePromise = future<WebGLParcelScene>()
    const sceneHost = new GamekitScene(transport.client)
    const logs: any[] = []

    sceneHost.manualUpdate = manualUpdate

    it('loads the mock and starts the system', async function(this: any) {
      this.timeout(5000)

      const land = await downloader.getParcelData(`${x},${y}`)

      try {
        if (!land) throw new Error(`Cannot load the parcel at ${x},${y}`)
        const loadableParcelScene = ILandToLoadableParcelScene(land)

        parcelScenePromise.resolve(new WebGLParcelScene(loadableParcelScene))
      } catch (e) {
        parcelScenePromise.reject(e)
        throw e
      }
    })

    it('waits for the system to be loaded and ready', async function(this: any) {
      this.timeout(5000)
      const parcelScene = await parcelScenePromise

      const originalLog = parcelScene.context.logger.log
      parcelScene.context.logger.log = function(...args: any[]) {
        logs.push(args)
        return (originalLog as any).apply(this, args)
      }

      const worker = loadParcelScene(parcelScene, transport.server)
      // keep this to avoid regressions
      await worker.system

      while (!sceneHost.didStart) {
        await sleep(10)
      }

      await parcelScene.context
    })

    try {
      cb({
        ensureNoErrors: () => {
          expect(sceneHost.devToolsAdapter!.exceptions.length).to.eq(
            0,
            `Found some(${sceneHost.devToolsAdapter!.exceptions.length}) errors`
          )
        },
        sceneHost,
        parcelScenePromise,
        root,
        logs
      })
    } catch (e) {
      if (e) {
        it('failed during test descriptor', () => {
          throw e
        })
      }
    }

    it('cleans up the parcelScene', async () => {
      const parcelScene = await parcelScenePromise
      const rootEntity = parcelScene.context.rootEntity
      const worker = await parcelScene.worker
      parcelScene.dispose()
      expect(rootEntity.isDisposed()).to.eq(true)
      expect(scene.getTransformNodesByID(rootEntity.id).length).to.eq(0, 'ParcelScene is still in the scene')
      if (worker) {
        stopParcelSceneWorker(worker)
      }
    })
  })
}

export async function awaitHud() {
  it('await hud system to be ready', async function(this: any) {
    this.timeout(10000)
    await hud
  })
  return hud
}

describe('Avatar hud initialization', () => {
  awaitHud()
})

start()
