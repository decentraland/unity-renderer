import { expect } from 'chai'
import { shallowEqual } from 'lib/javascript/shallowEqual'
import { deepEqual } from 'lib/javascript/deepEqual'

function testComparer(comparer: Function, a: any, b: any, expectedResult: boolean) {
  expect(comparer(a, a)).to.eq(true)
  expect(comparer(b, b)).to.eq(true)
  expect(comparer(a, b)).to.eq(expectedResult)
}

describe('object compare', function() {
  it(`shallowEqual`, () => {
    testComparer(shallowEqual, 1, 1, true)
    testComparer(shallowEqual, 1, 2, false)
    testComparer(shallowEqual, [1], [2], false)
    testComparer(shallowEqual, [1], [1], true)
    testComparer(shallowEqual, [1], [1], true)
    testComparer(shallowEqual, [true], [true], true)
    testComparer(shallowEqual, [true], [false], false)
    testComparer(shallowEqual, [true], [true, false], false)
    testComparer(shallowEqual, [true, false], [true], false)
    testComparer(shallowEqual, [true, false], [true, false], true)
    testComparer(shallowEqual, { a: true, b: false }, { b: false, a: true }, true)
    testComparer(shallowEqual, { a: true, b: { a: true } }, { b: { a: true }, a: true }, false)
  })

  it(`deepEqual`, () => {
    testComparer(deepEqual, 1, 1, true)
    testComparer(deepEqual, 1, 2, false)
    testComparer(deepEqual, [1], [2], false)
    testComparer(deepEqual, [1], [1], true)
    testComparer(deepEqual, [1], [1], true)
    testComparer(deepEqual, [true], [true], true)
    testComparer(deepEqual, [true], [false], false)
    testComparer(deepEqual, [true], [true, false], false)
    testComparer(deepEqual, [true, false], [true], false)
    testComparer(deepEqual, [true, false], [true, false], true)
    testComparer(deepEqual, { a: true, b: false }, { b: false, a: true }, true)
    testComparer(deepEqual, { a: true, b: { a: true, c: [1, 2, 3] } }, { b: { c: [1, 2, 3], a: true }, a: true }, true)
    testComparer(deepEqual, { a: true, b: { a: true } }, { b: { a: true }, a: true }, true)
    testComparer(
      deepEqual,
      { position: { delay: 0, duration: 100, timing: 'linear' } },
      { position: { duration: 100, delay: 0, timing: 'linear' } },
      true
    )
  })
})
