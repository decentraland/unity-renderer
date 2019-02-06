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

function totalBoundingInfo(meshes: BABYLON.AbstractMesh[]) {
  let boundingInfo = meshes[0].getBoundingInfo()
  let min = boundingInfo.boundingBox.minimumWorld.add(meshes[0].position)
  let max = boundingInfo.boundingBox.maximumWorld.add(meshes[0].position)
  for (let i = 1; i < meshes.length; i++) {
    boundingInfo = meshes[i].getBoundingInfo()
    min = BABYLON.Vector3.Minimize(min, boundingInfo.boundingBox.minimumWorld.add(meshes[i].position))
    max = BABYLON.Vector3.Maximize(max, boundingInfo.boundingBox.maximumWorld.add(meshes[i].position))
  }
  return new BABYLON.BoundingInfo(min, max)
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

    const meshes = mesh.getChildMeshes(false)

    if (meshes.length === 0) {
      return 'CONTINUE'
    }

    const bbox = totalBoundingInfo(meshes)

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
