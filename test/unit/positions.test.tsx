import * as BABYLON from 'babylonjs'
import { expect } from 'chai'
import { gridToWorld, worldToGrid } from 'atomicHelpers/parcelScenePositions'

function verifyW2G(worldX: number, worldZ: number, gridX: number, gridY: number) {
  it(`world(${worldX},${worldZ}) -> grid(${gridX},${gridY})`, () => {
    const wPosition = BABYLON.Vector2.Zero()
    worldToGrid(new BABYLON.Vector3(worldX, 0, worldZ), wPosition)
    expect(wPosition.x).to.eq(gridX, 'x')
    expect(wPosition.y).to.eq(gridY, 'y')
  })
}

describe('position unit tests', function() {
  it('converts grid to the right coordinate system', () => {
    const wPosition = BABYLON.Vector3.Zero()

    gridToWorld(0, 0, wPosition)
    expect(wPosition.x).to.eq(0)
    expect(wPosition.y).to.eq(0)
    expect(wPosition.z).to.eq(0)

    gridToWorld(-1, 1, wPosition)
    expect(wPosition.x).to.eq(-10)
    expect(wPosition.y).to.eq(0)
    expect(wPosition.z).to.eq(10)

    gridToWorld(1, -1, wPosition)
    expect(wPosition.x).to.eq(10)
    expect(wPosition.y).to.eq(0)
    expect(wPosition.z).to.eq(-10)
  })

  // (0,0)
  verifyW2G(10, 0, 1, 0)
  verifyW2G(5, 5, 0, 0)
  verifyW2G(9, 9, 0, 0)

  verifyW2G(6, 36, 0, 3)
  verifyW2G(6, 36, 0, 3)

  verifyW2G(6, 46, 0, 4)
  verifyW2G(6, 46, 0, 4)

  verifyW2G(0, 0, 0, 0)
  verifyW2G(-5, 5, -1, 0)
  verifyW2G(-9.9, 9.9, -1, 0)

  verifyW2G(5, -5, 0, -1)

  verifyW2G(-15, 15, -2, 1)
  verifyW2G(-10, 10, -1, 1)
})
