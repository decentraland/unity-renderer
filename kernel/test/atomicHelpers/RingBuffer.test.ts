import { RingBuffer } from 'atomicHelpers/RingBuffer'
import { expect } from 'chai'

describe('RingBuffer', () => {
  let buffer: RingBuffer<Float32Array>
  beforeEach(() => {
    buffer = new RingBuffer(20, Float32Array)
  })

  it('can write and read simple operations', () => {
    buffer.write(Float32Array.of(10, 20, 30))

    expect(buffer.read()).to.equal(Float32Array.of(10, 20, 30))

    buffer.write(Float32Array.of(40, 50, 60))
    expect(buffer.read()).to.equal(Float32Array.of(40, 50, 60))
  })

  it('can write multiple and read once', () => {
    buffer.write(Float32Array.of(10, 20, 30))
    buffer.write(Float32Array.of(40, 50, 60))

    expect(buffer.read()).to.equal(Float32Array.of(10, 20, 30, 40, 50, 60))
  })

  it('can write when the buffer is full, overwriting the first values', () => {
    const toWrite = new Float32Array(buffer.size)

    toWrite.fill(1)

    buffer.write(toWrite)
    buffer.write(Float32Array.of(10, 20, 30))

    const expected = new Float32Array(toWrite)
    expected.set(Float32Array.of(10, 20, 30), toWrite.length - 3)

    expect(buffer.read()).to.equal(expected)
  })

  it('can write values bytes and still work as expected', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size)

      toWrite.fill(i)

      buffer.write(toWrite)
    }

    buffer.write(Float32Array.of(10, 20, 30))

    const expected = new Float32Array(buffer.size)
    expected.fill(9)
    expected.set(Float32Array.of(10, 20, 30), buffer.size - 3)

    expect(buffer.read()).to.equal(expected)
  })

  it('can write a large array and it keeps the last values', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size)

      toWrite.fill(i)

      buffer.write(toWrite)
    }

    buffer.write(Float32Array.of(10, 20, 30))

    const expected = new Float32Array(buffer.size)
    expected.fill(9)
    expected.set(Float32Array.of(10, 20, 30), buffer.size - 3)

    expect(buffer.read()).to.equal(expected)
  })
})
