import type { ReadOnlyColor4 } from '@dcl/ecs-math'
import { Color3 } from '@dcl/ecs-math'

export function convertToRGBObject(colorString: any): ReadOnlyColor4 {
  if (!colorString) return { r: 1, g: 0, b: 1, a: 1 }
  if (typeof colorString !== 'string') {
    if (colorString === undefined) {
      throw new Error('Unexpected undefined value for color object: ' + JSON.stringify(colorString))
    }
    if (colorString.color !== undefined) {
      return convertToRGBObject(colorString)
    }
    if (colorString.r === undefined || colorString.g === undefined || colorString.b === undefined) {
      throw new Error('Unexpected undefined value for color object: ' + JSON.stringify(colorString))
    }
    if (colorString.a === undefined) {
      colorString.a = 1
    }
    return colorString
  }
  if (colorString.length < 6) {
    throw new Error(`Unexpected value for RGBA: "${colorString}"`)
  }
  const r = convertSection(1, colorString)
  const g = convertSection(3, colorString)
  const b = convertSection(5, colorString)
  const a = colorString.length > 7 ? convertSection(7, colorString) : 1
  return { r, g, b, a }
}

export function rgbToHex({ r, g, b }: Omit<ReadOnlyColor4, 'a'>): string {
  return new Color3(r, g, b).toHexString()
}

export function convertSection(index: number, colorString: string) {
  const value = parseInt(colorString.slice(index, index + 2), 16) / 256
  if (value < 0) {
    return 0
  }
  if (value > 1) {
    return 1
  }
  if (isNaN(value)) {
    throw new Error(`Unexpected value: ${colorString} could not be turned into RGBA`)
  }
  return value
}
