import { parcelLimits } from 'config'
import { uuid } from 'atomicHelpers/math'
import { decodeParcelSceneBoundaries, gridToParcel } from 'atomicHelpers/parcelScenePositions'
import { Color3 } from 'decentraland-ecs/src'
import { BaseEntity } from '../BaseEntity'
import { SharedSceneContext } from '../SharedSceneContext'
import { BoxShape } from '../../components/disposableComponents/BoxShape'
import { PBRMaterial } from '../../components/disposableComponents/PBRMaterial'
import { setEntityText } from '../../components/ephemeralComponents/TextShape'
import { scene } from '../../renderer'
import { checkerboardMaterial } from '../../renderer/ambientLights'
import { ignoreBoundaryChecksOnObject } from './checkParcelSceneLimits'

const debugContext = new SharedSceneContext('.', 'debug-shared-context')

const box = new BoxShape(debugContext, uuid())

const xMaterial = new PBRMaterial(debugContext, uuid())
xMaterial.updateData({ albedoColor: '#FF0000' }).catch($ => debugContext.logger.error('PBRMaterial#updateData', $))

const yMaterial = new PBRMaterial(debugContext, uuid())
yMaterial.updateData({ albedoColor: '#00FF00' }).catch($ => debugContext.logger.error('PBRMaterial#updateData', $))

const zMaterial = new PBRMaterial(debugContext, uuid())
zMaterial.updateData({ albedoColor: '#0000FF' }).catch($ => debugContext.logger.error('PBRMaterial#updateData', $))

export function createAxisEntity() {
  const ret = new BaseEntity(uuid(), debugContext)
  ignoreBoundaryChecksOnObject(ret)

  const xAxis = new BaseEntity(uuid(), debugContext)
  xAxis.setParentEntity(ret)
  xAxis.scaling.set(1, 0.01, 0.01)
  xAxis.position.set(0.5, 0.0, 0.0)
  xAxis.onDisposeObservable.add(() => debugContext.entities.delete(xAxis.id))
  box.attachTo(xAxis)
  xMaterial.attachTo(xAxis)
  setEntityText(xAxis, { value: 'X', color: new Color3(1, 0, 0), billboard: true })
  ignoreBoundaryChecksOnObject(xAxis)

  const yAxis = new BaseEntity(uuid(), debugContext)
  yAxis.setParentEntity(ret)
  yAxis.scaling.set(0.01, 1, 0.01)
  yAxis.position.set(0.0, 0.5, 0.0)
  yAxis.onDisposeObservable.add(() => debugContext.entities.delete(yAxis.id))
  box.attachTo(yAxis)
  yMaterial.attachTo(yAxis)
  setEntityText(yAxis, { value: 'Y', color: new Color3(0, 1, 0), billboard: true })
  ignoreBoundaryChecksOnObject(yAxis)

  const zAxis = new BaseEntity(uuid(), debugContext)
  zAxis.setParentEntity(ret)
  zAxis.scaling.set(0.01, 0.01, 1)
  zAxis.position.set(0.0, 0.0, 0.5)
  zAxis.onDisposeObservable.add(() => debugContext.entities.delete(zAxis.id))
  box.attachTo(zAxis)
  zMaterial.attachTo(zAxis)
  setEntityText(zAxis, { value: 'Z', color: new Color3(0, 0, 1), billboard: true })
  ignoreBoundaryChecksOnObject(zAxis)

  return ret
}

export function createParcelOutline(positions: string) {
  const decoded = decodeParcelSceneBoundaries(positions)
  const parcels = decoded.parcels.map($ => new BABYLON.Vector2($.x, $.y))

  const contains = (v: BABYLON.Vector2): boolean => {
    return !!parcels.find(p => p.equals(v))
  }

  const points: BABYLON.Vector3[][] = []

  const minX = Math.min(...parcels.map($ => $.x)) - 1
  const minY = Math.min(...parcels.map($ => $.y)) - 1
  const maxX = Math.max(...parcels.map($ => $.x)) + 1
  const maxY = Math.max(...parcels.map($ => $.y)) + 1
  const groundMeshes: BABYLON.Mesh[] = []
  /*
   * Iterate over all the parcels in the bounding box surrounding this
   * parcel, and draw a border whenever we change state from inside
   * the parcel to outside the parcel.
   */
  for (let x = minX; x < maxX + 1; x++) {
    for (let y = minY; y < maxY + 1; y++) {
      const p = contains(new BABYLON.Vector2(x, y))
      const northern = contains(new BABYLON.Vector2(x, y - 1))
      const western = contains(new BABYLON.Vector2(x - 1, y))
      if (p) {
        const floor = BABYLON.MeshBuilder.CreateGround(
          `ground-${x},${y}`,
          { width: parcelLimits.parcelSize, height: parcelLimits.parcelSize },
          scene
        )
        floor.material = checkerboardMaterial

        gridToParcel(decoded.base, x + 0.5, y + 0.5, floor.position)

        groundMeshes.push(floor)
      }
      if (p !== western) {
        const p1 = new BABYLON.Vector3(0, 0, 0)
        gridToParcel(decoded.base, x, y, p1)
        p1.z = p1.z
        const p2 = p1.clone()
        p2.z = p2.z + parcelLimits.parcelSize
        points.push([p1, p2])
      }
      if (p !== northern) {
        const p1 = new BABYLON.Vector3(0, 0, 0)
        gridToParcel(decoded.base, x, y, p1)
        p1.x = p1.x
        const p2 = p1.clone()
        p2.x = p2.x + parcelLimits.parcelSize
        points.push([p1, p2])
      }
    }
  }

  const ground = BABYLON.Mesh.MergeMeshes(groundMeshes, true) as BABYLON.Mesh
  ground.isPickable = false

  const lines = BABYLON.MeshBuilder.CreateLineSystem('lines', { lines: points }, scene)
  lines.color = BABYLON.Color3.FromHexString('#ff004f')
  lines.isPickable = false

  return { ground, result: lines }
}
