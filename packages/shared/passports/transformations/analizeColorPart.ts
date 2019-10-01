import { convertToRGBObject } from './convertToRGBObject'
export function analizeColorPart(avatar: any, ...alternativeNames: string[]) {
  for (let name of alternativeNames) {
    if (!avatar[name]) {
      continue
    }
    if (typeof avatar[name] === 'string') {
      if (avatar[name].length === 7) {
        return { color: convertToRGBObject(avatar[name]) }
      }
      if (avatar[name].length === 9) {
        return { color: convertToRGBObject(avatar[name]) }
      }
    }
    if (avatar[name]) {
      if (
        typeof avatar[name].r === 'number' &&
        typeof avatar[name].g === 'number' &&
        typeof avatar[name].b === 'number'
      ) {
        return avatar[name]
      }
    }
    if (avatar[name].color) {
      if (
        typeof avatar[name].color.r === 'number' &&
        typeof avatar[name].color.g === 'number' &&
        typeof avatar[name].color.b === 'number'
      ) {
        return avatar[name].color
      }
    }
  }
  throw new Error(
    'Unable to find a color between ' +
      JSON.stringify(alternativeNames) +
      ' in the submitted avatar model ' +
      JSON.stringify(avatar)
  )
}

export function stripAlpha(colorInput: any & { a: number }) {
  const { a, ...color } = colorInput
  return color
}
