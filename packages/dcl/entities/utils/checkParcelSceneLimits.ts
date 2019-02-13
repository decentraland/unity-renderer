import * as BABYLON from 'babylonjs'
import { isOnLimits } from 'atomicHelpers/parcelScenePositions'
import { IParcelSceneLimits } from 'atomicHelpers/landHelpers'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { parcelLimits } from 'config'
import { BasicShape } from 'engine/components/disposableComponents/DisposableComponent'

const ignoreBoundaryCheck = Symbol('ignoreBoundaryCheck')

/// --- EXPORTS ---

export function ignoreBoundaryChecksOnObject(obj: BABYLON.Node, ignore = true) {
  obj[ignoreBoundaryCheck] = ignore
}

export function areBoundariesIgnored(obj: BABYLON.Node) {
  return !!obj[ignoreBoundaryCheck]
}

export function measureObject3D(obj: BABYLON.AbstractMesh | BABYLON.Mesh | BABYLON.TransformNode): IParcelSceneLimits {
  let entities = 0
  let triangles = 0
  let bodies = 0
  let textures = 0
  let materials = 0
  let geometries = 0

  if (obj && !areBoundariesIgnored(obj)) {
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
export function checkParcelSceneBoundaries(
  encodedParcels: Set<string>,
  objectsOutside: Set<BaseEntity>,
  entity: BaseEntity
) {
  const maxHeight = Math.log2(encodedParcels.size + 1) * parcelLimits.height
  const minHeight = -maxHeight

  entity.traverseControl(entity => {
    if (entity[ignoreBoundaryCheck]) {
      return 'BREAK'
    }

    const mesh = entity.getObject3D(BasicShape.nameInEntity)

    if (!mesh) {
      return 'CONTINUE'
    }

    if (mesh[ignoreBoundaryCheck]) {
      return 'BREAK'
    }

    const bbox = entity.getMeshesBoundingBox()

    if (bbox.maximum.y > maxHeight || bbox.minimum.y < minHeight) {
      objectsOutside.add(entity)
      return 'BREAK'
    }

    if (!isOnLimits(bbox, encodedParcels)) {
      objectsOutside.add(entity)
      return 'BREAK'
    }

    return 'CONTINUE'
  })

  return objectsOutside
}
