import { dirname, basename } from 'path'

import * as BABYLON from 'babylonjs'
import { future } from 'fp-future'
import { domReadyFuture } from 'engine'

import { error } from 'engine/logger'

import { setSize, scene, initDCL, onWindowResize, engineMicroQueue } from 'engine/renderer'
import { initKeyboard } from 'engine/renderer/input'
import { reposition } from 'engine/renderer/ambientLights'

import { start, stop } from 'dcl'

import { resolveUrl } from 'atomicHelpers/parseUrl'
import { sleep, untilNextFrame } from 'atomicHelpers/sleep'
import { expect } from 'chai'
import { bodyReadyFuture } from 'engine/renderer/init'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { EngineAPI } from 'shared/apis/EngineAPI'
import { loadedParcelSceneWorkers } from 'shared/world/parcelSceneManager'
import { WebGLParcelScene } from 'dcl/WebGLParcelScene'
import { ILandToLoadableParcelScene, ILand, MappingsResponse, IScene } from 'shared/types'
import { SceneWorker } from 'shared/world/SceneWorker'
import { MemoryTransport } from 'decentraland-rpc'

import GamekitScene from '../packages/scene-system/scene.system'
import { gridToWorld } from 'atomicHelpers/parcelScenePositions'
import { BasicShape } from 'engine/components/disposableComponents/DisposableComponent'

const baseUrl = 'http://localhost:8080/local-ipfs/contents/'

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
  it(`wait ${ms}ms #${count++}`, async function() {
    this.timeout(ms + 1000)
    await sleep(ms)
    await untilNextFrame()
  })
}

function filterBabylonMaterials(material: BABYLON.Material) {
  return !['colorShader'].includes(material.name)
}
function filterBabylonTextures(texture: BABYLON.Texture) {
  return !['HighlightLayerMainRTT', 'GlowLayerBlurRTT', 'GlowLayerBlurRTT2', 'VerifiedBadge'].includes(texture.name)
}

/**
 * This method should be located directly inside a `describe` body.
 * NOT INSIDE A `it`
 * @param name name of the file to save
 * @param path folder
 */
export function saveScreenshot(name: string, opts: PlayerCamera = null) {
  it(`save the screenshot ${name} #${count++}`, async function() {
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

      fetch(`http://${location.hostname}:8080/upload?path=${name}`, {
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
    sounds: scene.sounds.slice(),
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

/**
 * Enables visual tests
 * This method should be located directly inside a `describe` body.
 * NOT INSIDE A `it`
 */
export function enableVisualTests(name: string, cb: (root: BABYLON.TransformNode) => void) {
  describe(name, () => {
    let root = new BABYLON.TransformNode('rootForTests')

    scene.removeTransformNode(root)

    let initialNumbers: ReturnType<typeof getSceneNumbers> = null

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

      initialNumbers = getSceneNumbers()

      scene.addTransformNode(root)

      if (typeof gc === 'function') {
        gc()
      }
    })

    it('start the renderer', async () => {
      initKeyboard()
      start()
      const canvas = initDCL()
      const body = await bodyReadyFuture
      if (!canvas.parentElement) {
        body.appendChild(canvas)
      }
      // Test output will be 800x600
      setSize(800, 600)
      onWindowResize()
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

      engineMicroQueue.flushMicroTaskQueue()
      engineMicroQueue.flushTaskQueue()

      await untilNextFrame()

      stop()

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
          initialNumbers[i] = initialNumbers[i].filter(filterBabylonMaterials)
        }

        if (i === 'textures') {
          actual[i] = actual[i].filter(filterBabylonTextures)
          initialNumbers[i] = initialNumbers[i].filter(filterBabylonTextures)
        }

        if (i === 'rootNodes') {
          actual[i] = actual[i].filter($ => $.name !== 'rootForTests')
          initialNumbers[i] = initialNumbers[i].filter($ => $.name !== 'rootForTests')
        }

        if (actual[i].length !== initialNumbers[i].length) {
          errors.push(`${i}: actual ${actual[i].length} != ${initialNumbers[i].length} expected`)

          // tslint:disable-next-line:no-console
          console.log('extra in actual ' + i, actual[i].filter($ => !initialNumbers[i].includes($)))
          // tslint:disable-next-line:no-console
          console.log('missing in actual ' + i, initialNumbers[i].filter($ => !actual[i].includes($)))
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
  ) => void
) {
  enableVisualTests(name, root => {
    const _parcelScene = future<SceneWorker>()
    const _glParcelScene = future<WebGLParcelScene>()
    let context: SharedSceneContext = null
    it(`loads the test scene at ${x},${y}`, async function() {
      const origY = scene.activeCamera.position.y
      gridToWorld(x, y, scene.activeCamera.position)
      scene.activeCamera.position.y = origY
      this.timeout(10000)
      const land = await loadMock('http://localhost:8080/local-ipfs/mappings', { x, y })
      let webGLParcelScene: WebGLParcelScene
      if (land) {
        webGLParcelScene = new WebGLParcelScene(ILandToLoadableParcelScene(land))

        _glParcelScene.resolve(webGLParcelScene)

        const parcelSceneWorker = new SceneWorker(webGLParcelScene)

        context = webGLParcelScene.context
        context.rootEntity.onDisposeObservable.add(() => {
          loadedParcelSceneWorkers.delete(parcelSceneWorker)
        })

        loadedParcelSceneWorkers.add(parcelSceneWorker)

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

      await waitToBeLoaded(webGLParcelScene.context.rootEntity)
    })
    try {
      cb(root, _glParcelScene, _parcelScene)
    } catch (e) {
      if (e) {
        it('failed during test descriptor', () => {
          throw e
        })
      }
    }
    it('cleans up the parcelScene', async () => {
      const parcelScene = await _parcelScene
      parcelScene.dispose()

      expect(context.isDisposed()).to.eq(true, 'context is disposed')
    })
  })
}

/**
 * Returns the scene data for a specific parcel.
 * This is a modified version of the function found on the land worker.
 * @param url The url pointing to the mock.json file
 * @param coords an object containing the X and Y positions of the parcel
 */
async function loadMock(url: string, coords: { x: number; y: number }): Promise<ILand | null> {
  const mockRequest = await fetch(url)

  if (!mockRequest.ok) {
    throw new Error(`Mock ${url} could not be loaded`)
  }

  const mock: MappingsResponse[] = await mockRequest.json()

  for (let parcel of mock) {
    try {
      const [x, y] = parcel.parcel_id.split(/,/).map($ => parseInt($, 10))

      if (x === coords.x && y === coords.y) {
        const sceneJsonMapping = parcel.contents.find($ => $.file === 'scene.json')

        if (!sceneJsonMapping) {
          throw new Error('scene.json not found in mock ' + parcel.parcel_id)
        }

        const sceneUrl = resolveUrl(baseUrl, sceneJsonMapping.hash)

        const sceneFetch = await fetch(sceneUrl)

        if (!sceneFetch.ok) {
          throw new Error('Error received in ' + sceneUrl + ': ' + (await sceneFetch.text()))
        }

        const scene = (await sceneFetch.json()) as IScene

        const mockedScene: ILand = {
          baseUrl,
          scene,
          mappingsResponse: parcel
        }

        return mockedScene
      }
    } catch (e) {
      error(`Error loading mock for ${parcel.parcel_id}`, e.message)
    }
  }
  return null
}

function LookAtRef(camera: BABYLON.TargetCamera, target: BABYLON.Vector3, ref: BABYLON.Quaternion) {
  const result = BABYLON.Matrix.Zero()
  BABYLON.Matrix.LookAtLHToRef(camera.position, target, new BABYLON.Vector3(0, 1, 0), result)
  result.invert()
  BABYLON.Quaternion.FromRotationMatrixToRef(result, ref)
}

export async function positionCamera(opts: PlayerCamera = null) {
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
  cb: (
    promises: {
      ensureNoErrors: () => void
      sceneHost: GamekitScene
      parcelScenePromise: Promise<WebGLParcelScene>
      root: BABYLON.TransformNode
      logs: any[]
    }
  ) => void,
  manualUpdate = false
) {
  enableVisualTests(`testScene ${x},${y}`, root => {
    const transport = MemoryTransport()

    const parcelScenePromise = future<WebGLParcelScene>()
    const sceneHost = new GamekitScene(transport.client)
    const logs = []

    sceneHost.manualUpdate = manualUpdate

    it('loads the mock and starts the system', async function() {
      this.timeout(5000)

      const land = await loadMock('http://localhost:8080/local-ipfs/mappings', { x, y })

      try {
        const loadableParcelScene = ILandToLoadableParcelScene(land)

        parcelScenePromise.resolve(new WebGLParcelScene(loadableParcelScene))
      } catch (e) {
        parcelScenePromise.reject(e)
        throw e
      }
    })

    it('waits for the system to be loaded and ready', async function() {
      this.timeout(5000)
      const parcelScene = await parcelScenePromise

      const originalLog = parcelScene.context.logger.log
      parcelScene.context.logger.log = function(...args: any[]) {
        logs.push(args)
        return originalLog.apply(this, args)
      }

      const worker = new SceneWorker(parcelScene, transport.server)
      loadedParcelSceneWorkers.add(worker)
      // keep this to avoid regressions
      await worker.system

      while (!sceneHost.didStart) {
        await sleep(10)
      }
    })

    try {
      cb({
        ensureNoErrors: () => {
          expect(sceneHost.devToolsAdapter.exceptions.length).to.eq(
            0,
            `Found some(${sceneHost.devToolsAdapter.exceptions.length}) errors`
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
      loadedParcelSceneWorkers.delete(worker)
    })
  })
}
