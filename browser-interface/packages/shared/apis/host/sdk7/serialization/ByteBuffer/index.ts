import * as utf8 from '@protobufjs/utf8'

/**
 * Take the max between currentSize and intendedSize and then plus 1024. Then,
 *  find the next nearer multiple of 1024.
 * @param currentSize - number
 * @param intendedSize - number
 * @returns the calculated number
 */
function getNextSize(currentSize: number, intendedSize: number) {
  const minNewSize = Math.max(currentSize, intendedSize) + 1024
  return Math.ceil(minNewSize / 1024) * 1024
}

const defaultInitialCapacity = 10240

/**
 * ByteBuffer is a wrapper of DataView which also adds a read and write offset.
 *  Also in a write operation it resizes the buffer is being used if it needs.
 *
 * - Use read and write function to generate or consume data.
 * - Use set and get only if you are sure that you're doing.
 *
 * It always passes littleEndian param as true
 */
export class ReadWriteByteBuffer implements ByteBuffer {
  _buffer: Uint8Array
  view: DataView
  woffset: number
  roffset: number
  /**
   * @param buffer - The initial buffer, provide a buffer if you need to set "initial capacity"
   * @param readingOffset - Set the cursor where begins to read. Default 0
   * @param writingOffset - Set the cursor to not start writing from the begin of it. Defaults to the buffer size
   */
  constructor(buffer?: Uint8Array | undefined, readingOffset?: number | undefined, writingOffset?: number | undefined) {
    this._buffer = buffer || new Uint8Array(defaultInitialCapacity)
    this.view = new DataView(this._buffer.buffer, this._buffer.byteOffset)
    this.woffset = writingOffset ?? (buffer ? this._buffer.length : null) ?? 0
    this.roffset = readingOffset ?? 0
  }

  /**
   * Increement the write offset and resize the buffer if it needs.
   */
  #woAdd(amount: number) {
    if (this.woffset + amount > this._buffer.byteLength) {
      const newsize = getNextSize(this._buffer.byteLength, this.woffset + amount)
      const newBuffer = new Uint8Array(newsize)
      newBuffer.set(this._buffer)
      const oldOffset = this._buffer.byteOffset
      this._buffer = newBuffer
      this.view = new DataView(this._buffer.buffer, oldOffset)
    }

    this.woffset += amount
    return this.woffset - amount
  }

  /**
   * Increment the read offset and throw an error if it's trying to read
   *  outside the bounds.
   */
  #roAdd(amount: number) {
    if (this.roffset + amount > this.woffset) {
      throw new Error('Outside of the bounds of writen data.')
    }

    this.roffset += amount
    return this.roffset - amount
  }

  buffer(): Uint8Array {
    return this._buffer
  }
  bufferLength(): number {
    return this._buffer.length
  }
  resetBuffer(): void {
    this.roffset = 0
    this.woffset = 0
  }
  currentReadOffset(): number {
    return this.roffset
  }
  currentWriteOffset(): number {
    return this.woffset
  }
  incrementReadOffset(amount: number): number {
    return this.#roAdd(amount)
  }
  remainingBytes(): number {
    return this.woffset - this.roffset
  }
  readFloat32(): number {
    return this.view.getFloat32(this.#roAdd(4), true) // littleEndian = true
  }
  readFloat64(): number {
    return this.view.getFloat64(this.#roAdd(8), true) // littleEndian = true
  }
  readInt8(): number {
    return this.view.getInt8(this.#roAdd(1))
  }
  readInt16(): number {
    return this.view.getInt16(this.#roAdd(2), true) // littleEndian = true
  }
  readInt32(): number {
    return this.view.getInt32(this.#roAdd(4), true) // littleEndian = true
  }
  readInt64(): bigint {
    return this.view.getBigInt64(this.#roAdd(8), true) // littleEndian = true
  }
  readUint8(): number {
    return this.view.getUint8(this.#roAdd(1))
  }
  readUint16(): number {
    return this.view.getUint16(this.#roAdd(2), true) // littleEndian = true
  }
  readUint32(): number {
    return this.view.getUint32(this.#roAdd(4), true) // littleEndian = true
  }
  readUint64(): bigint {
    return this.view.getBigUint64(this.#roAdd(8), true) // littleEndian = true
  }
  readBuffer() {
    const length = this.view.getUint32(this.#roAdd(4), true) // littleEndian = true
    return this._buffer.subarray(this.#roAdd(length), this.#roAdd(0))
  }
  readUtf8String() {
    const length = this.view.getUint32(this.#roAdd(4), true) // littleEndian = true
    return utf8.read(this._buffer, this.#roAdd(length), this.#roAdd(0))
  }
  incrementWriteOffset(amount: number): number {
    return this.#woAdd(amount)
  }
  toBinary() {
    return this._buffer.subarray(0, this.woffset)
  }
  toCopiedBinary() {
    return new Uint8Array(this.toBinary())
  }
  writeBuffer(value: Uint8Array, writeLength: boolean = true) {
    if (writeLength) {
      this.writeUint32(value.byteLength)
    }

    const o = this.#woAdd(value.byteLength)
    this._buffer.set(value, o)
  }
  writeUtf8String(value: string, writeLength: boolean = true) {
    const byteLength = utf8.length(value)

    if (writeLength) {
      this.writeUint32(byteLength)
    }

    const o = this.#woAdd(byteLength)

    utf8.write(value, this._buffer, o)
  }
  writeFloat32(value: number): void {
    const o = this.#woAdd(4)
    this.view.setFloat32(o, value, true) // littleEndian = true
  }
  writeFloat64(value: number): void {
    const o = this.#woAdd(8)
    this.view.setFloat64(o, value, true) // littleEndian = true
  }
  writeInt8(value: number): void {
    const o = this.#woAdd(1)
    this.view.setInt8(o, value)
  }
  writeInt16(value: number): void {
    const o = this.#woAdd(2)
    this.view.setInt16(o, value, true) // littleEndian = true
  }
  writeInt32(value: number): void {
    const o = this.#woAdd(4)
    this.view.setInt32(o, value, true) // littleEndian = true
  }
  writeInt64(value: bigint): void {
    const o = this.#woAdd(8)
    this.view.setBigInt64(o, value, true) // littleEndian = true
  }
  writeUint8(value: number): void {
    const o = this.#woAdd(1)
    this.view.setUint8(o, value)
  }
  writeUint16(value: number): void {
    const o = this.#woAdd(2)
    this.view.setUint16(o, value, true) // littleEndian = true
  }
  writeUint32(value: number): void {
    const o = this.#woAdd(4)
    this.view.setUint32(o, value, true) // littleEndian = true
  }
  writeUint64(value: bigint): void {
    const o = this.#woAdd(8)
    this.view.setBigUint64(o, value, true) // littleEndian = true
  }
  // DataView Proxy
  getFloat32(offset: number): number {
    return this.view.getFloat32(offset, true) // littleEndian = true
  }
  getFloat64(offset: number): number {
    return this.view.getFloat64(offset, true) // littleEndian = true
  }
  getInt8(offset: number): number {
    return this.view.getInt8(offset)
  }
  getInt16(offset: number): number {
    return this.view.getInt16(offset, true) // littleEndian = true
  }
  getInt32(offset: number): number {
    return this.view.getInt32(offset, true) // littleEndian = true
  }
  getInt64(offset: number): bigint {
    return this.view.getBigInt64(offset, true) // littleEndian = true
  }
  getUint8(offset: number): number {
    return this.view.getUint8(offset)
  }
  getUint16(offset: number): number {
    return this.view.getUint16(offset, true) // littleEndian = true
  }
  getUint32(offset: number): number {
    return this.view.getUint32(offset, true) // littleEndian = true >>> 0
  }
  getUint64(offset: number): bigint {
    return this.view.getBigUint64(offset, true) // littleEndian = true
  }
  setFloat32(offset: number, value: number): void {
    this.view.setFloat32(offset, value, true) // littleEndian = true
  }
  setFloat64(offset: number, value: number): void {
    this.view.setFloat64(offset, value, true) // littleEndian = true
  }
  setInt8(offset: number, value: number): void {
    this.view.setInt8(offset, value)
  }
  setInt16(offset: number, value: number): void {
    this.view.setInt16(offset, value, true) // littleEndian = true
  }
  setInt32(offset: number, value: number): void {
    this.view.setInt32(offset, value, true) // littleEndian = true
  }
  setInt64(offset: number, value: bigint): void {
    this.view.setBigInt64(offset, value, true) // littleEndian = true
  }
  setUint8(offset: number, value: number): void {
    this.view.setUint8(offset, value)
  }
  setUint16(offset: number, value: number): void {
    this.view.setUint16(offset, value, true) // littleEndian = true
  }
  setUint32(offset: number, value: number): void {
    this.view.setUint32(offset, value, true) // littleEndian = true
  }
  setUint64(offset: number, value: bigint): void {
    this.view.setBigUint64(offset, value, true) // littleEndian = true
  }
}

/**
 * @public
 */
export interface ByteBuffer {
  /**
   * @returns The entire current Uint8Array.
   *
   * WARNING: if the buffer grows, the view had changed itself,
   *  and the reference will be a invalid one.
   */
  buffer(): Uint8Array
  /**
   * @returns The capacity of the current buffer
   */
  bufferLength(): number
  /**
   * Resets byteBuffer to avoid creating a new one
   */
  resetBuffer(): void
  /**
   * @returns The current read offset
   */
  currentReadOffset(): number
  /**
   * @returns The current write offset
   */
  currentWriteOffset(): number
  /**
   * Reading purpose
   * Returns the previuos offsset size before incrementing
   */
  incrementReadOffset(amount: number): number
  /**
   * @returns How many bytes are available to read.
   */
  remainingBytes(): number
  readFloat32(): number
  readFloat64(): number
  readInt8(): number
  readInt16(): number
  readInt32(): number
  readInt64(): bigint
  readUint8(): number
  readUint16(): number
  readUint32(): number
  readUint64(): bigint
  readBuffer(): Uint8Array
  readUtf8String(): string
  /**
   * Writing purpose
   */
  /**
   * Increment offset
   * @param amount - how many bytes
   * @returns The offset when this reserving starts.
   */
  incrementWriteOffset(amount: number): number
  /**
   * Take care using this function, if you modify the data after, the
   * returned subarray will change too. If you'll modify the content of the
   * bytebuffer, maybe you want to use toCopiedBinary()
   *
   * @returns The subarray from 0 to offset as reference.
   */
  toBinary(): Uint8Array

  /**
   * Safe copied buffer of the current data of ByteBuffer
   *
   * @returns The subarray from 0 to offset.
   */
  toCopiedBinary(): Uint8Array

  writeUtf8String(value: string, writeLength?: boolean): void
  writeBuffer(value: Uint8Array, writeLength?: boolean): void
  writeFloat32(value: number): void
  writeFloat64(value: number): void
  writeInt8(value: number): void
  writeInt16(value: number): void
  writeInt32(value: number): void
  writeInt64(value: bigint): void
  writeUint8(value: number): void
  writeUint16(value: number): void
  writeUint32(value: number): void
  writeUint64(value: bigint): void
  // Dataview Proxy
  getFloat32(offset: number): number
  getFloat64(offset: number): number
  getInt8(offset: number): number
  getInt16(offset: number): number
  getInt32(offset: number): number
  getInt64(offset: number): bigint
  getUint8(offset: number): number
  getUint16(offset: number): number
  getUint32(offset: number): number
  getUint64(offset: number): bigint
  setFloat32(offset: number, value: number): void
  setFloat64(offset: number, value: number): void
  setInt8(offset: number, value: number): void
  setInt16(offset: number, value: number): void
  setInt32(offset: number, value: number): void
  setInt64(offset: number, value: bigint): void
  setUint8(offset: number, value: number): void
  setUint16(offset: number, value: number): void
  setUint32(offset: number, value: number): void
  setUint64(offset: number, value: bigint): void
}
