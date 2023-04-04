import { expect } from 'chai'
import { pickWorldSpawnpoint } from 'shared/world/positionThings'
import { isInsideWorldLimits, Scene } from '@dcl/schemas'
import { Vector3 } from '@dcl/ecs-math'
import { parcelSize } from 'lib/decentraland'

describe('pickWorldSpawnPoint unit tests', function () {
  it('picks a spawn point from the defined ones when no default', () => {
    const land: Scene = {
      main: '',
      scene: {
        base: '10,10',
        parcels: ['10,10']
      },
      spawnPoints: [
        {
          position: { x: 1, y: 2, z: 3 },
          cameraTarget: { x: 9, y: 8, z: 7 }
        }
      ]
    }
    const basePosition = new Vector3(10 * parcelSize, 0, 10 * parcelSize)

    const pick = pickWorldSpawnpoint(land, basePosition)

    expect(JSON.stringify(pick)).to.deep.equal(
      JSON.stringify({
        position: { x: 1 + basePosition.x, y: 2 + basePosition.y, z: 3 + basePosition.z },
        cameraTarget: { x: 9 + basePosition.x, y: 8 + basePosition.y, z: 7 + basePosition.z }
      })
    )
  })

  it('picks the nearest spawn point from the defined ones when no default', () => {
    const land: Scene = {
      main: '',
      scene: {
        base: '10,10',
        parcels: ['10,10', '11,10']
      },
      spawnPoints: [
        {
          position: { x: 1, y: 1, z: 1 },
          cameraTarget: { x: 1, y: 1, z: 1 }
        },
        {
          position: { x: 16, y: 1, z: 1 },
          cameraTarget: { x: 16, y: 1, z: 1 }
        }
      ]
    }
    const basePosition = new Vector3(10 * parcelSize, 0, 10 * parcelSize)
    const targetPosition = new Vector3(11 * parcelSize, 0, 10 * parcelSize)

    const pick = pickWorldSpawnpoint(land, targetPosition)

    expect(JSON.stringify(pick)).to.deep.equal(
      JSON.stringify({
        position: { x: 16 + basePosition.x, y: 1 + basePosition.y, z: 1 + basePosition.z },
        cameraTarget: { x: 16 + basePosition.x, y: 1 + basePosition.y, z: 1 + basePosition.z }
      })
    )
  })

  it('picks a spawn point from the default ones when existing', () => {
    const land: Scene = {
      main: '',
      scene: {
        base: '10,10',
        parcels: ['10,10']
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
    const basePosition = new Vector3(10 * parcelSize, 0, 10 * parcelSize)

    const pick = pickWorldSpawnpoint(land, basePosition)

    expect(JSON.stringify(pick)).to.deep.equal(
      JSON.stringify({
        position: { x: 1 + basePosition.x, y: 2 + basePosition.y, z: 3 + basePosition.z },
        cameraTarget: { x: 9 + basePosition.x, y: 8 + basePosition.y, z: 7 + basePosition.z }
      })
    )
  })

  it('picks the nearest spawn point from the default ones when existing', () => {
    const land: Scene = {
      main: '',
      scene: {
        base: '10,10',
        parcels: ['10,10', '11,10']
      },
      spawnPoints: [
        {
          position: { x: 1, y: 1, z: 1 },
          cameraTarget: { x: 1, y: 1, z: 1 },
          default: true
        },
        {
          position: { x: 12, y: 2, z: 2 },
          cameraTarget: { x: 12, y: 2, z: 2 },
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
    const basePosition = new Vector3(10 * parcelSize, 0, 10 * parcelSize)
    const targetPosition = new Vector3(11 * parcelSize, 0, 10 * parcelSize)

    const pick = pickWorldSpawnpoint(land, targetPosition)

    expect(JSON.stringify(pick)).to.deep.equal(
      JSON.stringify({
        position: { x: 12 + basePosition.x, y: 2 + basePosition.y, z: 2 + basePosition.z },
        cameraTarget: { x: 12 + basePosition.x, y: 2 + basePosition.y, z: 2 + basePosition.z }
      })
    )
  })

  it('spawn point with components in range', () => {
    const land: Scene = {
      main: '',
      scene: {
        base: '10,10',
        parcels: ['10,10']
      },
      spawnPoints: [
        {
          position: { x: [1, 2], y: [2, 3], z: [3, 4] }
        }
      ]
    }
    const basePosition = new Vector3(10 * parcelSize, 0, 10 * parcelSize)

    const pick = pickWorldSpawnpoint(land, basePosition)

    expect(pick.position.x).to.be.within(1 + basePosition.x, 2 + basePosition.x)
    expect(pick.position.y).to.be.within(2 + basePosition.y, 3 + basePosition.y)
    expect(pick.position.z).to.be.within(3 + basePosition.z, 4 + basePosition.z)
  })

  it('sets spawn point to base parcel position when none defined', () => {
    const land: Scene = {
      main: '',
      scene: {
        base: '10,10',
        parcels: ['10,10']
      }
    }
    const basePosition = new Vector3(10 * parcelSize, 0, 10 * parcelSize)
    const pick = pickWorldSpawnpoint(land, basePosition)

    expect(pick.position.x).to.be.at.least(160)
    expect(pick.position.x).to.be.below(176)
    expect(pick.position.y).to.equal(0)
    expect(pick.position.z).to.be.at.least(160)
    expect(pick.position.z).to.be.below(176)
    expect(pick.cameraTarget).to.equal(undefined)
  })
})

describe('isInsideWorldLimits unit tests', function () {
  it('valid positions outside main region are recognized properly', () => {
    expect(isInsideWorldLimits(63, 155)).to.be.true
    expect(isInsideWorldLimits(160, 147)).to.be.true
    expect(isInsideWorldLimits(163, 70)).to.be.true
  })

  it('invalid positions outside main region are recognized properly', () => {
    expect(isInsideWorldLimits(50, 155)).to.be.false
    expect(isInsideWorldLimits(160, 10)).to.be.false
    expect(isInsideWorldLimits(163, 40)).to.be.false
  })
})
