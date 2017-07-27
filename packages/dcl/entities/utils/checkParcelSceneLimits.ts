import * as BABYLON from 'babylonjs'
import { isOnLimits } from 'atomicHelpers/parcelScenePositions'
import { IParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { instrumentTelemetry } from 'atomicHelpers/DebugTelemetry'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { parcelLimits } from 'config'
import { BasicShape } from 'engine/components/disposableComponents/DisposableComponent'

const ignoreBoundaryCheck = Symbol('ignoreBoundaryCheck')

/// --- EXPORTS ---

export function ignoreBoundaryChecksOnObject(obj: BABYLON.Node, ignore = true) {
  obj[ignoreBoundaryCheck] = ignore
}

export function measureObject3D(obj: BABYLON.AbstractMesh | BABYLON.Mesh | BABYLON.TransformNode): IParcelSceneLimits {
  let entities = 0
  let triangles = 0
  let bodies = 0
  let textures = 0
  let materials = 0
  let geometries = 0

  if (obj && !obj[ignoreBoundaryCheck]) {
    if ('geometry' in obj && obj.geometry) {
      bodies++
      geometries++
      const indexes = obj.geometry.getTotalIndices()
      if (indexes) {
        triangles += Math.floor(indexes / 3)
      } else {
        const vertices = obj.geometry.getTotalVertices()
        triangles += Math.floor(vertices / 3)
      }
    } else if ('subMeshes' in obj && obj.subMeshes) {
      for (let i = 0; i < obj.subMeshes.length; i++) {
        const m = obj.subMeshes[i]
        bodies++
        geometries++
        if (m.indexCount) {
          triangles += Math.floor(m.indexCount / 3)
        } else {
          triangles += Math.floor(m.verticesCount / 3)
        }
      }
    }
  }

  return { entities, triangles, bodies, textures, materials, geometries }
}

/**
 * Returns the objects that are outside the parcelScene limits.
 * Receives the encoded parcelScene parcels and the entity to traverse
 */
export const checkParcelSceneBoundaries = instrumentTelemetry(
  'checkParcelSceneBoundaries',
  (encodedParcels: string, objectsOutside: Set<BaseEntity>, entity: BaseEntity) => {
    const numberOfParcels = encodedParcels.split(';').length
    const verificationText = ';' + encodedParcels + ';'

    const maxHeight = Math.log2(numberOfParcels + 1) * parcelLimits.height
    const minHeight = -maxHeight

    entity.traverseControl(node => {
      if (node[ignoreBoundaryCheck]) {
        return 'BREAK'
      }

      const mesh = node.getObject3D(BasicShape.nameInEntity)

      if (!mesh) {
        return 'CONTINUE'
      }

      if (mesh[ignoreBoundaryCheck]) {
        return 'BREAK'
      }

      mesh.computeWorldMatrix(true)
      const nodeMesh: BABYLON.AbstractMesh = mesh as any

      if (!('getBoundingInfo' in nodeMesh)) {
        return 'CONTINUE'
      }

      const bbox = nodeMesh.getBoundingInfo()

      if (bbox.boundingBox.maximumWorld.y > maxHeight || bbox.boundingBox.minimumWorld.y < minHeight) {
        objectsOutside.add(node)
        return 'BREAK'
      }

      if (!isOnLimits(bbox.boundingBox, verificationText)) {
        objectsOutside.add(node)
        return 'BREAK'
      }

      return 'CONTINUE'
    })

    return objectsOutside
  }
)
