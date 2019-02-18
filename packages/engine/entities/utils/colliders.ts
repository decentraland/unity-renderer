import { scene } from '../../renderer'
const colliderSymbol = Symbol('isCollider')

export const colliderMaterial = new BABYLON.GridMaterial('collider-material', scene)

colliderMaterial.opacity = 0.99
colliderMaterial.sideOrientation = 0
colliderMaterial.zOffset = -1
colliderMaterial.fogEnabled = false

export function markAsCollider(mesh: BABYLON.AbstractMesh) {
  mesh[colliderSymbol] = true
  mesh.material = colliderMaterial
}

export function isCollider(mesh: BABYLON.AbstractMesh) {
  return !!mesh[colliderSymbol]
}

function isMesh($: BABYLON.Node) {
  return $ instanceof BABYLON.AbstractMesh
}

export function toggleColliderHighlight(visible: boolean, root: BABYLON.TransformNode) {
  const meshes: BABYLON.AbstractMesh[] = root.getDescendants(false, isMesh) as any

  let normalVisibility = visible ? 0.2 : 1
  let colliderVisibility = visible ? 1 : 0

  for (let mesh of meshes) {
    if (isCollider(mesh)) {
      mesh.visibility = colliderVisibility
    } else {
      mesh.visibility = normalVisibility
    }
  }
}

export function toggleBoundingBoxes(visible: boolean, root: BABYLON.TransformNode) {
  const meshes: BABYLON.AbstractMesh[] = root.getDescendants(false, isMesh) as any

  for (let mesh of meshes) {
    mesh.showBoundingBox = mesh.showSubMeshesBoundingBox = visible
  }
}
