import { Matrix } from './Matrix'
import { Vector3 } from './Vector3'
import { MathTmp } from './preallocatedVariables'
import { DEG2RAD, RAD2DEG } from './types'
import { Scalar } from "./Scalar"

/** @public */
export type ReadOnlyQuaternion = {
  readonly x: number
  readonly y: number
  readonly z: number
  readonly w: number
}

/**
 * Class used to store quaternion data
 * {@link https://en.wikipedia.org/wiki/Quaternion }
 * {@link http://doc.babylonjs.com/features/position,_rotation,_scaling }
 * @public
 */
export class Quaternion {
  /**
   * Creates a new Quaternion from the given floats
   * @param x - defines the first component (0 by default)
   * @param y - defines the second component (0 by default)
   * @param z - defines the third component (0 by default)
   * @param w - defines the fourth component (1.0 by default)
   */
  constructor(
    /** defines the first component (0 by default) */
    public x: number = 0.0,
    /** defines the second component (0 by default) */
    public y: number = 0.0,
    /** defines the third component (0 by default) */
    public z: number = 0.0,
    /** defines the fourth component (1.0 by default) */
    public w: number = 1.0
  ) {}

  // Statics

  /**
   * Creates a new quaternion from a rotation matrix
   * @param matrix - defines the source matrix
   * @returns a new quaternion created from the given rotation matrix values
   */
  public static FromRotationMatrix(matrix: Matrix): Quaternion {
    let result = new Quaternion()
    Quaternion.FromRotationMatrixToRef(matrix, result)
    return result
  }

  /**
   * Updates the given quaternion with the given rotation matrix values
   * @param matrix - defines the source matrix
   * @param result - defines the target quaternion
   */
  public static FromRotationMatrixToRef(matrix: Matrix, result: Quaternion): void {
    let data = matrix.m
    // tslint:disable:one-variable-per-declaration
    let m11 = data[0],
      m12 = data[4],
      m13 = data[8]
    let m21 = data[1],
      m22 = data[5],
      m23 = data[9]
    let m31 = data[2],
      m32 = data[6],
      m33 = data[10]
    // tslint:enable:one-variable-per-declaration
    let trace = m11 + m22 + m33
    let s

    if (trace > 0) {
      s = 0.5 / Math.sqrt(trace + 1.0)

      result.w = 0.25 / s
      result.x = (m32 - m23) * s
      result.y = (m13 - m31) * s
      result.z = (m21 - m12) * s
    } else if (m11 > m22 && m11 > m33) {
      s = 2.0 * Math.sqrt(1.0 + m11 - m22 - m33)

      result.w = (m32 - m23) / s
      result.x = 0.25 * s
      result.y = (m12 + m21) / s
      result.z = (m13 + m31) / s
    } else if (m22 > m33) {
      s = 2.0 * Math.sqrt(1.0 + m22 - m11 - m33)

      result.w = (m13 - m31) / s
      result.x = (m12 + m21) / s
      result.y = 0.25 * s
      result.z = (m23 + m32) / s
    } else {
      s = 2.0 * Math.sqrt(1.0 + m33 - m11 - m22)

      result.w = (m21 - m12) / s
      result.x = (m13 + m31) / s
      result.y = (m23 + m32) / s
      result.z = 0.25 * s
    }
  }

  /**
   * Returns the dot product (float) between the quaternions "left" and "right"
   * @param left - defines the left operand
   * @param right - defines the right operand
   * @returns the dot product
   */
  public static Dot(left: ReadOnlyQuaternion, right: ReadOnlyQuaternion): number {
    return left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w
  }

  /**
   * Checks if the two quaternions are close to each other
   * @param quat0 - defines the first quaternion to check
   * @param quat1 - defines the second quaternion to check
   * @returns true if the two quaternions are close to each other
   */
  public static AreClose(quat0: ReadOnlyQuaternion, quat1: ReadOnlyQuaternion): boolean {
    let dot = Quaternion.Dot(quat0, quat1)

    return dot >= 0
  }

  /**
   * Creates an empty quaternion
   * @returns a new quaternion set to (0.0, 0.0, 0.0)
   */
  public static Zero(): Quaternion {
    return new Quaternion(0.0, 0.0, 0.0, 0.0)
  }

  /**
   * Inverse a given quaternion
   * @param q - defines the source quaternion
   * @returns a new quaternion as the inverted current quaternion
   */
  public static Inverse(q: Quaternion): Quaternion {
    return new Quaternion(-q.x, -q.y, -q.z, q.w)
  }

  /**
   * Gets a boolean indicating if the given quaternion is identity
   * @param quaternion - defines the quaternion to check
   * @returns true if the quaternion is identity
   */
  public static IsIdentity(quaternion: ReadOnlyQuaternion): boolean {
    return quaternion && quaternion.x === 0 && quaternion.y === 0 && quaternion.z === 0 && quaternion.w === 1
  }

  /**
   * Creates a quaternion from a rotation around an axis
   * @param axis - defines the axis to use
   * @param angle - defines the angle to use (in Euler degrees)
   * @returns a new quaternion created from the given axis (Vector3) and angle in radians (float)
   */
  public static RotationAxis(axis: Vector3, angle: number): Quaternion {
    const angleRad = angle * DEG2RAD
    return Quaternion.RotationAxisToRef(axis, angleRad, new Quaternion())
  }

  /**
   * Creates a rotation around an axis and stores it into the given quaternion
   * @param axis - defines the axis to use
   * @param angle - defines the angle to use (in Euler degrees)
   * @param result - defines the target quaternion
   * @returns the target quaternion
   */
  public static RotationAxisToRef(axis: Vector3, angle: number, result: Quaternion): Quaternion {
    const angleRad = angle * DEG2RAD
    let sin = Math.sin(angleRad / 2)
    axis.normalize()
    result.w = Math.cos(angleRad / 2)
    result.x = axis.x * sin
    result.y = axis.y * sin
    result.z = axis.z * sin
    return result
  }

  /**
   * Creates a new quaternion from data stored into an array
   * @param array - defines the data source
   * @param offset - defines the offset in the source array where the data starts
   * @returns a new quaternion
   */
  public static FromArray(array: ArrayLike<number>, offset: number = 0): Quaternion {
    return new Quaternion(array[offset], array[offset + 1], array[offset + 2], array[offset + 3])
  }

  /**
   * Creates a new quaternion from a set of euler angles and stores it in the target quaternion
   */
  public static FromEulerAnglesRef(x: number, y: number, z: number, result: Quaternion): void {
    return Quaternion.RotationYawPitchRollToRef(y * DEG2RAD, x * DEG2RAD, z * DEG2RAD, result)
  }

  /**
   * Creates a new quaternion from the given Euler float angles (y, x, z)
   * @param yaw - defines the rotation around Y axis
   * @param pitch - defines the rotation around X axis
   * @param roll - defines the rotation around Z axis
   * @returns the new quaternion
   */
  public static RotationYawPitchRoll(yaw: number, pitch: number, roll: number): Quaternion {
    let q = new Quaternion()
    Quaternion.RotationYawPitchRollToRef(yaw, pitch, roll, q)
    return q
  }

  /**
   * Creates a new rotation from the given Euler float angles (y, x, z) and stores it in the target quaternion
   * @param yaw - defines the rotation around Y axis
   * @param pitch - defines the rotation around X axis
   * @param roll - defines the rotation around Z axis
   * @param result - defines the target quaternion
   */
  public static RotationYawPitchRollToRef(yaw: number, pitch: number, roll: number, result: Quaternion): void {
    // Implemented unity-based calculations from: https://stackoverflow.com/a/56055813

    let halfPitch = pitch * 0.5
    let halfYaw = yaw * 0.5
    let halfRoll = roll * 0.5

    const c1 = Math.cos(halfPitch)
    const c2 = Math.cos(halfYaw)
    const c3 = Math.cos(halfRoll)
    const s1 = Math.sin(halfPitch)
    const s2 = Math.sin(halfYaw)
    const s3 = Math.sin(halfRoll)

    result.x = c2 * s1 * c3 + s2 * c1 * s3
    result.y = s2 * c1 * c3 - c2 * s1 * s3
    result.z = c2 * c1 * s3 - s2 * s1 * c3
    result.w = c2 * c1 * c3 + s2 * s1 * s3
  }

  /**
   * Creates a new quaternion from the given Euler float angles expressed in z-x-z orientation
   * @param alpha - defines the rotation around first axis
   * @param beta - defines the rotation around second axis
   * @param gamma - defines the rotation around third axis
   * @returns the new quaternion
   */
  public static RotationAlphaBetaGamma(alpha: number, beta: number, gamma: number): Quaternion {
    let result = new Quaternion()
    Quaternion.RotationAlphaBetaGammaToRef(alpha, beta, gamma, result)
    return result
  }

  /**
   * Creates a new quaternion from the given Euler float angles expressed in z-x-z orientation and stores it in the target quaternion
   * @param alpha - defines the rotation around first axis
   * @param beta - defines the rotation around second axis
   * @param gamma - defines the rotation around third axis
   * @param result - defines the target quaternion
   */
  public static RotationAlphaBetaGammaToRef(alpha: number, beta: number, gamma: number, result: Quaternion): void {
    // Produces a quaternion from Euler angles in the z-x-z orientation
    let halfGammaPlusAlpha = (gamma + alpha) * 0.5
    let halfGammaMinusAlpha = (gamma - alpha) * 0.5
    let halfBeta = beta * 0.5

    result.x = Math.cos(halfGammaMinusAlpha) * Math.sin(halfBeta)
    result.y = Math.sin(halfGammaMinusAlpha) * Math.sin(halfBeta)
    result.z = Math.sin(halfGammaPlusAlpha) * Math.cos(halfBeta)
    result.w = Math.cos(halfGammaPlusAlpha) * Math.cos(halfBeta)
  }

  /**
   * Creates a new quaternion containing the rotation value to reach the target (axis1, axis2, axis3) orientation as a rotated XYZ system (axis1, axis2 and axis3 are normalized during this operation)
   * @param axis1 - defines the first axis
   * @param axis2 - defines the second axis
   * @param axis3 - defines the third axis
   * @returns the new quaternion
   */
  public static RotationQuaternionFromAxis(axis1: Vector3, axis2: Vector3, axis3: Vector3): Quaternion {
    let quat = new Quaternion(0.0, 0.0, 0.0, 0.0)
    Quaternion.RotationQuaternionFromAxisToRef(axis1, axis2, axis3, quat)
    return quat
  }

  /**
   * Creates a rotation value to reach the target (axis1, axis2, axis3) orientation as a rotated XYZ system (axis1, axis2 and axis3 are normalized during this operation) and stores it in the target quaternion
   * @param axis1 - defines the first axis
   * @param axis2 - defines the second axis
   * @param axis3 - defines the third axis
   * @param ref - defines the target quaternion
   */
  public static RotationQuaternionFromAxisToRef(axis1: Vector3, axis2: Vector3, axis3: Vector3, ref: Quaternion): void {
    let rotMat = MathTmp.Matrix[0]
    Matrix.FromXYZAxesToRef(axis1.normalize(), axis2.normalize(), axis3.normalize(), rotMat)
    Quaternion.FromRotationMatrixToRef(rotMat, ref)
  }

  /**
   * Interpolates between two quaternions
   * @param left - defines first quaternion
   * @param right - defines second quaternion
   * @param amount - defines the gradient to use
   * @returns the new interpolated quaternion
   */
  public static Slerp(left: ReadOnlyQuaternion, right: ReadOnlyQuaternion, amount: number): Quaternion {
    let result = Quaternion.Identity

    Quaternion.SlerpToRef(left, right, amount, result)

    return result
  }

  /**
   * Interpolates between two quaternions and stores it into a target quaternion
   * @param left - defines first quaternion
   * @param right - defines second quaternion
   * @param amount - defines the gradient to use
   * @param result - defines the target quaternion
   */
  public static SlerpToRef(
    left: ReadOnlyQuaternion,
    right: ReadOnlyQuaternion,
    amount: number,
    result: Quaternion
  ): void {
    let num2
    let num3
    let num4 = left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w
    let flag = false

    if (num4 < 0) {
      flag = true
      num4 = -num4
    }

    if (num4 > 0.999999) {
      num3 = 1 - amount
      num2 = flag ? -amount : amount
    } else {
      let num5 = Math.acos(num4)
      let num6 = 1.0 / Math.sin(num5)
      num3 = Math.sin((1.0 - amount) * num5) * num6
      num2 = flag ? -Math.sin(amount * num5) * num6 : Math.sin(amount * num5) * num6
    }

    result.x = num3 * left.x + num2 * right.x
    result.y = num3 * left.y + num2 * right.y
    result.z = num3 * left.z + num2 * right.z
    result.w = num3 * left.w + num2 * right.w
  }

  /**
   * Interpolate between two quaternions using Hermite interpolation
   * @param value1 - defines first quaternion
   * @param tangent1 - defines the incoming tangent
   * @param value2 - defines second quaternion
   * @param tangent2 - defines the outgoing tangent
   * @param amount - defines the target quaternion
   * @returns the new interpolated quaternion
   */
  public static Hermite(
    value1: ReadOnlyQuaternion,
    tangent1: ReadOnlyQuaternion,
    value2: ReadOnlyQuaternion,
    tangent2: ReadOnlyQuaternion,
    amount: number
  ): Quaternion {
    let squared = amount * amount
    let cubed = amount * squared
    let part1 = 2.0 * cubed - 3.0 * squared + 1.0
    let part2 = -2.0 * cubed + 3.0 * squared
    let part3 = cubed - 2.0 * squared + amount
    let part4 = cubed - squared

    let x = value1.x * part1 + value2.x * part2 + tangent1.x * part3 + tangent2.x * part4
    let y = value1.y * part1 + value2.y * part2 + tangent1.y * part3 + tangent2.y * part4
    let z = value1.z * part1 + value2.z * part2 + tangent1.z * part3 + tangent2.z * part4
    let w = value1.w * part1 + value2.w * part2 + tangent1.w * part3 + tangent2.w * part4
    return new Quaternion(x, y, z, w)
  }

  /**
   * Creates an identity quaternion
   * @returns - the identity quaternion
   */
  public static get Identity(): Quaternion {
    return new Quaternion(0.0, 0.0, 0.0, 1.0)
  }

  /**
   * Returns the angle in degrees between two rotations a and b.
   * @param quat1 - defines the first quaternion
   * @param quat2 - defines the second quaternion
   */
  public static Angle(quat1: ReadOnlyQuaternion, quat2: ReadOnlyQuaternion): number {
    const dot = Quaternion.Dot(quat1, quat2)
    return Math.acos(Math.min(Math.abs(dot), 1)) * 2 * RAD2DEG
  }

  /**
   * Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis.
   * @param x - the rotation on the x axis in euler degrees
   * @param y - the rotation on the y axis in euler degrees
   * @param z - the rotation on the z axis in euler degrees
   */
  public static Euler(x: number, y: number, z: number): Quaternion {
    return Quaternion.RotationYawPitchRoll(y * DEG2RAD, x * DEG2RAD, z * DEG2RAD)
  }

  /**
   * Creates a rotation with the specified forward and upwards directions.
   * @param forward - the direction to look in
   * @param up - the vector that defines in which direction up is
   */
  public static LookRotation(forward: Vector3, up: Vector3 = MathTmp.staticUp): Quaternion {
    const forwardNew = Vector3.Normalize(forward)
    const right: Vector3 = Vector3.Normalize(Vector3.Cross(up, forwardNew))
    const upNew = Vector3.Cross(forwardNew, right)
    let m00 = right.x
    let m01 = right.y
    let m02 = right.z
    let m10 = upNew.x
    let m11 = upNew.y
    let m12 = upNew.z
    let m20 = forwardNew.x
    let m21 = forwardNew.y
    let m22 = forwardNew.z

    const num8 = m00 + m11 + m22
    let quaternion = new Quaternion()

    if (num8 > 0) {
      let num = Math.sqrt(num8 + 1)
      quaternion.w = num * 0.5
      num = 0.5 / num
      quaternion.x = (m12 - m21) * num
      quaternion.y = (m20 - m02) * num
      quaternion.z = (m01 - m10) * num
      return quaternion
    }

    if (m00 >= m11 && m00 >= m22) {
      let num7 = Math.sqrt(1 + m00 - m11 - m22)
      let num4 = 0.5 / num7
      quaternion.x = 0.5 * num7
      quaternion.y = (m01 + m10) * num4
      quaternion.z = (m02 + m20) * num4
      quaternion.w = (m12 - m21) * num4
      return quaternion
    }

    if (m11 > m22) {
      let num6 = Math.sqrt(1 + m11 - m00 - m22)
      let num3 = 0.5 / num6
      quaternion.x = (m10 + m01) * num3
      quaternion.y = 0.5 * num6
      quaternion.z = (m21 + m12) * num3
      quaternion.w = (m20 - m02) * num3
      return quaternion
    }

    let num5 = Math.sqrt(1 + m22 - m00 - m11)
    let num2 = 0.5 / num5
    quaternion.x = (m20 + m02) * num2
    quaternion.y = (m21 + m12) * num2
    quaternion.z = 0.5 * num5
    quaternion.w = (m01 - m10) * num2
    return quaternion
  }

  /**
   * The from quaternion is rotated towards to by an angular step of maxDegreesDelta.
   * @param from - defines the first quaternion
   * @param to - defines the second quaternion
   * @param maxDegreesDelta - the interval step
   */
  public static RotateTowards(from: ReadOnlyQuaternion, to: Quaternion, maxDegreesDelta: number): Quaternion {
    const num: number = Quaternion.Angle(from, to)
    if (num === 0) {
      return to
    }
    const t: number = Math.min(1, maxDegreesDelta / num)

    return Quaternion.Slerp(from, to, t)
  }

  /**
   * Creates a rotation which rotates from fromDirection to toDirection.
   * @param from - defines the first direction Vector
   * @param to - defines the target direction Vector
   */
  public static FromToRotation(from: Vector3, to: Vector3, up: Vector3 = MathTmp.staticUp): Quaternion {
    // Unity-based calculations implemented from https://forum.unity.com/threads/quaternion-lookrotation-around-an-axis.608470/#post-4069888

    let v0 = from.normalize()
    let v1 = to.normalize()

    const a = Vector3.Cross(v0, v1)
    const w = Math.sqrt(v0.lengthSquared() * v1.lengthSquared()) + Vector3.Dot(v0, v1)
    if (a.lengthSquared() < 0.0001) {
      // the vectors are parallel, check w to find direction
      // if w is 0 then values are opposite, and we sould rotate 180 degrees around the supplied axis
      // otherwise the vectors in the same direction and no rotation should occur
      return (Math.abs(w) < 0.0001) ? new Quaternion(up.x, up.y, up.z, 0).normalized : Quaternion.Identity
    } else {
      return new Quaternion(a.x, a.y, a.z, w).normalized
    }
  }

  /**
   * Converts this quaternion to one with the same orientation but with a magnitude of 1.
   */
  public get normalized() {
    return this.normalize()
  }

  /**
   * Creates a rotation which rotates from fromDirection to toDirection.
   * @param from - defines the first Vector
   * @param to - defines the second Vector
   * @param up - defines the direction
   */
  public setFromToRotation(from: Vector3, to: Vector3, up: Vector3 = MathTmp.staticUp) {
    const result = Quaternion.FromToRotation(from, to, up)
    this.x = result.x
    this.y = result.y
    this.z = result.z
    this.w = result.w
  }

  /**
   * Sets the euler angle representation of the rotation.
   */
  public set eulerAngles(euler: Vector3) {
    this.setEuler(euler.x, euler.y, euler.z)
  }

  /**
   * Gets the euler angle representation of the rotation.
   * Implemented unity-based calculations from: https://stackoverflow.com/a/56055813
   */
  public get eulerAngles() {
    const out = new Vector3()

    // if the input quaternion is normalized, this is exactly one. Otherwise, this acts as a correction factor for the quaternion's not-normalizedness
    const unit = (this.x * this.x) + (this.y * this.y) + (this.z * this.z) + (this.w * this.w)

    // this will have a magnitude of 0.5 or greater if and only if this is a singularity case
    const test = this.x * this.w - this.y * this.z

    if (test > 0.4995 * unit) { // singularity at north pole
      out.x = Math.PI / 2
      out.y = 2 * Math.atan2(this.y, this.x)
      out.z = 0
    } else if (test < -0.4995 * unit) { // singularity at south pole
      out.x = -Math.PI / 2
      out.y = -2 * Math.atan2(this.y, this.x)
      out.z = 0
    } else { // no singularity - this is the majority of cases
      out.x = Math.asin(2 * (this.w * this.x - this.y * this.z))
      out.y = Math.atan2(2 * this.w * this.y + 2 * this.z * this.x, 1 - 2 * (this.x * this.x + this.y * this.y))
      out.z = Math.atan2(2 * this.w * this.z + 2 * this.x * this.y, 1 - 2 * (this.z * this.z + this.x * this.x))
    }
    out.x *= RAD2DEG
    out.y *= RAD2DEG
    out.z *= RAD2DEG

    // ensure the degree values are between 0 and 360
    out.x = Scalar.Repeat(out.x, 360)
    out.y = Scalar.Repeat(out.y, 360)
    out.z = Scalar.Repeat(out.z, 360)

    return out
  }

  /**
   * Gets a string representation for the current quaternion
   * @returns a string with the Quaternion coordinates
   */
  public toString(): string {
    return `(${this.x}, ${this.y}, ${this.z}, ${this.w})`
  }

  /**
   * Gets length of current quaternion
   * @returns the quaternion length (float)
   */
  public get length(): number {
    return Math.sqrt(this.lengthSquared)
  }

  /**
   * Gets length of current quaternion
   * @returns the quaternion length (float)
   */
  public get lengthSquared(): number {
    return this.x * this.x + this.y * this.y + this.z * this.z + this.w * this.w
  }

  /**
   * Gets the class name of the quaternion
   * @returns the string "Quaternion"
   */
  public getClassName(): string {
    return 'Quaternion'
  }

  /**
   * Gets a hash code for this quaternion
   * @returns the quaternion hash code
   */
  public getHashCode(): number {
    let hash = this.x || 0
    hash = (hash * 397) ^ (this.y || 0)
    hash = (hash * 397) ^ (this.z || 0)
    hash = (hash * 397) ^ (this.w || 0)
    return hash
  }

  /**
   * Copy the quaternion to an array
   * @returns a new array populated with 4 elements from the quaternion coordinates
   */
  public asArray(): number[] {
    return [this.x, this.y, this.z, this.w]
  }
  /**
   * Check if two quaternions are equals
   * @param otherQuaternion - defines the second operand
   * @returns true if the current quaternion and the given one coordinates are strictly equals
   */
  public equals(otherQuaternion: ReadOnlyQuaternion): boolean {
    return (
      otherQuaternion &&
      this.x === otherQuaternion.x &&
      this.y === otherQuaternion.y &&
      this.z === otherQuaternion.z &&
      this.w === otherQuaternion.w
    )
  }

  /**
   * Clone the current quaternion
   * @returns a new quaternion copied from the current one
   */
  public clone(): Quaternion {
    return new Quaternion(this.x, this.y, this.z, this.w)
  }

  /**
   * Copy a quaternion to the current one
   * @param other - defines the other quaternion
   * @returns the updated current quaternion
   */
  public copyFrom(other: ReadOnlyQuaternion): Quaternion {
    this.x = other.x
    this.y = other.y
    this.z = other.z
    this.w = other.w
    return this
  }

  /**
   * Updates the current quaternion with the given float coordinates
   * @param x - defines the x coordinate
   * @param y - defines the y coordinate
   * @param z - defines the z coordinate
   * @param w - defines the w coordinate
   * @returns the updated current quaternion
   */
  public copyFromFloats(x: number, y: number, z: number, w: number): Quaternion {
    this.x = x
    this.y = y
    this.z = z
    this.w = w
    return this
  }

  /**
   * Updates the current quaternion from the given float coordinates
   * @param x - defines the x coordinate
   * @param y - defines the y coordinate
   * @param z - defines the z coordinate
   * @param w - defines the w coordinate
   * @returns the updated current quaternion
   */
  public set(x: number, y: number, z: number, w: number): Quaternion {
    return this.copyFromFloats(x, y, z, w)
  }

  /**
   * Updates the current quaternion from the given euler angles
   * @returns the updated current quaternion
   */
  public setEuler(x: number, y: number, z: number): Quaternion {
    Quaternion.RotationYawPitchRollToRef(y * DEG2RAD, x * DEG2RAD, z * DEG2RAD, this)
    return this
  }

  /**
   * @internal
   * Adds two quaternions
   * @param other - defines the second operand
   * @returns a new quaternion as the addition result of the given one and the current quaternion
   */
  public add(other: Quaternion): Quaternion {
    return new Quaternion(this.x + other.x, this.y + other.y, this.z + other.z, this.w + other.w)
  }

  /**
   * @internal
   * Add a quaternion to the current one
   * @param other - defines the quaternion to add
   * @returns the current quaternion
   */
  public addInPlace(other: Quaternion): Quaternion {
    this.x += other.x
    this.y += other.y
    this.z += other.z
    this.w += other.w
    return this
  }
  /**
   * Subtract two quaternions
   * @param other - defines the second operand
   * @returns a new quaternion as the subtraction result of the given one from the current one
   */
  public subtract(other: Quaternion): Quaternion {
    return new Quaternion(this.x - other.x, this.y - other.y, this.z - other.z, this.w - other.w)
  }

  /**
   * Multiplies the current quaternion by a scale factor
   * @param value - defines the scale factor
   * @returns a new quaternion set by multiplying the current quaternion coordinates by the float "scale"
   */
  public scale(value: number): Quaternion {
    return new Quaternion(this.x * value, this.y * value, this.z * value, this.w * value)
  }

  /**
   * Scale the current quaternion values by a factor and stores the result to a given quaternion
   * @param scale - defines the scale factor
   * @param result - defines the Quaternion object where to store the result
   * @returns the unmodified current quaternion
   */
  public scaleToRef(scale: number, result: Quaternion): Quaternion {
    result.x = this.x * scale
    result.y = this.y * scale
    result.z = this.z * scale
    result.w = this.w * scale
    return this
  }

  /**
   * Multiplies in place the current quaternion by a scale factor
   * @param value - defines the scale factor
   * @returns the current modified quaternion
   */
  public scaleInPlace(value: number): Quaternion {
    this.x *= value
    this.y *= value
    this.z *= value
    this.w *= value

    return this
  }

  /**
   * Scale the current quaternion values by a factor and add the result to a given quaternion
   * @param scale - defines the scale factor
   * @param result - defines the Quaternion object where to store the result
   * @returns the unmodified current quaternion
   */
  public scaleAndAddToRef(scale: number, result: Quaternion): Quaternion {
    result.x += this.x * scale
    result.y += this.y * scale
    result.z += this.z * scale
    result.w += this.w * scale
    return this
  }

  /**
   * Multiplies two quaternions
   * @param q1 - defines the second operand
   * @returns a new quaternion set as the multiplication result of the current one with the given one "q1"
   */
  public multiply(q1: ReadOnlyQuaternion): Quaternion {
    let result = new Quaternion(0, 0, 0, 1.0)
    this.multiplyToRef(q1, result)
    return result
  }

  /**
   * Sets the given "result" as the the multiplication result of the current one with the given one "q1"
   * @param q1 - defines the second operand
   * @param result - defines the target quaternion
   * @returns the current quaternion
   */
  public multiplyToRef(q1: ReadOnlyQuaternion, result: Quaternion): Quaternion {
    let x = this.x * q1.w + this.y * q1.z - this.z * q1.y + this.w * q1.x
    let y = -this.x * q1.z + this.y * q1.w + this.z * q1.x + this.w * q1.y
    let z = this.x * q1.y - this.y * q1.x + this.z * q1.w + this.w * q1.z
    let w = -this.x * q1.x - this.y * q1.y - this.z * q1.z + this.w * q1.w
    result.copyFromFloats(x, y, z, w)
    return this
  }

  /**
   * Updates the current quaternion with the multiplication of itself with the given one "q1"
   * @param q1 - defines the second operand
   * @returns the currentupdated quaternion
   */
  public multiplyInPlace(q1: ReadOnlyQuaternion): Quaternion {
    this.multiplyToRef(q1, this)
    return this
  }

  /**
   * Conjugates (1-q) the current quaternion and stores the result in the given quaternion
   * @param ref - defines the target quaternion
   * @returns the current quaternion
   */
  public conjugateToRef(ref: Quaternion): Quaternion {
    ref.copyFromFloats(-this.x, -this.y, -this.z, this.w)
    return this
  }

  /**
   * Conjugates in place (1-q) the current quaternion
   * @returns the current updated quaternion
   */
  public conjugateInPlace(): Quaternion {
    this.x *= -1
    this.y *= -1
    this.z *= -1
    return this
  }

  /**
   * Conjugates in place (1-q) the current quaternion
   * @returns a new quaternion
   */
  public conjugate(): Quaternion {
    let result = new Quaternion(-this.x, -this.y, -this.z, this.w)
    return result
  }

  /**
   * Normalize in place the current quaternion
   * @returns the current updated quaternion
   */
  public normalize(): Quaternion {
    let length = 1.0 / this.length
    this.x *= length
    this.y *= length
    this.z *= length
    this.w *= length
    return this
  }

  public angleAxis(degress: number, axis: Vector3) {
    if (axis.lengthSquared() === 0) {
      return Quaternion.Identity
    }

    const result: Quaternion = Quaternion.Identity
    let radians = degress * DEG2RAD
    radians *= 0.5

    let a2 = axis.normalize()
    a2 = axis.scaleInPlace(Math.sin(radians))

    result.x = a2.x
    result.y = a2.y
    result.z = a2.z
    result.w = Math.cos(radians)

    return result.normalize()
  }

  /**
   * Updates the given rotation matrix with the current quaternion values
   * @param result - defines the target matrix
   * @returns the current unchanged quaternion
   */
  public toRotationMatrix(result: Matrix): Quaternion {
    Matrix.FromQuaternionToRef(this, result)
    return this
  }

  /**
   * Updates the current quaternion from the given rotation matrix values
   * @param matrix - defines the source matrix
   * @returns the current updated quaternion
   */
  public fromRotationMatrix(matrix: Matrix): Quaternion {
    Quaternion.FromRotationMatrixToRef(matrix, this)
    return this
  }
}
