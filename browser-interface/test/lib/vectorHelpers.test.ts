import { expect } from 'chai'
import { isEqual } from 'lib/math/Vector3'

describe('vectorHelpers unit tests', () => {
  describe('isEqual', () => {
    it('should return true with equal vectors', () => {
      const a = { x: 10, y: 8, z: 2 }
      const b = { x: 10, y: 8, z: 2 }

      const result = isEqual(a, b)

      expect(result).to.equal(true)
    })

    it('should return false with not equal vectors', () => {
      const a = { x: 10, y: 8, z: 4 }
      const b = { x: 10, y: 9, z: 3 }

      const result = isEqual(a, b)

      expect(result).to.equal(false)
    })
  })
})
