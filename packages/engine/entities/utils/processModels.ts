import * as BABYLON from 'babylonjs'
import { markAsCollider } from './colliders'

function disposeDelegate($: { dispose: Function }) {
  $.dispose()
}
function disposeNodeDelegate($: BABYLON.TransformNode) {
  $.setEnabled(false)
  $.parent = null
  $.dispose(false)
}

export function cleanupAssetContainer($: BABYLON.AssetContainer) {
  if ($) {
    $.removeAllFromScene()
    $.transformNodes && $.transformNodes.forEach(disposeNodeDelegate)
    $.rootNodes && $.rootNodes.forEach(disposeNodeDelegate)
    $.meshes && $.meshes.forEach(disposeNodeDelegate)
    $.textures && $.textures.forEach(disposeDelegate)
    $.multiMaterials && $.multiMaterials.forEach(disposeDelegate)
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
