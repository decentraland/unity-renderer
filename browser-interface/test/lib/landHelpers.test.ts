import { expect } from 'chai'
import { isAdjacent } from 'lib/decentraland/parcels/isAdjacent'
import { isEqual } from 'lib/math/Vector2'

describe('landHelpers unit tests', () => {
  describe('isEqual', () => {
    it('should return true with equal parcels', () => {
      const p1 = { x: 10, y: 8 }
      const p2 = { x: 10, y: 8 }

      const result = isEqual(p1, p2)

      expect(result).to.equal(true)
    })

    it('should return false with not equal parcels', () => {
      const p1 = { x: 10, y: 8 }
      const p2 = { x: 10, y: 9 }

      const result = isEqual(p1, p2)

      expect(result).to.equal(false)
    })
  })
  describe('isAdjacent', () => {
    it('should return true parcels that are one next to each other', () => {
      const p1 = { x: 10, y: 8 }
      const p2 = { x: 10, y: 9 }

      const result = isAdjacent(p1, p2)

      expect(result).to.equal(true)
    })

    it("should return false with parcels that aren't one next to each other", () => {
      const p1 = { x: 10, y: 8 }
      const p2 = { x: 11, y: 9 }

      const result = isAdjacent(p1, p2)

      expect(result).to.equal(false)
    })
  })
})
