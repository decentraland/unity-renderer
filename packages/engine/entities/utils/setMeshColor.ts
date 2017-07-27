import { Mesh, Color3 } from 'babylonjs'

export function setMeshColor(mesh: Mesh, color: Color3) {
  let colors = mesh.getVerticesData(BABYLON.VertexBuffer.ColorKind)

  if (!colors) {
    colors = []

    let positions = mesh.getVerticesData(BABYLON.VertexBuffer.PositionKind)

    for (let p = 0; p < positions.length / 3; p++) {
      colors.push(color.r, color.g, color.b, 1)
    }
  } else {
    for (let i = 0; i < colors.length; i++) {
      switch (i % 4) {
        case 0:
          colors[i] = color.r
          break
        case 1:
          colors[i] = color.g
          break
        case 2:
          colors[i] = color.b
          break
        case 3:
          colors[i] = 1
          break
      }
    }
  }

  mesh.setVerticesData(BABYLON.VertexBuffer.ColorKind, colors)
}
