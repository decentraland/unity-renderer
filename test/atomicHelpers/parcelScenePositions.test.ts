import { expect } from 'chai'
import { isValidParcelSceneShape, isOnLimits, areConnected } from 'atomicHelpers/parcelScenePositions'
import { Vector2Component } from 'atomicHelpers/landHelpers'

describe('parcelScenePositions unit tests', () => {
  describe('isValidParcelSceneShape', () => {
    it('should return true with parcels that have a valid parcelScene shape', () => {
      const parcels: Vector2Component[] = [
        { x: -130, y: -35 },
        { x: -129, y: -35 },
        { x: -130, y: -34 },
        { x: -129, y: -34 },
        { x: -128, y: -34 }
      ]

      const result = isValidParcelSceneShape(parcels)

      expect(result).to.equal(true)
    })

    it('should return false with parcels that have an invalid parcelScene shape', () => {
      const parcels: Vector2Component[] = [
        { x: -130, y: -34 },
        { x: -130, y: -35 },
        { x: -129, y: 35 },
        { x: -128, y: -34 }
      ]

      const result = isValidParcelSceneShape(parcels)

      expect(result).to.equal(false)
    })
  })

  describe('isOnLimits (validates that limits are inclusive and no outside of grid)', () => {
    describe.skip('vertical limits', () => {
      it('(20) -> true', () => {
        const bbox = { maximum: { x: 1, y: 20, z: 1 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(true)
      })
      it('(20.1) -> false', () => {
        const bbox = { maximum: { x: 1, y: 20.1, z: 1 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(false)
      })
      it('(-20) -> true', () => {
        const bbox = { maximum: { x: 1, y: 0, z: 1 }, minimum: { x: 0, y: -20, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(true)
      })
      it('(-20.1) -> false', () => {
        const bbox = { maximum: { x: 1, y: 0, z: 1 }, minimum: { x: 0, y: -20.1, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(false)
      })
    })
    describe('horizontal limits', () => {
      it('(10, 10) -> true', () => {
        const bbox = { maximum: { x: 10, y: 0, z: 10 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(true)
      })
      it('(0, 0) -> true', () => {
        const bbox = { maximum: { x: 0, y: 0, z: 0 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(true)
      })
      it('a(10, 10) -> true', () => {
        const bbox = { maximum: { x: 10, y: 0, z: 10 }, minimum: { x: 10, y: 0, z: 10 } }
        const result = isOnLimits(bbox, '1,1')
        expect(result).to.eq(true)
      })
      it('(10.1, 10.1) -> false', () => {
        const bbox = { maximum: { x: 10.1, y: 0, z: 10.1 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(false)
      })
      it('(-0.1, -0.1) -> false', () => {
        const bbox = { maximum: { x: 1, y: 0, z: 1 }, minimum: { x: -0.1, y: 0, z: -0.1 } }
        const result = isOnLimits(bbox, '0,0')
        expect(result).to.eq(false)
      })
    })
  })
  describe('areConnected', () => {
    it('should return true for connected parcels', () => {
      const result = areConnected([{ x: 1, y: 2 }, { x: 1, y: 3 }])
      expect(result).to.deep.equal(true)
    })
    it('should return false for not connected parcels', () => {
      const result = areConnected([{ x: 1, y: 2 }, { x: 1, y: 5 }])
      expect(result).to.deep.equal(false)
    })
    it('should return true for one parcel.', () => {
      const result = areConnected([{ x: 1, y: 2 }])
      expect(result).to.deep.equal(true)
    })
    it('should return false for connected parcels but not all of them', () => {
      const result = areConnected([{ x: 1, y: 2 }, { x: 1, y: 3 }, { x: 1, y: 5 }, { x: 1, y: 6 }])
      expect(result).to.deep.equal(false)
    })
  })
})
