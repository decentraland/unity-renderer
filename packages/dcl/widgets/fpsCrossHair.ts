import * as BABYLON from 'babylonjs'

export function createCrossHair(scene: BABYLON.Scene) {
  const e = 128

  const texture = new BABYLON.DynamicTexture('reticule', e, scene, !1)
  texture.hasAlpha = true

  const i = texture.getContext()
  i.fillStyle = 'transparent'
  i.clearRect(0, 0, e, e)

  i.strokeStyle = 'rgba(64, 64, 64, 0.9)'
  i.fillStyle = 'rgba(255, 255, 255, 0.3)'
  i.lineWidth = 2

  i.beginPath()
  i.arc(64, 64, 8, 0, Math.PI * 2, true)
  i.fill()
  i.stroke()

  texture.update()

  const s = new BABYLON.StandardMaterial('reticule', scene)
  s.diffuseTexture = texture
  s.opacityTexture = texture
  s.emissiveTexture = texture
  s.emissiveColor.set(1, 1, 1)
  s.disableLighting = true

  const reticule = BABYLON.MeshBuilder.CreatePlane('reticule', { size: 0.02 }, scene)
  reticule.material = s
  reticule.position.set(0, 0, 0.2)
  reticule.isPickable = false

  return reticule
}
