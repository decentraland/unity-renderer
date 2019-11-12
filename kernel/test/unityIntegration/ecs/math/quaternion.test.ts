import { expect } from 'chai'
import { Quaternion, Vector3 } from 'decentraland-ecs/src'

const results = {
  staticAngle01: '90',
  staticAngle02: '14.8557',
  staticAngle03: '4.99997',
  staticAngle04: '180',
  staticEuler01: '(0.0, 0.0, 0.0, 1.0)',
  staticEuler02: '(0.5, -0.5, 0.5, 0.5)',
  staticEuler03: '(0.0, 0.9, -0.4, 0.0)',
  staticEuler04: '(0.8, 0.0, 0.6, 0.0)',
  staticEuler05: '(-0.7, 0.2, -0.2, -0.6)',
  staticFromToRotation01: '(0.0, 0.0, 0.0, 1.0)',
  staticFromToRotation02: '(0.0, 0.7, 0.0, 0.7)',
  staticFromToRotation03: '(0.0, 0.0, 0.4, 0.9)',
  staticFromToRotation04: '(0.4, 0.0, 0.0, 0.9)',
  staticFromToRotation05: '(0.7, 0.0, -0.7, -0.3)',
  staticFromToRotation06: '(0.3, -0.2, 0.0, 0.9)',
  staticFromToRotation07: '(0.1, 0.4, -0.1, 0.9)',
  staticFromToRotation08: '(0.3, 0.0, -0.3, 0.9)',
  staticRotateTowards01: '(0.1, 0.1, 0.1, 1.0)',
  staticRotateTowards02: '(0.0, 0.1, 0.0, 1.0)',
  staticRotateTowards03: '(0.0, 0.1, -0.1, -1.0)',
  staticRotateTowards04: '(0.0, 0.0, 0.0, 1.0)',
  staticLookRotation01: '(-0.3, 0.4, 0.1, 0.9)',
  staticLookRotation02: '(0.0, 0.0, 0.0, 1.0)',
  staticLookRotation03: '(0.0, 0.7, 0.0, 0.7)',
  staticLookRotation04: '(0.0, 0.9, 0.4, 0.0)',
  staticSlerp01: '(0.5, 0.2, 0.2, 0.8)',
  staticSlerp02: '(0.0, 0.1, 0.8, 0.6)',
  staticSlerp03: '(-0.1, 0.0, -0.1, 1.0)',
  staticSlerp04: '(0.0, 0.3, 0.0, 0.9)',
  eulerAngles01: '(10.0, 10.0, 10.0)',
  eulerAngles02: '(0.0, 90.0, 0.0)',
  eulerAngles03: '(80.0, 190.0, 220.0)',
  eulerAngles04: '(0.0, 10.0, 0.0)',
  normalized01: '(0.1, 0.1, 0.1, 1.0)',
  normalized02: '(0.0, 0.7, 0.0, 0.7)',
  normalized03: '(-0.7, 0.2, -0.2, -0.6)',
  normalized04: '(0.0, -0.1, 0.0, -1.0)',
  setFromToRotation01: '(0.0, 0.0, 0.0, 1.0)',
  setFromToRotation02: '(0.0, 0.7, 0.0, 0.7)',
  setFromToRotation03: '(0.0, 0.0, 0.4, 0.9)',
  setFromToRotation04: '(0.4, 0.0, 0.0, 0.9)'
}

const normalize = (v: string) => (v === '-0.0' ? '0.0' : v)

function quaternionToString(quat: Quaternion) {
  const x = normalize(quat.x.toFixed(1).substr(0, 6))
  const y = normalize(quat.y.toFixed(1).substr(0, 6))
  const z = normalize(quat.z.toFixed(1).substr(0, 6))
  const w = normalize(quat.w.toFixed(1).substr(0, 6))

  return `(${x}, ${y}, ${z}, ${w})`
}

function vector3ToString(vec: Vector3) {
  const x = normalize(vec.x.toFixed(1).substr(0, 6))
  const y = normalize(vec.y.toFixed(1).substr(0, 6))
  const z = normalize(vec.z.toFixed(1).substr(0, 6))

  return `(${x}, ${y}, ${z})`
}

describe.skip('ECS Quaternion tests', () => {
  it('Quaternion.Angle', () => {
    expect(
      Quaternion.Angle(Quaternion.Euler(0, 0, 0), Quaternion.Euler(90, 90, 90))
        .toString()
        .substr(0, 5)
    ).to.eq(results.staticAngle01.substr(0, 5), 'staticAngle01')

    expect(
      Quaternion.Angle(Quaternion.Euler(10, 0, 10), Quaternion.Euler(360, 0, -1))
        .toString()
        .substr(0, 5)
    ).to.eq(results.staticAngle02.substr(0, 5), 'staticAngle02')

    expect(
      Quaternion.Angle(Quaternion.Euler(0, 5, 0), Quaternion.Euler(0, 0, 0))
        .toString()
        .substr(0, 5)
    ).to.eq(results.staticAngle03.substr(0, 5), 'staticAngle03')

    expect(
      Quaternion.Angle(Quaternion.Euler(360, -360, 0), Quaternion.Euler(180, 90, 0))
        .toString()
        .substr(0, 5)
    ).to.eq(results.staticAngle04.substr(0, 5), 'staticAngle04')
  })

  it('Quaternion.Euler', () => {
    expect(quaternionToString(Quaternion.Euler(0, 0, 0))).to.eq(results.staticEuler01, 'staticEuler01')

    expect(quaternionToString(Quaternion.Euler(90, 0, 90))).to.eq(results.staticEuler02, 'staticEuler02')

    expect(quaternionToString(Quaternion.Euler(45, 180, -1))).to.eq(results.staticEuler03, 'staticEuler03')

    expect(quaternionToString(Quaternion.Euler(360, 110, -180))).to.eq(results.staticEuler04, 'staticEuler04')

    expect(quaternionToString(Quaternion.Euler(100, 10, 400))).to.eq(results.staticEuler05, 'staticEuler05')
  })

  it('Quaternion.RotateTowards', () => {
    expect(
      quaternionToString(Quaternion.RotateTowards(Quaternion.Euler(10, 10, 10), Quaternion.Euler(100, 100, 100), 0.1))
    ).to.eq(results.staticRotateTowards01, 'staticRotateTowards01')

    expect(
      quaternionToString(Quaternion.RotateTowards(Quaternion.Euler(0, 10, -0), Quaternion.Euler(0, 9, 45), 0.1))
    ).to.eq(results.staticRotateTowards02, 'staticRotateTowards02')

    expect(
      quaternionToString(Quaternion.RotateTowards(Quaternion.Euler(360, -10, 10), Quaternion.Euler(0, 100, 0), 0.1))
    ).to.eq(results.staticRotateTowards03, 'staticRotateTowards03')

    expect(
      quaternionToString(Quaternion.RotateTowards(Quaternion.Euler(0, 0, 0), Quaternion.Euler(0, 0, 0), 0.1))
    ).to.eq(results.staticRotateTowards04, 'staticRotateTowards04')
  })

  it('Quaternion.LookRotation', () => {
    expect(quaternionToString(Quaternion.LookRotation(new Vector3(1, 1, 1), Vector3.Up()))).to.eq(
      results.staticLookRotation01,
      'staticLookRotation01'
    )

    expect(quaternionToString(Quaternion.LookRotation(new Vector3(-10, -0, 110), Vector3.Up()))).to.eq(
      results.staticLookRotation02,
      'staticLookRotation02'
    )

    expect(quaternionToString(Quaternion.LookRotation(new Vector3(1230, 10, 0), Vector3.Up()))).to.eq(
      results.staticLookRotation03,
      'staticLookRotation03'
    )

    expect(quaternionToString(Quaternion.LookRotation(new Vector3(0, 123, -123), Vector3.Up()))).to.eq(
      results.staticLookRotation04,
      'staticLookRotation04'
    )
  })

  it.skip('Quaternion.FromToRotation', () => {
    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(0, 0, 0), new Vector3(100, 100, 100)))).to.eq(
      results.staticFromToRotation01,
      'staticFromToRotation01'
    )

    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(-10, -0, 110), new Vector3(4452, 0, 100)))).to.eq(
      results.staticFromToRotation02,
      'staticFromToRotation02'
    )

    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(1230, 10, 0), new Vector3(100, 100, 0)))).to.eq(
      results.staticFromToRotation03,
      'staticFromToRotation03'
    )

    expect(
      quaternionToString(Quaternion.FromToRotation(new Vector3(0, 123, -123), new Vector3(100, 213123, 100)))
    ).to.eq(results.staticFromToRotation04, 'staticFromToRotation04')

    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(-10, -10, -10), new Vector3(360, -10, 360)))).to.eq(
      results.staticFromToRotation05,
      'staticFromToRotation05'
    )

    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(12, -0, -400), new Vector3(200, 360, -400)))).to.eq(
      results.staticFromToRotation06,
      'staticFromToRotation06'
    )

    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(25, 45, 180), new Vector3(127, 0, 90)))).to.eq(
      results.staticFromToRotation07,
      'staticFromToRotation07'
    )

    expect(quaternionToString(Quaternion.FromToRotation(new Vector3(0, 1, 0), new Vector3(1, 1, 1)))).to.eq(
      results.staticFromToRotation08,
      'staticFromToRotation08'
    )
  })

  it('quaternion.eulerAngles', () => {
    expect(vector3ToString(Quaternion.Euler(10, 10, 10).eulerAngles)).to.eq(results.eulerAngles01, 'eulerAngles01')

    expect(vector3ToString(Quaternion.Euler(0, 90, 0).eulerAngles)).to.eq(results.eulerAngles02, 'eulerAngles02')

    expect(vector3ToString(Quaternion.Euler(100, 10, 400).eulerAngles)).to.eq(results.eulerAngles03, 'eulerAngles03')

    expect(vector3ToString(Quaternion.Euler(360, 10, 0).eulerAngles)).to.eq(results.eulerAngles04, 'eulerAngles04')
  })

  it('quaternion.normalized', () => {
    expect(quaternionToString(Quaternion.Euler(10, 10, 10).normalized)).to.eq(results.normalized01, 'normalized01')

    expect(quaternionToString(Quaternion.Euler(0, 90, 0).normalized)).to.eq(results.normalized02, 'normalized02')

    expect(quaternionToString(Quaternion.Euler(100, 10, 400).normalized)).to.eq(results.normalized03, 'normalized03')

    expect(quaternionToString(Quaternion.Euler(360, 10, 0).normalized)).to.eq(results.normalized04, 'normalized04')
  })

  it.skip('quaternion.setFromToRotation', () => {
    const q1 = Quaternion.Euler(10, 10, 10)
    q1.setFromToRotation(new Vector3(0, 0, 0), new Vector3(100, 100, 100))
    expect(quaternionToString(q1)).to.eq(results.setFromToRotation01, 'setFromToRotation01')
  })

  it('Quaternion.Slerp', () => {
    expect(quaternionToString(Quaternion.Slerp(Quaternion.Euler(10, 10, 10), Quaternion.Euler(45, 45, 45), 1))).to.eq(
      results.staticSlerp01,
      'staticSlerp01'
    )

    expect(
      quaternionToString(Quaternion.Slerp(Quaternion.Euler(-10, -0, 110), Quaternion.Euler(4100, 100, 100), 0.00123))
    ).to.eq(results.staticSlerp02, 'staticSlerp02')

    expect(
      quaternionToString(Quaternion.Slerp(Quaternion.Euler(0, 123, -123), Quaternion.Euler(360, -10, 360), 0.9))
    ).to.eq(results.staticSlerp03, 'staticSlerp03')

    expect(quaternionToString(Quaternion.Slerp(Quaternion.Euler(1, 1, 0), Quaternion.Euler(12, 12341, 1), 0.4))).to.eq(
      results.staticSlerp04,
      'staticSlerp04'
    )
  })
})
