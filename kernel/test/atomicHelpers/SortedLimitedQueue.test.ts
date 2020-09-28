import { SortedLimitedQueue } from 'atomicHelpers/SortedLimitedQueue'
import { expect } from 'chai'

describe('SortedLimitedQueue', () => {
  let queue: SortedLimitedQueue<number>
  const criteria = (a: number, b: number) => a - b

  const queueLength = 10

  beforeEach(() => {
    queue = new SortedLimitedQueue(queueLength, criteria)
  })

  it('can queue and dequeue elements', () => {
    queue.queue(1)
    queue.queue(2)

    expect(queue.dequeue()).to.eql(1)
    expect(queue.dequeue()).to.eql(2)
    expect(queue.dequeue()).to.be.undefined
  })

  it('can queue out of order and it gets ordered', () => {
    queue.queue(4)
    queue.queue(2)
    queue.queue(7)
    queue.queue(1)

    expect(queue.dequeueItems(4)).to.eql([1, 2, 4, 7])
  })

  it('can queue in random order and it gets ordered', () => {
    const expected = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

    expected
      .slice()
      .sort(() => Math.random() - 0.5)
      .forEach((item) => queue.queue(item))

    expect(queue.dequeueItems(10)).to.eql(expected)
  })

  it('can queue more than the length limit and the first elements get removed', () => {
    const queued = []
    const length = 20

    for (let i = 0; i < length; i++) {
      const item = Math.floor(Math.random() * 1000)
      queued.push(item)
      queue.queue(item)
    }

    const expected = queued.sort(criteria).slice(length - queueLength, length)

    expect(queue.dequeueItems(10)).to.eql(expected)
  })

  it('can queue any element with custom sorting', () => {
    const queue = new SortedLimitedQueue<{ name: string; order: number }>(10, (a, b) => a.order - b.order)

    queue.queue({ name: 'second', order: 2 })
    queue.queue({ name: 'fourth', order: 4 })
    queue.queue({ name: 'third', order: 3 })
    queue.queue({ name: 'first', order: 1 })

    expect(queue.dequeueItems(4).map((it) => it.name)).to.eql(['first', 'second', 'third', 'fourth'])
  })

  it('can await elements to be dequeued', async () => {
    queue.queue(1)
    queue.queue(2)

    const result = await queue.dequeueItemsWhenAvailable(2, 100)

    expect(result).to.eql([1, 2])
  })

  it('can await elements with a timeout', async () => {
    let timedout = false

    queue.queue(1)

    setTimeout(() => (timedout = true), 5)

    const result = await queue.dequeueItemsWhenAvailable(2, 10)

    expect(result).to.eql([1])
    expect(timedout).to.be.true
  })

  it('can await elements with a timeout and it returns empty array', async () => {
    let timedout = false

    setTimeout(() => (timedout = true), 5)

    const result = await queue.dequeueItemsWhenAvailable(2, 10)

    expect(result).to.eql([])
    expect(timedout).to.be.true
  })

  it('can await elements and it blocks until elements are available', async () => {
    let blocked = false
    let timedout = false

    queue.queue(1)

    setTimeout(() => (blocked = true), 5)
    setTimeout(() => queue.queue(2), 10)
    setTimeout(() => (timedout = true), 50)

    const result = await queue.dequeueItemsWhenAvailable(2, 100)

    expect(result).to.eql([1, 2])
    expect(timedout).to.be.false
    expect(blocked).to.be.true
  })

  it('puts the elements in the order they came if they have the same order value', () => {
    const queue = new SortedLimitedQueue<{ name: string; order: number }>(10, (a, b) => a.order - b.order)

    queue.queue({ name: 'first', order: 1 })
    queue.queue({ name: 'second', order: 1 })

    expect(queue.dequeueItems(2).map((it) => it.name)).to.eql(['first', 'second'])
  })
})
