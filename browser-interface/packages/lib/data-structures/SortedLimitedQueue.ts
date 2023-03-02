import future, { IFuture } from 'fp-future'

export class SortedLimitedQueue<T> {
  private internalArray: T[]

  private pendingDequeue?: { amount: number; futures: IFuture<T[]>[]; timedoutValues?: T[] }

  constructor(private readonly maxLength: number, private readonly sortCriteria: (a: T, b: T) => number) {
    this.internalArray = []
  }

  queue(item: T): T | undefined {
    let insertIndex = 0
    let discardedItem: T | undefined = undefined

    // Since the most likely scenario for our use case is that we insert the item at the end,
    // we start by the end. This may be parameterized in the future
    for (let i = this.internalArray.length - 1; i >= 0; i--) {
      if (this.sortCriteria(item, this.internalArray[i]) >= 0) {
        insertIndex = i + 1
        break
      }
    }

    if (insertIndex === 0) {
      this.internalArray.unshift(item)
    } else if (insertIndex === this.internalArray.length) {
      this.internalArray.push(item)
    } else {
      this.internalArray.splice(insertIndex, 0, item)
    }

    if (this.internalArray.length > this.maxLength) {
      discardedItem = this.internalArray.shift()
    }

    this.resolveBlockedDequeues()

    return discardedItem
  }

  queuedCount() {
    return this.internalArray.length
  }

  dequeue(): T | undefined {
    return this.internalArray.shift()
  }

  dequeueItems(count?: number): T[] {
    return this.internalArray.splice(0, count ?? this.internalArray.length)
  }

  async dequeueItemsWhenAvailable(count: number, timeout: number): Promise<T[]> {
    if (this.pendingDequeue && this.pendingDequeue.amount !== count) {
      // To have multiple dequeue requests they all have to have the same amount. We prioritize the new request, and resolve the other with empty arrays.
      this.pendingDequeue.futures.forEach((it) => it.resolve([]))
    }

    if (this.queuedCount() >= count) {
      const items = this.dequeueItems(count)
      this.pendingDequeue?.futures.forEach((it) => it.resolve(items))
      this.pendingDequeue = undefined
      return Promise.resolve(items)
    } else {
      if (!this.pendingDequeue) {
        this.pendingDequeue = {
          amount: count,
          futures: []
        }
      }
      const newFuture = future<T[]>()
      this.pendingDequeue.futures.push(newFuture)

      setTimeout(() => {
        if (this.pendingDequeue && this.pendingDequeue.futures.indexOf(newFuture) >= 0) {
          this.resolveBlockedDequeueWith(this.queuedCount())
        }
      }, timeout)

      return newFuture
    }
  }

  isFull() {
    return this.queuedCount() >= this.maxLength
  }

  private resolveBlockedDequeues() {
    if (this.pendingDequeue && this.queuedCount() >= this.pendingDequeue.amount) {
      this.resolveBlockedDequeueWith(this.pendingDequeue.amount)
    }
  }

  private resolveBlockedDequeueWith(amount: number) {
    const items = amount === 0 ? [] : this.dequeueItems(amount)
    this.pendingDequeue?.futures.forEach((it) => it.resolve(items))
    this.pendingDequeue = undefined
  }
}
