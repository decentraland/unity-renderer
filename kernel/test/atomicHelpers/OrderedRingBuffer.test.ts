import { OrderedRingBuffer } from 'atomicHelpers/RingBuffer'
import { expect } from 'chai'

describe('OrderedRingBuffer', () => {
  let buffer: OrderedRingBuffer<Float32Array>
  beforeEach(() => {
    buffer = new OrderedRingBuffer(20, Float32Array)
  })

  it('can write and read simple operations', () => {
    buffer.write(Float32Array.of(10, 20, 30), 0)

    expect(buffer.read()).to.eql(Float32Array.of(10, 20, 30))

    buffer.write(Float32Array.of(40, 50, 60), 1)
    expect(buffer.read()).to.eql(Float32Array.of(40, 50, 60))
  })

  it('can write multiple and read once', () => {
    buffer.write(Float32Array.of(10, 20, 30), 0)
    buffer.write(Float32Array.of(40, 50, 60), 1)

    expect(buffer.read()).to.eql(Float32Array.of(10, 20, 30, 40, 50, 60))
  })

  it('can write when the buffer is full, overwriting the first values', () => {
    const toWrite = new Float32Array(buffer.size)

    toWrite.fill(1)

    buffer.write(toWrite, 0)
    buffer.write(Float32Array.of(10, 20, 30), 1)

    const expected = new Float32Array(toWrite)
    expected.set(Float32Array.of(10, 20, 30), toWrite.length - 3)

    expect(buffer.read()).to.eql(expected)
  })

  it('can write values bytes and still work as expected', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size)

      toWrite.fill(i)

      buffer.write(toWrite, i)
    }

    buffer.write(Float32Array.of(10, 20, 30), 11)

    const expected = new Float32Array(buffer.size)
    expected.fill(9)
    expected.set(Float32Array.of(10, 20, 30), buffer.size - 3)

    expect(buffer.read()).to.eql(expected)
  })

  it('can write a large array and it keeps the last values', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size)

      toWrite.fill(i)

      buffer.write(toWrite, i)
    }

    buffer.write(Float32Array.of(10, 20, 30), 11)

    const expected = new Float32Array(buffer.size)
    expected.fill(9)
    expected.set(Float32Array.of(10, 20, 30), buffer.size - 3)

    expect(buffer.read()).to.eql(expected)
  })

  it('can write out of order and it gets ordered on read', () => {
    buffer.write(Float32Array.of(40, 50, 60), 1)
    buffer.write(Float32Array.of(10, 20, 30), 0)

    expect(buffer.read()).to.eql(Float32Array.of(10, 20, 30, 40, 50, 60))
  })

  it('can write at random order and it gets ordered on read', () => {
    const chunksToWrite = buffer.size / 2
    const toWrite: Record<number, Float32Array> = {}
    const expected = new Float32Array(buffer.size)

    for (let i = 0; i < chunksToWrite; i++) {
      const chunk = Float32Array.of(i * 100 + 1, i * 100 + 2)
      toWrite[i] = chunk
      expected.set(chunk, i * 2)
    }

    const randomized = Object.keys(toWrite).sort(() => Math.random() - 0.5)

    randomized.forEach((i) => buffer.write(toWrite[i], parseInt(i)))

    expect(buffer.read()).to.eql(expected)
  })

  it('when writing discards part of buffer, old chunks get resized but not discarded', () => {
    const toWrite = new Float32Array(buffer.size)

    toWrite.fill(1)

    buffer.write(toWrite, 0)

    const second = Float32Array.of(40, 50, 60)
    const first = Float32Array.of(10, 20, 30)

    buffer.write(second, 2)
    buffer.write(first, 1)

    const expected = new Float32Array(buffer.size)
    expected.fill(1)

    expected.set(first, expected.length - first.length - second.length)
    expected.set(second, expected.length - second.length)

    expect(buffer.read()).to.eql(expected)
  })

  it('when writing discards whole old chunks, they get cleaned up', () => {
    for (let i = 0; i < 10; i++) {
      const toWrite = new Float32Array(buffer.size / 5)

      toWrite.fill(i)

      buffer.write(toWrite, i * 10)
    }

    //@ts-ignore
    expect(buffer.chunks.length).to.eql(5)

    expect(buffer.peek()).to.eql(Float32Array.of(5, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7, 7, 8, 8, 8, 8, 9, 9, 9, 9))

    // We write something in the middle of 5 and 6, and something in the middle of 7 and 8 and something at the end
    buffer.write(Float32Array.of(11, 11, 11, 11), 55)

    buffer.write(Float32Array.of(22, 22, 22, 22), 75)

    buffer.write(Float32Array.of(33, 33), 95)

    expect(buffer.read()).to.eql(Float32Array.of(6, 6, 7, 7, 7, 7, 22, 22, 22, 22, 8, 8, 8, 8, 9, 9, 9, 9, 33, 33))
  })
})
