import future, { IFuture } from 'fp-future'

type TypedArray =
  | Int8Array
  | Uint8Array
  | Uint8ClampedArray
  | Int16Array
  | Uint16Array
  | Int32Array
  | Uint32Array
  | Float32Array
  | Float64Array

export class RingBuffer<T extends TypedArray> {
  private writePointer: number = 0
  private readPointer: number = 0
  private buffer: T

  constructor(public readonly size: number, private readonly ArrayTypeConstructor: { new (size: number): T }) {
    this.buffer = new this.ArrayTypeConstructor(size)
  }

  readAvailableCount() {
    return this.writePointer - this.readPointer
  }

  getWritePointer() {
    return this.writePointer
  }

  getReadPointer() {
    return this.readPointer
  }

  write(array: T, length?: number) {
    this.writeAt(array, this.writePointer, length)
  }

  read(readCount?: number): T {
    const result = this.peek(this.readPointer, readCount)

    this.readPointer += result.length

    return result
  }

  peek(startPointer?: number, readCount?: number): T {
    const start = startPointer ? startPointer : this.readPointer

    const maxCountToRead = this.writePointer - this.readPointer

    const count = readCount ? Math.min(readCount, maxCountToRead) : maxCountToRead

    const readPosition = start % this.buffer.length

    const endIndex = readPosition + count

    let result: T

    if (endIndex > this.buffer.length) {
      result = new this.ArrayTypeConstructor(count)
      result.set(this.buffer.slice(readPosition, this.buffer.length))
      result.set(this.buffer.slice(0, endIndex - this.buffer.length), this.buffer.length - readPosition)
    } else {
      result = this.buffer.slice(readPosition, endIndex) as T
    }

    return result
  }

  writeAt(array: T, startPointer: number, length?: number) {
    const len = length || array.length

    let toWrite = array
    if (len > this.buffer.length) {
      // If too many bytes are provided, we only write the last ones.
      toWrite = array.slice(array.length - this.buffer.length, array.length) as T
    }

    const writePosition = startPointer % this.buffer.length

    const endIndex = writePosition + len

    if (endIndex > this.buffer.length) {
      const partitionIndex = this.buffer.length - writePosition
      this.buffer.set(toWrite.slice(0, partitionIndex), writePosition)
      this.buffer.set(toWrite.slice(partitionIndex, len), 0)
    } else {
      this.buffer.set(toWrite.slice(0, len), writePosition)
    }

    const endPointer = startPointer + len

    if (endPointer > this.writePointer) {
      this.writePointer = endPointer
    }

    this.updateReadPointerToMinReadPosition()
  }

  isFull() {
    return this.readAvailableCount() >= this.size
  }

  private updateReadPointerToMinReadPosition() {
    const minReadPointer = this.writePointer - this.buffer.length

    if (this.readPointer < minReadPointer) {
      this.readPointer = minReadPointer
    }
  }
}

type Chunk = {
  order: number
  startPointer: number
  length: number
}

export class OrderedRingBuffer<T extends TypedArray> {
  private internalRingBuffer: RingBuffer<T>

  private chunks: Chunk[] = []

  private blockedReadChunksFuture?: { chunksToRead: number; future: IFuture<T[]> }

  constructor(public readonly size: number, ArrayTypeConstructor: { new (size: number): T }) {
    this.internalRingBuffer = new RingBuffer(size, ArrayTypeConstructor)
  }

  readAvailableCount() {
    return this.internalRingBuffer.readAvailableCount()
  }

  write(array: T, order: number, length?: number) {
    // We find those chunks that should be after this chunk
    const nextChunks = this.chunks.filter((it) => it.order > order)

    if (nextChunks.length === 0) {
      // If there are no chunks that should be after this chunk, then we just need to write the chunk at the end.
      this.chunks.push({
        order,
        startPointer: this.internalRingBuffer.getWritePointer(),
        length: length || array.length
      })

      this.internalRingBuffer.write(array, length)
    } else {
      // Otherwise, we need to get those chunks that should be after this one, and write them one after the other

      let writePointer = nextChunks[0].startPointer

      const newChunk = {
        order,
        startPointer: writePointer,
        length: length || array.length
      }

      // Chunks are ordered by "order", so we need to ensure that we place this new chunk in the corresponding index.
      this.chunks.splice(this.chunks.length - nextChunks.length, 0, newChunk)

      const arraysToWrite = [length ? array.slice(0, length) : array] as T[]

      // We get the arrays for each chunk, and we update their pointers while we are at it
      nextChunks.forEach((chunk) => {
        arraysToWrite.push(this.arrayForChunk(chunk))

        chunk.startPointer += newChunk.length
      })

      // We write starting from the position of the first chunk that will be rewritten
      arraysToWrite.forEach((toWrite) => {
        this.internalRingBuffer.writeAt(toWrite, writePointer)
        writePointer += toWrite.length
      })
    }

    this.discardUnreadableChunks()
    this.resolveBlockedRead()
  }

  arrayForChunk(chunk: Chunk): T {
    return this.peek(chunk.startPointer, chunk.length)
  }

  peek(startPointer?: number, readCount?: number): T {
    return this.internalRingBuffer.peek(startPointer, readCount)
  }

  read(readCount?: number): T {
    const result = this.internalRingBuffer.read(readCount)

    this.discardUnreadableChunks()

    return result
  }

  /**
   * The promise will block until there is chunksCount chunks to read,
   * or until timeToWait has passed.
   *
   * Once timeToWait has passed, if there is nothing to read, an empty array is returned.
   */
  async blockAndReadChunks(chunksCount: number, timeToWait: number): Promise<T[]> {
    if (this.chunks.length >= chunksCount) {
      const chunks = this.readChunks(chunksCount)

      return Promise.resolve(chunks)
    } else {
      if (this.blockedReadChunksFuture) {
        this.blockedReadChunksFuture.future.reject(new Error('Only one blocking call is possible at the same time'))
      }

      const thisFuture = { chunksToRead: chunksCount, future: future() }

      this.blockedReadChunksFuture = thisFuture

      setTimeout(() => {
        if (this.blockedReadChunksFuture === thisFuture) {
          if (this.chunks.length > 0) {
            thisFuture.future.resolve(this.readChunks(this.chunks.length))
          } else {
            thisFuture.future.resolve([])
          }
        }
      }, timeToWait)

      return this.blockedReadChunksFuture.future
    }
  }

  private readChunks(chunksCount: number) {
    return this.chunks.slice(0, chunksCount).map((it) => this.read(it.length))
  }

  private discardUnreadableChunks() {
    const isReadable = (chunk: Chunk) => {
      // A chunk is readable if its end pointer is ahead of the read pointer
      const endPointer = chunk.startPointer + chunk.length
      return endPointer > this.internalRingBuffer.getReadPointer()
    }

    this.chunks = this.chunks.filter(isReadable)

    if (this.chunks.length > 0 && this.chunks[0].startPointer < this.internalRingBuffer.getReadPointer()) {
      this.chunks[0].startPointer = this.internalRingBuffer.getReadPointer()
    }
  }

  private resolveBlockedRead() {
    if (this.blockedReadChunksFuture && this.chunks.length >= this.blockedReadChunksFuture.chunksToRead) {
      const read = this.readChunks(this.blockedReadChunksFuture.chunksToRead)
      this.blockedReadChunksFuture.future.resolve(read)
      delete this.blockedReadChunksFuture
    }
  }
}
