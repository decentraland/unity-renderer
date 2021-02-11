import { expect } from 'chai'
import { Quaternion, Vector3 } from 'decentraland-ecs/src'

const results = {
  rotatedWithQuat01: '(0.7, 0.0, 1.6)',
  rotatedWithQuat02: '(-0.1, -0.9, 0.6)',
  rotatedWithQuat03: '(127.7, 46.0, -30.2)',
  rotatedWithQuat04: '(-115.4, -138.8, 327.7)'
}

const normalize = (v: string) => (v === '-0.0' ? '0.0' : v)

function vector3ToString(vec: Vector3) {
  const x = normalize(vec.x.toFixed(1).substr(0, 6))
  const y = normalize(vec.y.toFixed(1).substr(0, 6))
  const z = normalize(vec.z.toFixed(1).substr(0, 6))

  return `(${x}, ${y}, ${z})`
}

describe('ECS Vector3 tests', () => {
  it('vector3.rotate', () => {
    expect(vector3ToString(new Vector3(1, 1, 1).rotate(Quaternion.Euler(45, 60, 90)))).to.eq(
      results.rotatedWithQuat01,
      'rotatedWithQuat01'
    )

    expect(vector3ToString(new Vector3(1, 0, -0.5).rotate(Quaternion.Euler(-165, 55, 125)))).to.eq(
      results.rotatedWithQuat02,
      'rotatedWithQuat02'
    )

    expect(vector3ToString(new Vector3(100, -90, 35).rotate(Quaternion.Euler(45, 60, 90)))).to.eq(
      results.rotatedWithQuat03,
      'rotatedWithQuat03'
    )

    expect(vector3ToString(new Vector3(100, 200, 300).rotate(Quaternion.Euler(47.572, 13.179, 83.369)))).to.eq(
      results.rotatedWithQuat04,
      'rotatedWithQuat04'
    )
  })
})
