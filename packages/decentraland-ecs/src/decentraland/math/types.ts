/**
 * Defines potential orientation for back face culling
 * @public
 */
export enum Orientation {
  /**
   * Clockwise
   */
  CW = 0,
  /** Counter clockwise */
  CCW = 1
}

/**
 * Defines supported spaces
 * @public
 */
export enum Space {
  /** Local (object) space */
  LOCAL = 0,
  /** World space */
  WORLD = 1,
  /** Bone space */
  BONE = 2
}

export type Nullable<T> = T | null

export type FloatArray = number[]
export type float = number
export type double = number

/**
 * Constant used to convert a value to gamma space
 */
export const ToGammaSpace = 1 / 2.2

/**
 * Constant used to convert a value to linear space
 */
export const ToLinearSpace = 2.2

/**
 * Constant used to define the minimal number value in Babylon.js
 */
export const Epsilon = 0.000001

/**
 * Constant used to convert from Euler degrees to radians
 */
export const DEG2RAD = Math.PI / 180

/**
 * Constant used to convert from radians to Euler degrees
 */
export const RAD2DEG = 360 / (Math.PI * 2)

/**
 * Interface for the size containing width and height
 * @public
 */
export interface ISize {
  /**
   * Width
   */
  width: number
  /**
   * Heighht
   */
  height: number
}
