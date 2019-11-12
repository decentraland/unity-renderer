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
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(true)
      })
      it('(20.1) -> false', () => {
        const bbox = { maximum: { x: 1, y: 20.1, z: 1 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(false)
      })
      it('(-20) -> true', () => {
        const bbox = { maximum: { x: 1, y: 0, z: 1 }, minimum: { x: 0, y: -20, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(true)
      })
      it('(-20.1) -> false', () => {
        const bbox = { maximum: { x: 1, y: 0, z: 1 }, minimum: { x: 0, y: -20.1, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(false)
      })
    })
    describe('horizontal limits', () => {
      it('(16, 16) -> true', () => {
        const bbox = { maximum: { x: 16, y: 0, z: 16 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(true)
      })
      it('(0, 0) -> true', () => {
        const bbox = { maximum: { x: 0, y: 0, z: 0 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(true)
      })
      it('a(16, 16) -> true', () => {
        const bbox = { maximum: { x: 16, y: 0, z: 16 }, minimum: { x: 16, y: 0, z: 16 } }
        const result = isOnLimits(bbox, [{ x: 16, y: 0, z: 16 }])
        expect(result).to.eq(true)
      })
      it('(16.1, 16.1) -> false', () => {
        const bbox = { maximum: { x: 16.1, y: 0, z: 16.1 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(false)
      })
      it('(16.001, 16.001) -> true', () => {
        const bbox = { maximum: { x: 16.001, y: 0, z: 16.001 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(true)
      })
      it('(-0.001, -0.001) -> true', () => {
        const bbox = { maximum: { x: -0.001, y: 0, z: -0.001 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(true)
      })
      it('(-0.01, -0.01) -> false', () => {
        const bbox = { maximum: { x: -0.01, y: 0, z: -0.01 }, minimum: { x: 0, y: 0, z: 0 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
        expect(result).to.eq(false)
      })
      it('(-0.1, -0.1) -> false', () => {
        const bbox = { maximum: { x: 1, y: 0, z: 1 }, minimum: { x: -0.1, y: 0, z: -0.1 } }
        const result = isOnLimits(bbox, [{ x: 0, y: 0, z: 0 }])
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
