export function generateNormalMesh(
  mesh: BABYLON.AbstractMesh,
  size: number,
  color: BABYLON.Color3,
  scene: BABYLON.Scene
) {
  const normals = mesh.getVerticesData(BABYLON.VertexBuffer.NormalKind)
  const positions = mesh.getVerticesData(BABYLON.VertexBuffer.PositionKind)
  const lines = []

  for (let i = 0; i < normals.length; i += 3) {
    const v1 = BABYLON.Vector3.FromArray(positions, i)
    const v2 = v1.add(BABYLON.Vector3.FromArray(normals, i).scaleInPlace(size))
    lines.push([v1.add(mesh.position), v2.add(mesh.position)])
  }

  const normalLines = BABYLON.MeshBuilder.CreateLineSystem('normalLines', { lines: lines, updatable: false }, scene)

  normalLines.color = color
  return normalLines
}
