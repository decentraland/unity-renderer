import { joinBuffers } from 'lib/javascript/uint8arrays'

export function base64ToBuffer(base64: string): Uint8Array {
  const sliceSize = 1024
  const byteChars = globalThis.atob(base64)
  const byteArrays: ArrayBuffer[] = []
  let len = byteChars.length

  for (let offset = 0; offset < len; offset += sliceSize) {
    const slice = byteChars.slice(offset, offset + sliceSize)

    const byteNumbers = new Array(slice.length)
    for (let i = 0; i < slice.length; i++) {
      byteNumbers[i] = slice.charCodeAt(i)
    }

    const byteArray = new Uint8Array(byteNumbers)

    byteArrays.push(byteArray)
    len = byteChars.length
  }

  return joinBuffers(...byteArrays)
}

export function base64ToBlob(base64: string, type: string = 'image/jpeg'): Blob {
  return new Blob([base64ToBuffer(base64)], { type })
}
