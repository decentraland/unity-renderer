import * as BABYLON from 'babylonjs'
import { DEBUG, isRunningTest } from '../../../config'

import { BaseComponent } from '../BaseComponent'
import { validators } from '../helpers/schemaValidator'
import { BasicShape } from '../disposableComponents/DisposableComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { CLASS_ID } from 'decentraland-ecs/src'

const overlayColor = new BABYLON.Color3(1, 0, 0)

if (DEBUG && !isRunningTest) {
  // we only set this interval in debug mode because this boxes are only visible in that mode
  setInterval(() => {
    overlayColor.set(1 - overlayColor.r, overlayColor.g, 1 - overlayColor.b)
  }, 160)
}

function filterMeshes($: BABYLON.TransformNode) {
  return $ instanceof BABYLON.AbstractMesh
}

export class HighlightBox extends BaseComponent<boolean> {
  transformValue(x) {
    return validators.boolean(x, false)
  }

  shouldSceneUpdate() {
    return true
  }

  update(oldValue, newValue: boolean) {
    const mesh: BABYLON.Mesh = this.entity.getObject3D(BasicShape.nameInEntity) as any
    if (mesh) {
      const didChange = mesh.renderOverlay !== !!newValue
      mesh.renderOverlay = !!newValue
      mesh.showBoundingBox = !!newValue
      mesh.overlayColor = overlayColor

      if (didChange) {
        const children = mesh.getChildTransformNodes(false, filterMeshes) as BABYLON.AbstractMesh[]

        for (let child of children) {
          child.renderOverlay = !!newValue
          child.showBoundingBox = !!newValue
          child.overlayColor = overlayColor
        }
      }
    }
  }

  detach() {
    const mesh: BABYLON.Mesh = this.entity.getObject3D(BasicShape.nameInEntity) as any
    if (mesh) {
      mesh.renderOverlay = false
      mesh.renderOverlay = false
      mesh.overlayColor = new BABYLON.Color3()

      const children = mesh.getChildTransformNodes(false, filterMeshes) as BABYLON.AbstractMesh[]

      for (let child of children) {
        child.renderOverlay = false
        child.showBoundingBox = false
        child.overlayColor = new BABYLON.Color3()
      }
    }
  }
}

export function highlightEntity(entity: BaseEntity) {
  entity.context.UpdateEntityComponent({
    classId: CLASS_ID.HIGHLIGHT_ENTITY,
    entityId: entity.id,
    json: '{}',
    name: 'highlight'
  })
}

export function removeEntityHighlight(entity: BaseEntity) {
  entity.context.ComponentRemoved({
    entityId: entity.id,
    name: 'highlight'
  })
}
