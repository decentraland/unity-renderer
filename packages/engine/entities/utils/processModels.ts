import * as BABYLON from 'babylonjs'
import { markAsCollider } from './colliders'

function disposeDelegate($: { dispose: Function }) {
  $.dispose()
}

export function cleanupAssetContainer($: BABYLON.AssetContainer) {
  if ($) {
    $.removeAllFromScene()
    $.transformNodes && $.transformNodes.forEach(disposeDelegate)
    $.meshes && $.meshes.forEach(disposeDelegate)
    $.textures && $.textures.forEach(disposeDelegate)
    $.sounds && $.sounds.forEach(disposeDelegate)
    $.skeletons && $.skeletons.forEach(disposeDelegate)
    $.materials && $.materials.forEach(disposeDelegate)
    $.lights && $.lights.forEach(disposeDelegate)
  }
}

export function processColliders($: BABYLON.AssetContainer, actionManager: BABYLON.ActionManager) {
  for (let i = 0; i < $.meshes.length; i++) {
    let mesh = $.meshes[i]

    if (mesh.name.toLowerCase().endsWith('collider')) {
      mesh.checkCollisions = true
      mesh.visibility = 0
      mesh.isPickable = false
      markAsCollider(mesh)
    } else {
      mesh.isPickable = true
      mesh.actionManager = actionManager
    }
  }
}
