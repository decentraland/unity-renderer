/**
 * @public
 */
export enum UIValueType {
  PERCENT = 0,
  PIXELS = 1
}

/**
 * @public
 */
export class UIValue {
  value: number
  type: UIValueType

  constructor(value: string | number) {
    this.type = UIValueType.PIXELS

    if (typeof value === 'string') {
      let valueAsString: string = value
      if (valueAsString.indexOf('px') > -1) {
        this.type = UIValueType.PIXELS
      } else if (valueAsString.indexOf('%') > -1) {
        this.type = UIValueType.PERCENT
      }

      this.value = parseFloat(valueAsString)
    } else {
      this.value = value
    }
  }

  toString(): string {
    let result: string = this.value.toString()

    if (this.type === UIValueType.PERCENT) {
      result += '%'
    } else {
      result += 'px'
    }

    return result
  }
}
