import * as BABYLON from 'babylonjs'

import { BaseComponent } from '../BaseComponent'
import { BasicShape } from '../disposableComponents/DisposableComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { getHighlightLayer } from 'engine/renderer/init'

const OUTLINE_NAME = 'outline'
const outlineColor = BABYLON.Color3.FromHexString('#1D82FF')

export class Outline extends BaseComponent<{}> {
  readonly name = OUTLINE_NAME

  transformValue(x) {
    return {
      nonce: Math.random()
    }
  }

  shouldSceneUpdate() {
    return true
  }

  didUpdateMesh = () => {
    this.update()
  }

  attach() {
    this.entity.onChangeObject3DObservable.add(this.didUpdateMesh)
    this.update()
  }

  update() {
    const mesh: BABYLON.AbstractMesh = this.entity.getObject3D(BasicShape.nameInEntity) as any

    if (mesh) {
      if (mesh instanceof BABYLON.Mesh) {
        getHighlightLayer().addMesh(mesh, outlineColor)
      }

      const children = mesh.getChildMeshes(false) as BABYLON.Mesh[]

      for (let child of children) {
        getHighlightLayer().addMesh(child, outlineColor)
      }
    }
  }

  detach() {
    this.entity.onChangeObject3DObservable.removeCallback(this.didUpdateMesh)

    const mesh: BABYLON.AbstractMesh = this.entity.getObject3D(BasicShape.nameInEntity) as any
    if (mesh) {
      if (mesh instanceof BABYLON.Mesh) {
        getHighlightLayer().removeMesh(mesh)
      }

      const children = mesh.getChildMeshes(false) as BABYLON.Mesh[]

      for (let child of children) {
        getHighlightLayer().removeMesh(child)
      }
    }
  }
}

export function removeEntityOutline(entity: BaseEntity) {
  const b = entity.getBehaviorByName(OUTLINE_NAME)
  if (b) {
    entity.removeBehavior(b)
  }
}

export function addEntityOutline(entity: BaseEntity) {
  const b = entity.getBehaviorByName(OUTLINE_NAME)
  if (!b) {
    entity.addBehavior(new Outline(entity, {}))
  }
}
