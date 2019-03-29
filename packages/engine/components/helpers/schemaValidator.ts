import * as BABYLON from 'babylonjs'
import { ReadOnlyVector2, ReadOnlyVector3, ReadOnlyQuaternion } from 'decentraland-ecs/src'

export type ISchema<Keys> = { [key: string]: { type: keyof Keys; default?: any } }
export type Validator<T = any> = (x: any, defaultValue: T) => T

export const validators = {
  int(x: any, def: number) {
    if (x === null || x === undefined) return def
    if (typeof x === 'number' && isFinite(x)) return x | 0
    try {
      const tmp = parseInt(x, 10)
      if (isFinite(tmp)) return tmp | 0
    } catch (e) {
      return def | 0
    }
    return def | 0
  },

  float(x: any, def: number) {
    if (x === null || x === undefined) return def
    if (typeof x === 'number' && isFinite(x)) return x
    try {
      const tmp = parseFloat(x)
      if (isFinite(tmp)) return tmp
    } catch (e) {
      return def
    }
    return def
  },

  number(x: any, def: number) {
    if (typeof x === 'number' && isFinite(x)) return x
    try {
      const tmp = parseFloat(x)
      if (isFinite(tmp)) return tmp
    } catch (e) {
      return def
    }
    return def
  },

  boolean(x: any, def: boolean): boolean {
    if (x === null || x === undefined) return def
    if (x === false || x === 'false' || x === 0 || x === '') {
      return false
    } else if (x === true || x === 'true') {
      return true
    } else if (isFinite(x)) {
      return parseFloat(x) !== 0
    }
    return def
  },

  string(x: any, def: string): string {
    if (x === null || x === undefined) return def
    const type = typeof x
    if (type === 'string') {
      return x
    }

    if (x != null && type === 'object') {
      return x.toString()
    }

    if (x === null || x === undefined) {
      return def
    }

    return String(x)
  },

  vector2(value: any, def: ReadOnlyVector2): ReadOnlyVector2 {
    if (value === null || value === undefined) return def

    if (Number.isFinite(value)) {
      return { x: value, y: value }
    }

    const validType = value != null && typeof value === 'object' && 'x' in value && 'y' in value
    const validNumbers = validType && Number.isFinite(value.x) && Number.isFinite(value.y)

    if (validNumbers) {
      return value
    } else {
      return def
    }
  },

  vector3(value: any, def: ReadOnlyVector3): ReadOnlyVector3 {
    if (value === null || value === undefined) return def

    if (Number.isFinite(value)) {
      return { x: value, y: value, z: value }
    }

    const validType = value != null && typeof value === 'object' && 'x' in value && 'y' in value && 'z' in value
    const validNumbers = validType && Number.isFinite(value.x) && Number.isFinite(value.y) && Number.isFinite(value.z)

    if (validNumbers) {
      return value
    } else {
      return def
    }
  },

  quaternion(value: any, def: ReadOnlyQuaternion): ReadOnlyQuaternion {
    if (value === null || value === undefined) return def

    if (Number.isFinite(value)) {
      return { x: value, y: value, z: value, w: value }
    }

    const validType =
      value != null && typeof value === 'object' && 'x' in value && 'y' in value && 'z' in value && 'w' in value
    const validNumbers =
      validType &&
      Number.isFinite(value.x) &&
      Number.isFinite(value.y) &&
      Number.isFinite(value.z) &&
      Number.isFinite(value.w)

    if (validNumbers) {
      return value
    } else {
      return def
    }
  },

  color(x: any, def: BABYLON.Color3) {
    if (x === null || x === undefined) return def
    const color = BABYLON.Color3.Black()
    if (typeof x === 'string') {
      const v = x.trim()
      if (v.startsWith('#')) {
        color.copyFrom(BABYLON.Color3.FromHexString(x))
      }
    } else if (typeof x === 'object' && (x.r !== undefined && x.g !== undefined && x.b !== undefined)) {
      color.copyFrom(x)
    } else if (typeof x === 'number') {
      color.copyFrom(BABYLON.Color3.FromHexString('#' + ('000000' + (x | 0).toString(16)).substr(-6)))
    }
    return color
  },

  side(val: any, def: number) {
    if (val === 0 || val === 1 || val === 2) {
      return val
    }
    if (val === 'back') {
      return BABYLON.Mesh.BACKSIDE
    } else if (val === 'double') {
      return BABYLON.Mesh.DOUBLESIDE
    } else if (val === 'front') {
      return BABYLON.Mesh.FRONTSIDE
    }

    return def
  },

  floatArray(x: any, def: number[]) {
    let ret = []
    if (x === null || x === undefined || x.length === 0 || x.constructor !== Array) return def
    for (let i = 0; i < x.length; i++) {
      ret.push(validators.float(x[i], 0))
    }
    return ret
  }
}

export function createSchemaValidator(schema: ISchema<typeof validators>) {
  const schemaKeys = Object.keys(schema)

  return Object.assign(
    function(input: any) {
      if (input != null && typeof input === 'object') {
        for (let k = 0; k < schemaKeys.length; k++) {
          const key = schemaKeys[k]
          if (key in input) {
            input[key] = (validators[schema[key].type] as Validator)(input[key], schema[key].default)
          } else if (typeof schema[key].default !== 'undefined') {
            input[key] = schema[key].default
          }
        }
        return input
      } else {
        return null
      }
    },
    { schema, schemaKeys }
  )
}
