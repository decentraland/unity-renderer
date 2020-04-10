import { expect } from 'chai'
import { pickWorldSpawnpoint } from 'shared/world/positionThings'
import { gridToWorld } from '../../packages/atomicHelpers/parcelScenePositions'

describe('pickWorldSpawnPoint unit tests', function() {
  it('picks a spawn point from the defined ones when no default', () => {
    const land: any = {
      sceneJsonData: {
        scene: {
          base: '10,10'
        },
        spawnPoints: [
          {
            position: { x: 1, y: 2, z: 3 },
            cameraTarget: { x: 9, y: 8, z: 7 }
          }
        ]
      }
    }
    const basePosition = gridToWorld(10, 10)

    const pick = pickWorldSpawnpoint(land)

    expect(pick).to.deep.equal({
      position: { x: 1 + basePosition.x, y: 2 + basePosition.y, z: 3 + basePosition.z },
      cameraTarget: { x: 9 + basePosition.x, y: 8 + basePosition.y, z: 7 + basePosition.z }
    })
  })

  it('picks a spawn point from the default ones when existing', () => {
    const land: any = {
      sceneJsonData: {
        scene: {
          base: '10,10'
        },
        spawnPoints: [
          {
            position: { x: 1, y: 2, z: 3 },
            cameraTarget: { x: 9, y: 8, z: 7 },
            default: true
          },
          {
            position: { x: 2, y: 3, z: 4 }
          },
          {
            position: { x: 3, y: 4, z: 5 }
          }
        ]
      }
    }
    const basePosition = gridToWorld(10, 10)

    const pick = pickWorldSpawnpoint(land)

    expect(pick).to.deep.equal({
      position: { x: 1 + basePosition.x, y: 2 + basePosition.y, z: 3 + basePosition.z },
      cameraTarget: { x: 9 + basePosition.x, y: 8 + basePosition.y, z: 7 + basePosition.z }
    })
  })

  it('spawn point with components in range', () => {
    const land: any = {
      sceneJsonData: {
        scene: {
          base: '10,10'
        },
        spawnPoints: [
          {
            position: { x: [1, 2], y: [2, 3], z: [3, 4] }
          }
        ]
      }
    }
    const basePosition = gridToWorld(10, 10)

    const pick = pickWorldSpawnpoint(land)

    expect(pick.position.x).to.be.within(1 + basePosition.x, 2 + basePosition.x)
    expect(pick.position.y).to.be.within(2 + basePosition.y, 3 + basePosition.y)
    expect(pick.position.z).to.be.within(3 + basePosition.z, 4 + basePosition.z)
  })

  it('sets spawn point to base parcel position when none defined', () => {
    const land: any = {
      sceneJsonData: {
        scene: {
          base: '10,10'
        }
      }
    }
    const basePosition = gridToWorld(10, 10)

    const pick = pickWorldSpawnpoint(land)

    expect(pick).to.deep.equal({
      position: basePosition,
      cameraTarget: undefined
    })
  })
})
