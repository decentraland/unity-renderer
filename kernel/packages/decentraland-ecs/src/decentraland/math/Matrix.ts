import { FloatArray, Nullable } from './types'
import { Vector3 } from './Vector3'
import { Quaternion } from './Quaternion'
import { MathTmp } from './preallocatedVariables'
import { Plane } from './Plane'
import { Vector4 } from './Vector4'

/**
 * Class used to store matrix data (4x4)
 * @public
 */
export class Matrix {
  /**
   * Gets the internal data of the matrix
   */
  public get m(): Readonly<FloatArray> {
    return this._m
  }

  /**
   * Gets an identity matrix that must not be updated
   */
  public static get IdentityReadOnly(): Readonly<Matrix> {
    return Matrix._identityReadOnly
  }

  private static _updateFlagSeed = 0
  private static _identityReadOnly = Matrix.Identity()

  /**
   * Gets the update flag of the matrix which is an unique number for the matrix.
   * It will be incremented every time the matrix data change.
   * You can use it to speed the comparison between two versions of the same matrix.
   */
  public updateFlag!: number

  private _isIdentity = false
  private _isIdentityDirty = true
  private _isIdentity3x2 = true
  private _isIdentity3x2Dirty = true

  private readonly _m: FloatArray = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]

  /**
   * Creates an empty matrix (filled with zeros)
   */
  public constructor() {
    this._updateIdentityStatus(false)
  }

  // Statics

  /**
   * Creates a matrix from an array
   * @param array - defines the source array
   * @param offset - defines an offset in the source array
   * @returns a new Matrix set from the starting index of the given array
   */
  public static FromArray(array: ArrayLike<number>, offset: number = 0): Matrix {
    let result = new Matrix()
    Matrix.FromArrayToRef(array, offset, result)
    return result
  }

  /**
   * Copy the content of an array into a given matrix
   * @param array - defines the source array
   * @param offset - defines an offset in the source array
   * @param result - defines the target matrix
   */
  public static FromArrayToRef(array: ArrayLike<number>, offset: number, result: Matrix) {
    for (let index = 0; index < 16; index++) {
      result._m[index] = array[index + offset]
    }
    result._markAsUpdated()
  }

  /**
   * Stores an array into a matrix after having multiplied each component by a given factor
   * @param array - defines the source array
   * @param offset - defines the offset in the source array
   * @param scale - defines the scaling factor
   * @param result - defines the target matrix
   */
  public static FromFloatArrayToRefScaled(array: FloatArray, offset: number, scale: number, result: Matrix) {
    for (let index = 0; index < 16; index++) {
      result._m[index] = array[index + offset] * scale
    }
    result._markAsUpdated()
  }

  /**
   * Stores a list of values (16) inside a given matrix
   * @param initialM11 - defines 1st value of 1st row
   * @param initialM12 - defines 2nd value of 1st row
   * @param initialM13 - defines 3rd value of 1st row
   * @param initialM14 - defines 4th value of 1st row
   * @param initialM21 - defines 1st value of 2nd row
   * @param initialM22 - defines 2nd value of 2nd row
   * @param initialM23 - defines 3rd value of 2nd row
   * @param initialM24 - defines 4th value of 2nd row
   * @param initialM31 - defines 1st value of 3rd row
   * @param initialM32 - defines 2nd value of 3rd row
   * @param initialM33 - defines 3rd value of 3rd row
   * @param initialM34 - defines 4th value of 3rd row
   * @param initialM41 - defines 1st value of 4th row
   * @param initialM42 - defines 2nd value of 4th row
   * @param initialM43 - defines 3rd value of 4th row
   * @param initialM44 - defines 4th value of 4th row
   * @param result - defines the target matrix
   */
  public static FromValuesToRef(
    initialM11: number,
    initialM12: number,
    initialM13: number,
    initialM14: number,
    initialM21: number,
    initialM22: number,
    initialM23: number,
    initialM24: number,
    initialM31: number,
    initialM32: number,
    initialM33: number,
    initialM34: number,
    initialM41: number,
    initialM42: number,
    initialM43: number,
    initialM44: number,
    result: Matrix
  ): void {
    const m = result._m
    m[0] = initialM11
    m[1] = initialM12
    m[2] = initialM13
    m[3] = initialM14
    m[4] = initialM21
    m[5] = initialM22
    m[6] = initialM23
    m[7] = initialM24
    m[8] = initialM31
    m[9] = initialM32
    m[10] = initialM33
    m[11] = initialM34
    m[12] = initialM41
    m[13] = initialM42
    m[14] = initialM43
    m[15] = initialM44

    result._markAsUpdated()
  }

  /**
   * Creates new matrix from a list of values (16)
   * @param initialM11 - defines 1st value of 1st row
   * @param initialM12 - defines 2nd value of 1st row
   * @param initialM13 - defines 3rd value of 1st row
   * @param initialM14 - defines 4th value of 1st row
   * @param initialM21 - defines 1st value of 2nd row
   * @param initialM22 - defines 2nd value of 2nd row
   * @param initialM23 - defines 3rd value of 2nd row
   * @param initialM24 - defines 4th value of 2nd row
   * @param initialM31 - defines 1st value of 3rd row
   * @param initialM32 - defines 2nd value of 3rd row
   * @param initialM33 - defines 3rd value of 3rd row
   * @param initialM34 - defines 4th value of 3rd row
   * @param initialM41 - defines 1st value of 4th row
   * @param initialM42 - defines 2nd value of 4th row
   * @param initialM43 - defines 3rd value of 4th row
   * @param initialM44 - defines 4th value of 4th row
   * @returns the new matrix
   */
  public static FromValues(
    initialM11: number,
    initialM12: number,
    initialM13: number,
    initialM14: number,
    initialM21: number,
    initialM22: number,
    initialM23: number,
    initialM24: number,
    initialM31: number,
    initialM32: number,
    initialM33: number,
    initialM34: number,
    initialM41: number,
    initialM42: number,
    initialM43: number,
    initialM44: number
  ): Matrix {
    let result = new Matrix()
    const m = result._m
    m[0] = initialM11
    m[1] = initialM12
    m[2] = initialM13
    m[3] = initialM14
    m[4] = initialM21
    m[5] = initialM22
    m[6] = initialM23
    m[7] = initialM24
    m[8] = initialM31
    m[9] = initialM32
    m[10] = initialM33
    m[11] = initialM34
    m[12] = initialM41
    m[13] = initialM42
    m[14] = initialM43
    m[15] = initialM44
    result._markAsUpdated()
    return result
  }

  /**
   * Creates a new matrix composed by merging scale (vector3), rotation (quaternion) and translation (vector3)
   * @param scale - defines the scale vector3
   * @param rotation - defines the rotation quaternion
   * @param translation - defines the translation vector3
   * @returns a new matrix
   */
  public static Compose(scale: Vector3, rotation: Quaternion, translation: Vector3): Matrix {
    let result = new Matrix()
    Matrix.ComposeToRef(scale, rotation, translation, result)
    return result
  }

  /**
   * Sets a matrix to a value composed by merging scale (vector3), rotation (quaternion) and translation (vector3)
   * @param scale - defines the scale vector3
   * @param rotation - defines the rotation quaternion
   * @param translation - defines the translation vector3
   * @param result - defines the target matrix
   */
  public static ComposeToRef(scale: Vector3, rotation: Quaternion, translation: Vector3, result: Matrix): void {
    Matrix.ScalingToRef(scale.x, scale.y, scale.z, MathTmp.Matrix[1])
    rotation.toRotationMatrix(MathTmp.Matrix[0])
    MathTmp.Matrix[1].multiplyToRef(MathTmp.Matrix[0], result)

    result.setTranslation(translation)
  }

  /**
   * Creates a new identity matrix
   * @returns a new identity matrix
   */
  public static Identity(): Matrix {
    const identity = Matrix.FromValues(1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0)
    identity._updateIdentityStatus(true)
    return identity
  }

  /**
   * Creates a new identity matrix and stores the result in a given matrix
   * @param result - defines the target matrix
   */
  public static IdentityToRef(result: Matrix): void {
    Matrix.FromValuesToRef(1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, result)
    result._updateIdentityStatus(true)
  }

  /**
   * Creates a new zero matrix
   * @returns a new zero matrix
   */
  public static Zero(): Matrix {
    const zero = Matrix.FromValues(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)
    zero._updateIdentityStatus(false)
    return zero
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the X axis
   * @param angle - defines the angle (in radians) to use
   * @returns the new matrix
   */
  public static RotationX(angle: number): Matrix {
    let result = new Matrix()
    Matrix.RotationXToRef(angle, result)
    return result
  }

  /**
   * Creates a new matrix as the invert of a given matrix
   * @param source - defines the source matrix
   * @returns the new matrix
   */
  public static Invert(source: Matrix): Matrix {
    let result = new Matrix()
    source.invertToRef(result)
    return result
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the X axis and stores it in a given matrix
   * @param angle - defines the angle (in radians) to use
   * @param result - defines the target matrix
   */
  public static RotationXToRef(angle: number, result: Matrix): void {
    let s = Math.sin(angle)
    let c = Math.cos(angle)
    Matrix.FromValuesToRef(1.0, 0.0, 0.0, 0.0, 0.0, c, s, 0.0, 0.0, -s, c, 0.0, 0.0, 0.0, 0.0, 1.0, result)

    result._updateIdentityStatus(c === 1 && s === 0)
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the Y axis
   * @param angle - defines the angle (in radians) to use
   * @returns the new matrix
   */
  public static RotationY(angle: number): Matrix {
    let result = new Matrix()
    Matrix.RotationYToRef(angle, result)
    return result
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the Y axis and stores it in a given matrix
   * @param angle - defines the angle (in radians) to use
   * @param result - defines the target matrix
   */
  public static RotationYToRef(angle: number, result: Matrix): void {
    let s = Math.sin(angle)
    let c = Math.cos(angle)
    Matrix.FromValuesToRef(c, 0.0, -s, 0.0, 0.0, 1.0, 0.0, 0.0, s, 0.0, c, 0.0, 0.0, 0.0, 0.0, 1.0, result)

    result._updateIdentityStatus(c === 1 && s === 0)
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the Z axis
   * @param angle - defines the angle (in radians) to use
   * @returns the new matrix
   */
  public static RotationZ(angle: number): Matrix {
    let result = new Matrix()
    Matrix.RotationZToRef(angle, result)
    return result
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the Z axis and stores it in a given matrix
   * @param angle - defines the angle (in radians) to use
   * @param result - defines the target matrix
   */
  public static RotationZToRef(angle: number, result: Matrix): void {
    let s = Math.sin(angle)
    let c = Math.cos(angle)
    Matrix.FromValuesToRef(c, s, 0.0, 0.0, -s, c, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, result)

    result._updateIdentityStatus(c === 1 && s === 0)
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the given axis
   * @param axis - defines the axis to use
   * @param angle - defines the angle (in radians) to use
   * @returns the new matrix
   */
  public static RotationAxis(axis: Vector3, angle: number): Matrix {
    let result = new Matrix()
    Matrix.RotationAxisToRef(axis, angle, result)
    return result
  }

  /**
   * Creates a new rotation matrix for "angle" radians around the given axis and stores it in a given matrix
   * @param axis - defines the axis to use
   * @param angle - defines the angle (in radians) to use
   * @param result - defines the target matrix
   */
  public static RotationAxisToRef(axis: Vector3, angle: number, result: Matrix): void {
    let s = Math.sin(-angle)
    let c = Math.cos(-angle)
    let c1 = 1 - c

    axis.normalize()
    const m = result._m
    m[0] = axis.x * axis.x * c1 + c
    m[1] = axis.x * axis.y * c1 - axis.z * s
    m[2] = axis.x * axis.z * c1 + axis.y * s
    m[3] = 0.0

    m[4] = axis.y * axis.x * c1 + axis.z * s
    m[5] = axis.y * axis.y * c1 + c
    m[6] = axis.y * axis.z * c1 - axis.x * s
    m[7] = 0.0

    m[8] = axis.z * axis.x * c1 - axis.y * s
    m[9] = axis.z * axis.y * c1 + axis.x * s
    m[10] = axis.z * axis.z * c1 + c
    m[11] = 0.0

    m[12] = 0.0
    m[13] = 0.0
    m[14] = 0.0
    m[15] = 1.0

    result._markAsUpdated()
  }

  /**
   * Creates a rotation matrix
   * @param yaw - defines the yaw angle in radians (Y axis)
   * @param pitch - defines the pitch angle in radians (X axis)
   * @param roll - defines the roll angle in radians (X axis)
   * @returns the new rotation matrix
   */
  public static RotationYawPitchRoll(yaw: number, pitch: number, roll: number): Matrix {
    let result = new Matrix()
    Matrix.RotationYawPitchRollToRef(yaw, pitch, roll, result)
    return result
  }

  /**
   * Creates a rotation matrix and stores it in a given matrix
   * @param yaw - defines the yaw angle in radians (Y axis)
   * @param pitch - defines the pitch angle in radians (X axis)
   * @param roll - defines the roll angle in radians (X axis)
   * @param result - defines the target matrix
   */
  public static RotationYawPitchRollToRef(yaw: number, pitch: number, roll: number, result: Matrix): void {
    Quaternion.RotationYawPitchRollToRef(yaw, pitch, roll, MathTmp.Quaternion[0])
    MathTmp.Quaternion[0].toRotationMatrix(result)
  }

  /**
   * Creates a scaling matrix
   * @param x - defines the scale factor on X axis
   * @param y - defines the scale factor on Y axis
   * @param z - defines the scale factor on Z axis
   * @returns the new matrix
   */
  public static Scaling(x: number, y: number, z: number): Matrix {
    let result = new Matrix()
    Matrix.ScalingToRef(x, y, z, result)
    return result
  }

  /**
   * Creates a scaling matrix and stores it in a given matrix
   * @param x - defines the scale factor on X axis
   * @param y - defines the scale factor on Y axis
   * @param z - defines the scale factor on Z axis
   * @param result - defines the target matrix
   */
  public static ScalingToRef(x: number, y: number, z: number, result: Matrix): void {
    Matrix.FromValuesToRef(x, 0.0, 0.0, 0.0, 0.0, y, 0.0, 0.0, 0.0, 0.0, z, 0.0, 0.0, 0.0, 0.0, 1.0, result)

    result._updateIdentityStatus(x === 1 && y === 1 && z === 1)
  }

  /**
   * Creates a translation matrix
   * @param x - defines the translation on X axis
   * @param y - defines the translation on Y axis
   * @param z - defines the translationon Z axis
   * @returns the new matrix
   */
  public static Translation(x: number, y: number, z: number): Matrix {
    let result = new Matrix()
    Matrix.TranslationToRef(x, y, z, result)
    return result
  }

  /**
   * Creates a translation matrix and stores it in a given matrix
   * @param x - defines the translation on X axis
   * @param y - defines the translation on Y axis
   * @param z - defines the translationon Z axis
   * @param result - defines the target matrix
   */
  public static TranslationToRef(x: number, y: number, z: number, result: Matrix): void {
    Matrix.FromValuesToRef(1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, x, y, z, 1.0, result)
    result._updateIdentityStatus(x === 0 && y === 0 && z === 0)
  }

  /**
   * Returns a new Matrix whose values are the interpolated values for "gradient" (float) between the ones of the matrices "startValue" and "endValue".
   * @param startValue - defines the start value
   * @param endValue - defines the end value
   * @param gradient - defines the gradient factor
   * @returns the new matrix
   */
  public static Lerp(startValue: Matrix, endValue: Matrix, gradient: number): Matrix {
    let result = new Matrix()
    Matrix.LerpToRef(startValue, endValue, gradient, result)
    return result
  }

  /**
   * Set the given matrix "result" as the interpolated values for "gradient" (float) between the ones of the matrices "startValue" and "endValue".
   * @param startValue - defines the start value
   * @param endValue - defines the end value
   * @param gradient - defines the gradient factor
   * @param result - defines the Matrix object where to store data
   */
  public static LerpToRef(startValue: Matrix, endValue: Matrix, gradient: number, result: Matrix): void {
    for (let index = 0; index < 16; index++) {
      result._m[index] = startValue._m[index] * (1.0 - gradient) + endValue._m[index] * gradient
    }
    result._markAsUpdated()
  }

  /**
   * Builds a new matrix whose values are computed by:
   * * decomposing the the "startValue" and "endValue" matrices into their respective scale, rotation and translation matrices
   * * interpolating for "gradient" (float) the values between each of these decomposed matrices between the start and the end
   * * recomposing a new matrix from these 3 interpolated scale, rotation and translation matrices
   * @param startValue - defines the first matrix
   * @param endValue - defines the second matrix
   * @param gradient - defines the gradient between the two matrices
   * @returns the new matrix
   */
  public static DecomposeLerp(startValue: Matrix, endValue: Matrix, gradient: number): Matrix {
    let result = new Matrix()
    Matrix.DecomposeLerpToRef(startValue, endValue, gradient, result)
    return result
  }

  /**
   * Update a matrix to values which are computed by:
   * * decomposing the the "startValue" and "endValue" matrices into their respective scale, rotation and translation matrices
   * * interpolating for "gradient" (float) the values between each of these decomposed matrices between the start and the end
   * * recomposing a new matrix from these 3 interpolated scale, rotation and translation matrices
   * @param startValue - defines the first matrix
   * @param endValue - defines the second matrix
   * @param gradient - defines the gradient between the two matrices
   * @param result - defines the target matrix
   */
  public static DecomposeLerpToRef(startValue: Matrix, endValue: Matrix, gradient: number, result: Matrix) {
    let startScale = MathTmp.Vector3[0]
    let startRotation = MathTmp.Quaternion[0]
    let startTranslation = MathTmp.Vector3[1]
    startValue.decompose(startScale, startRotation, startTranslation)

    let endScale = MathTmp.Vector3[2]
    let endRotation = MathTmp.Quaternion[1]
    let endTranslation = MathTmp.Vector3[3]
    endValue.decompose(endScale, endRotation, endTranslation)

    let resultScale = MathTmp.Vector3[4]
    Vector3.LerpToRef(startScale, endScale, gradient, resultScale)
    let resultRotation = MathTmp.Quaternion[2]
    Quaternion.SlerpToRef(startRotation, endRotation, gradient, resultRotation)

    let resultTranslation = MathTmp.Vector3[5]
    Vector3.LerpToRef(startTranslation, endTranslation, gradient, resultTranslation)

    Matrix.ComposeToRef(resultScale, resultRotation, resultTranslation, result)
  }

  /**
   * Gets a new rotation matrix used to rotate an entity so as it looks at the target vector3, from the eye vector3 position, the up vector3 being oriented like "up"
   * This function works in left handed mode
   * @param eye - defines the final position of the entity
   * @param target - defines where the entity should look at
   * @param up - defines the up vector for the entity
   * @returns the new matrix
   */
  public static LookAtLH(eye: Vector3, target: Vector3, up: Vector3): Matrix {
    let result = new Matrix()
    Matrix.LookAtLHToRef(eye, target, up, result)
    return result
  }

  /**
   * Sets the given "result" Matrix to a rotation matrix used to rotate an entity so that it looks at the target vector3, from the eye vector3 position, the up vector3 being oriented like "up".
   * This function works in left handed mode
   * @param eye - defines the final position of the entity
   * @param target - defines where the entity should look at
   * @param up - defines the up vector for the entity
   * @param result - defines the target matrix
   */
  public static LookAtLHToRef(eye: Vector3, target: Vector3, up: Vector3, result: Matrix): void {
    const xAxis = MathTmp.Vector3[0]
    const yAxis = MathTmp.Vector3[1]
    const zAxis = MathTmp.Vector3[2]

    // Z axis
    target.subtractToRef(eye, zAxis)
    zAxis.normalize()

    // X axis
    Vector3.CrossToRef(up, zAxis, xAxis)

    const xSquareLength = xAxis.lengthSquared()
    if (xSquareLength === 0) {
      xAxis.x = 1.0
    } else {
      xAxis.normalizeFromLength(Math.sqrt(xSquareLength))
    }

    // Y axis
    Vector3.CrossToRef(zAxis, xAxis, yAxis)
    yAxis.normalize()

    // Eye angles
    let ex = -Vector3.Dot(xAxis, eye)
    let ey = -Vector3.Dot(yAxis, eye)
    let ez = -Vector3.Dot(zAxis, eye)

    Matrix.FromValuesToRef(
      xAxis.x,
      yAxis.x,
      zAxis.x,
      0.0,
      xAxis.y,
      yAxis.y,
      zAxis.y,
      0.0,
      xAxis.z,
      yAxis.z,
      zAxis.z,
      0.0,
      ex,
      ey,
      ez,
      1.0,
      result
    )
  }

  /**
   * Gets a new rotation matrix used to rotate an entity so as it looks at the target vector3, from the eye vector3 position, the up vector3 being oriented like "up"
   * This function works in right handed mode
   * @param eye - defines the final position of the entity
   * @param target - defines where the entity should look at
   * @param up - defines the up vector for the entity
   * @returns the new matrix
   */
  public static LookAtRH(eye: Vector3, target: Vector3, up: Vector3): Matrix {
    let result = new Matrix()
    Matrix.LookAtRHToRef(eye, target, up, result)
    return result
  }

  /**
   * Sets the given "result" Matrix to a rotation matrix used to rotate an entity so that it looks at the target vector3, from the eye vector3 position, the up vector3 being oriented like "up".
   * This function works in right handed mode
   * @param eye - defines the final position of the entity
   * @param target - defines where the entity should look at
   * @param up - defines the up vector for the entity
   * @param result - defines the target matrix
   */
  public static LookAtRHToRef(eye: Vector3, target: Vector3, up: Vector3, result: Matrix): void {
    const xAxis = MathTmp.Vector3[0]
    const yAxis = MathTmp.Vector3[1]
    const zAxis = MathTmp.Vector3[2]

    // Z axis
    eye.subtractToRef(target, zAxis)
    zAxis.normalize()

    // X axis
    Vector3.CrossToRef(up, zAxis, xAxis)

    const xSquareLength = xAxis.lengthSquared()
    if (xSquareLength === 0) {
      xAxis.x = 1.0
    } else {
      xAxis.normalizeFromLength(Math.sqrt(xSquareLength))
    }

    // Y axis
    Vector3.CrossToRef(zAxis, xAxis, yAxis)
    yAxis.normalize()

    // Eye angles
    let ex = -Vector3.Dot(xAxis, eye)
    let ey = -Vector3.Dot(yAxis, eye)
    let ez = -Vector3.Dot(zAxis, eye)

    Matrix.FromValuesToRef(
      xAxis.x,
      yAxis.x,
      zAxis.x,
      0.0,
      xAxis.y,
      yAxis.y,
      zAxis.y,
      0.0,
      xAxis.z,
      yAxis.z,
      zAxis.z,
      0.0,
      ex,
      ey,
      ez,
      1.0,
      result
    )
  }

  /**
   * Create a left-handed orthographic projection matrix
   * @param width - defines the viewport width
   * @param height - defines the viewport height
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @returns a new matrix as a left-handed orthographic projection matrix
   */
  public static OrthoLH(width: number, height: number, znear: number, zfar: number): Matrix {
    let matrix = new Matrix()
    Matrix.OrthoLHToRef(width, height, znear, zfar, matrix)
    return matrix
  }

  /**
   * Store a left-handed orthographic projection to a given matrix
   * @param width - defines the viewport width
   * @param height - defines the viewport height
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @param result - defines the target matrix
   */
  public static OrthoLHToRef(width: number, height: number, znear: number, zfar: number, result: Matrix): void {
    let n = znear
    let f = zfar

    let a = 2.0 / width
    let b = 2.0 / height
    let c = 2.0 / (f - n)
    let d = -(f + n) / (f - n)

    Matrix.FromValuesToRef(a, 0.0, 0.0, 0.0, 0.0, b, 0.0, 0.0, 0.0, 0.0, c, 0.0, 0.0, 0.0, d, 1.0, result)

    result._updateIdentityStatus(a === 1 && b === 1 && c === 1 && d === 0)
  }

  /**
   * Create a left-handed orthographic projection matrix
   * @param left - defines the viewport left coordinate
   * @param right - defines the viewport right coordinate
   * @param bottom - defines the viewport bottom coordinate
   * @param top - defines the viewport top coordinate
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @returns a new matrix as a left-handed orthographic projection matrix
   */
  public static OrthoOffCenterLH(
    left: number,
    right: number,
    bottom: number,
    top: number,
    znear: number,
    zfar: number
  ): Matrix {
    let matrix = new Matrix()
    Matrix.OrthoOffCenterLHToRef(left, right, bottom, top, znear, zfar, matrix)
    return matrix
  }

  /**
   * Stores a left-handed orthographic projection into a given matrix
   * @param left - defines the viewport left coordinate
   * @param right - defines the viewport right coordinate
   * @param bottom - defines the viewport bottom coordinate
   * @param top - defines the viewport top coordinate
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @param result - defines the target matrix
   */
  public static OrthoOffCenterLHToRef(
    left: number,
    right: number,
    bottom: number,
    top: number,
    znear: number,
    zfar: number,
    result: Matrix
  ): void {
    let n = znear
    let f = zfar

    let a = 2.0 / (right - left)
    let b = 2.0 / (top - bottom)
    let c = 2.0 / (f - n)
    let d = -(f + n) / (f - n)
    let i0 = (left + right) / (left - right)
    let i1 = (top + bottom) / (bottom - top)

    Matrix.FromValuesToRef(a, 0.0, 0.0, 0.0, 0.0, b, 0.0, 0.0, 0.0, 0.0, c, 0.0, i0, i1, d, 1.0, result)

    result._markAsUpdated()
  }

  /**
   * Creates a right-handed orthographic projection matrix
   * @param left - defines the viewport left coordinate
   * @param right - defines the viewport right coordinate
   * @param bottom - defines the viewport bottom coordinate
   * @param top - defines the viewport top coordinate
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @returns a new matrix as a right-handed orthographic projection matrix
   */
  public static OrthoOffCenterRH(
    left: number,
    right: number,
    bottom: number,
    top: number,
    znear: number,
    zfar: number
  ): Matrix {
    let matrix = new Matrix()
    Matrix.OrthoOffCenterRHToRef(left, right, bottom, top, znear, zfar, matrix)
    return matrix
  }

  /**
   * Stores a right-handed orthographic projection into a given matrix
   * @param left - defines the viewport left coordinate
   * @param right - defines the viewport right coordinate
   * @param bottom - defines the viewport bottom coordinate
   * @param top - defines the viewport top coordinate
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @param result - defines the target matrix
   */
  public static OrthoOffCenterRHToRef(
    left: number,
    right: number,
    bottom: number,
    top: number,
    znear: number,
    zfar: number,
    result: Matrix
  ): void {
    Matrix.OrthoOffCenterLHToRef(left, right, bottom, top, znear, zfar, result)
    result._m[10] *= -1 // No need to call _markAsUpdated as previous function already called it and let _isIdentityDirty to true
  }

  /**
   * Creates a left-handed perspective projection matrix
   * @param width - defines the viewport width
   * @param height - defines the viewport height
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @returns a new matrix as a left-handed perspective projection matrix
   */
  public static PerspectiveLH(width: number, height: number, znear: number, zfar: number): Matrix {
    let matrix = new Matrix()

    let n = znear
    let f = zfar

    let a = (2.0 * n) / width
    let b = (2.0 * n) / height
    let c = (f + n) / (f - n)
    let d = (-2.0 * f * n) / (f - n)

    Matrix.FromValuesToRef(a, 0.0, 0.0, 0.0, 0.0, b, 0.0, 0.0, 0.0, 0.0, c, 1.0, 0.0, 0.0, d, 0.0, matrix)

    matrix._updateIdentityStatus(false)
    return matrix
  }

  /**
   * Creates a left-handed perspective projection matrix
   * @param fov - defines the horizontal field of view
   * @param aspect - defines the aspect ratio
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @returns a new matrix as a left-handed perspective projection matrix
   */
  public static PerspectiveFovLH(fov: number, aspect: number, znear: number, zfar: number): Matrix {
    let matrix = new Matrix()
    Matrix.PerspectiveFovLHToRef(fov, aspect, znear, zfar, matrix)
    return matrix
  }

  /**
   * Stores a left-handed perspective projection into a given matrix
   * @param fov - defines the horizontal field of view
   * @param aspect - defines the aspect ratio
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @param result - defines the target matrix
   * @param isVerticalFovFixed - defines it the fov is vertically fixed (default) or horizontally
   */
  public static PerspectiveFovLHToRef(
    fov: number,
    aspect: number,
    znear: number,
    zfar: number,
    result: Matrix,
    isVerticalFovFixed = true
  ): void {
    let n = znear
    let f = zfar

    let t = 1.0 / Math.tan(fov * 0.5)
    let a = isVerticalFovFixed ? t / aspect : t
    let b = isVerticalFovFixed ? t : t * aspect
    let c = (f + n) / (f - n)
    let d = (-2.0 * f * n) / (f - n)

    Matrix.FromValuesToRef(a, 0.0, 0.0, 0.0, 0.0, b, 0.0, 0.0, 0.0, 0.0, c, 1.0, 0.0, 0.0, d, 0.0, result)
    result._updateIdentityStatus(false)
  }

  /**
   * Creates a right-handed perspective projection matrix
   * @param fov - defines the horizontal field of view
   * @param aspect - defines the aspect ratio
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @returns a new matrix as a right-handed perspective projection matrix
   */
  public static PerspectiveFovRH(fov: number, aspect: number, znear: number, zfar: number): Matrix {
    let matrix = new Matrix()
    Matrix.PerspectiveFovRHToRef(fov, aspect, znear, zfar, matrix)
    return matrix
  }

  /**
   * Stores a right-handed perspective projection into a given matrix
   * @param fov - defines the horizontal field of view
   * @param aspect - defines the aspect ratio
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @param result - defines the target matrix
   * @param isVerticalFovFixed - defines it the fov is vertically fixed (default) or horizontally
   */
  public static PerspectiveFovRHToRef(
    fov: number,
    aspect: number,
    znear: number,
    zfar: number,
    result: Matrix,
    isVerticalFovFixed = true
  ): void {
    /* alternatively this could be expressed as:
    //    m = PerspectiveFovLHToRef
    //    m[10] *= -1.0;
    //    m[11] *= -1.0;
    */

    let n = znear
    let f = zfar

    let t = 1.0 / Math.tan(fov * 0.5)
    let a = isVerticalFovFixed ? t / aspect : t
    let b = isVerticalFovFixed ? t : t * aspect
    let c = -(f + n) / (f - n)
    let d = (-2 * f * n) / (f - n)

    Matrix.FromValuesToRef(a, 0.0, 0.0, 0.0, 0.0, b, 0.0, 0.0, 0.0, 0.0, c, -1.0, 0.0, 0.0, d, 0.0, result)

    result._updateIdentityStatus(false)
  }

  /**
   * Stores a perspective projection for WebVR info a given matrix
   * @param fov - defines the field of view
   * @param znear - defines the near clip plane
   * @param zfar - defines the far clip plane
   * @param result - defines the target matrix
   * @param rightHanded - defines if the matrix must be in right-handed mode (false by default)
   */
  public static PerspectiveFovWebVRToRef(
    fov: { upDegrees: number; downDegrees: number; leftDegrees: number; rightDegrees: number },
    znear: number,
    zfar: number,
    result: Matrix,
    rightHanded = false
  ): void {
    let rightHandedFactor = rightHanded ? -1 : 1

    let upTan = Math.tan((fov.upDegrees * Math.PI) / 180.0)
    let downTan = Math.tan((fov.downDegrees * Math.PI) / 180.0)
    let leftTan = Math.tan((fov.leftDegrees * Math.PI) / 180.0)
    let rightTan = Math.tan((fov.rightDegrees * Math.PI) / 180.0)
    let xScale = 2.0 / (leftTan + rightTan)
    let yScale = 2.0 / (upTan + downTan)
    const m = result._m
    m[0] = xScale
    m[1] = m[2] = m[3] = m[4] = 0.0
    m[5] = yScale
    m[6] = m[7] = 0.0
    m[8] = (leftTan - rightTan) * xScale * 0.5
    m[9] = -((upTan - downTan) * yScale * 0.5)
    m[10] = -zfar / (znear - zfar)
    m[11] = 1.0 * rightHandedFactor
    m[12] = m[13] = m[15] = 0.0
    m[14] = -(2.0 * zfar * znear) / (zfar - znear)

    result._markAsUpdated()
  }

  /**
   * Extracts a 2x2 matrix from a given matrix and store the result in a FloatArray
   * @param matrix - defines the matrix to use
   * @returns a new FloatArray array with 4 elements : the 2x2 matrix extracted from the given matrix
   */
  public static GetAsMatrix2x2(matrix: Matrix): FloatArray {
    return [matrix._m[0], matrix._m[1], matrix._m[4], matrix._m[5]]
  }
  /**
   * Extracts a 3x3 matrix from a given matrix and store the result in a FloatArray
   * @param matrix - defines the matrix to use
   * @returns a new FloatArray array with 9 elements : the 3x3 matrix extracted from the given matrix
   */
  public static GetAsMatrix3x3(matrix: Matrix): FloatArray {
    return [
      matrix._m[0],
      matrix._m[1],
      matrix._m[2],
      matrix._m[4],
      matrix._m[5],
      matrix._m[6],
      matrix._m[8],
      matrix._m[9],
      matrix._m[10]
    ]
  }

  /**
   * Compute the transpose of a given matrix
   * @param matrix - defines the matrix to transpose
   * @returns the new matrix
   */
  public static Transpose(matrix: Matrix): Matrix {
    let result = new Matrix()
    Matrix.TransposeToRef(matrix, result)
    return result
  }

  /**
   * Compute the transpose of a matrix and store it in a target matrix
   * @param matrix - defines the matrix to transpose
   * @param result - defines the target matrix
   */
  public static TransposeToRef(matrix: Matrix, result: Matrix): void {
    const rm = result._m
    const mm = matrix._m
    rm[0] = mm[0]
    rm[1] = mm[4]
    rm[2] = mm[8]
    rm[3] = mm[12]

    rm[4] = mm[1]
    rm[5] = mm[5]
    rm[6] = mm[9]
    rm[7] = mm[13]

    rm[8] = mm[2]
    rm[9] = mm[6]
    rm[10] = mm[10]
    rm[11] = mm[14]

    rm[12] = mm[3]
    rm[13] = mm[7]
    rm[14] = mm[11]
    rm[15] = mm[15]
    // identity-ness does not change when transposing
    result._updateIdentityStatus(matrix._isIdentity, matrix._isIdentityDirty)
  }

  /**
   * Computes a reflection matrix from a plane
   * @param plane - defines the reflection plane
   * @returns a new matrix
   */
  public static Reflection(plane: Plane): Matrix {
    let matrix = new Matrix()
    Matrix.ReflectionToRef(plane, matrix)
    return matrix
  }

  /**
   * Computes a reflection matrix from a plane
   * @param plane - defines the reflection plane
   * @param result - defines the target matrix
   */
  public static ReflectionToRef(plane: Plane, result: Matrix): void {
    plane.normalize()
    let x = plane.normal.x
    let y = plane.normal.y
    let z = plane.normal.z
    let temp = -2 * x
    let temp2 = -2 * y
    let temp3 = -2 * z
    Matrix.FromValuesToRef(
      temp * x + 1,
      temp2 * x,
      temp3 * x,
      0.0,
      temp * y,
      temp2 * y + 1,
      temp3 * y,
      0.0,
      temp * z,
      temp2 * z,
      temp3 * z + 1,
      0.0,
      temp * plane.d,
      temp2 * plane.d,
      temp3 * plane.d,
      1.0,
      result
    )
  }

  /**
   * Sets the given matrix as a rotation matrix composed from the 3 left handed axes
   * @param xaxis - defines the value of the 1st axis
   * @param yaxis - defines the value of the 2nd axis
   * @param zaxis - defines the value of the 3rd axis
   * @param result - defines the target matrix
   */
  public static FromXYZAxesToRef(xaxis: Vector3, yaxis: Vector3, zaxis: Vector3, result: Matrix) {
    Matrix.FromValuesToRef(
      xaxis.x,
      xaxis.y,
      xaxis.z,
      0.0,
      yaxis.x,
      yaxis.y,
      yaxis.z,
      0.0,
      zaxis.x,
      zaxis.y,
      zaxis.z,
      0.0,
      0.0,
      0.0,
      0.0,
      1.0,
      result
    )
  }

  /**
   * Creates a rotation matrix from a quaternion and stores it in a target matrix
   * @param quat - defines the quaternion to use
   * @param result - defines the target matrix
   */
  public static FromQuaternionToRef(quat: Quaternion, result: Matrix) {
    let xx = quat.x * quat.x
    let yy = quat.y * quat.y
    let zz = quat.z * quat.z
    let xy = quat.x * quat.y
    let zw = quat.z * quat.w
    let zx = quat.z * quat.x
    let yw = quat.y * quat.w
    let yz = quat.y * quat.z
    let xw = quat.x * quat.w

    result._m[0] = 1.0 - 2.0 * (yy + zz)
    result._m[1] = 2.0 * (xy + zw)
    result._m[2] = 2.0 * (zx - yw)
    result._m[3] = 0.0

    result._m[4] = 2.0 * (xy - zw)
    result._m[5] = 1.0 - 2.0 * (zz + xx)
    result._m[6] = 2.0 * (yz + xw)
    result._m[7] = 0.0

    result._m[8] = 2.0 * (zx + yw)
    result._m[9] = 2.0 * (yz - xw)
    result._m[10] = 1.0 - 2.0 * (yy + xx)
    result._m[11] = 0.0

    result._m[12] = 0.0
    result._m[13] = 0.0
    result._m[14] = 0.0
    result._m[15] = 1.0

    result._markAsUpdated()
  }

  /** @internal */
  public _markAsUpdated() {
    this.updateFlag = Matrix._updateFlagSeed++
    this._isIdentity = false
    this._isIdentity3x2 = false
    this._isIdentityDirty = true
    this._isIdentity3x2Dirty = true
  }

  // Properties

  /**
   * Check if the current matrix is identity
   * @returns true is the matrix is the identity matrix
   */
  public isIdentity(): boolean {
    if (this._isIdentityDirty) {
      this._isIdentityDirty = false
      const m = this._m
      this._isIdentity =
        m[0] === 1.0 &&
        m[1] === 0.0 &&
        m[2] === 0.0 &&
        m[3] === 0.0 &&
        m[4] === 0.0 &&
        m[5] === 1.0 &&
        m[6] === 0.0 &&
        m[7] === 0.0 &&
        m[8] === 0.0 &&
        m[9] === 0.0 &&
        m[10] === 1.0 &&
        m[11] === 0.0 &&
        m[12] === 0.0 &&
        m[13] === 0.0 &&
        m[14] === 0.0 &&
        m[15] === 1.0
    }

    return this._isIdentity
  }

  /**
   * Check if the current matrix is identity as a texture matrix (3x2 store in 4x4)
   * @returns true is the matrix is the identity matrix
   */
  public isIdentityAs3x2(): boolean {
    if (this._isIdentity3x2Dirty) {
      this._isIdentity3x2Dirty = false
      if (this._m[0] !== 1.0 || this._m[5] !== 1.0 || this._m[15] !== 1.0) {
        this._isIdentity3x2 = false
      } else if (
        this._m[1] !== 0.0 ||
        this._m[2] !== 0.0 ||
        this._m[3] !== 0.0 ||
        this._m[4] !== 0.0 ||
        this._m[6] !== 0.0 ||
        this._m[7] !== 0.0 ||
        this._m[8] !== 0.0 ||
        this._m[9] !== 0.0 ||
        this._m[10] !== 0.0 ||
        this._m[11] !== 0.0 ||
        this._m[12] !== 0.0 ||
        this._m[13] !== 0.0 ||
        this._m[14] !== 0.0
      ) {
        this._isIdentity3x2 = false
      } else {
        this._isIdentity3x2 = true
      }
    }

    return this._isIdentity3x2
  }

  /**
   * Gets the determinant of the matrix
   * @returns the matrix determinant
   */
  public determinant(): number {
    if (this._isIdentity === true) {
      return 1
    }

    const m = this._m
    // tslint:disable-next-line:one-variable-per-declaration
    const m00 = m[0],
      m01 = m[1],
      m02 = m[2],
      m03 = m[3]
    // tslint:disable-next-line:one-variable-per-declaration
    const m10 = m[4],
      m11 = m[5],
      m12 = m[6],
      m13 = m[7]
    // tslint:disable-next-line:one-variable-per-declaration
    const m20 = m[8],
      m21 = m[9],
      m22 = m[10],
      m23 = m[11]
    // tslint:disable-next-line:one-variable-per-declaration
    const m30 = m[12],
      m31 = m[13],
      m32 = m[14],
      m33 = m[15]
    /*
    // https://en.wikipedia.org/wiki/Laplace_expansion
    // to compute the deterrminant of a 4x4 Matrix we compute the cofactors of any row or column,
    // then we multiply each Cofactor by its corresponding matrix value and sum them all to get the determinant
    // Cofactor(i, j) = sign(i,j) * det(Minor(i, j))
    // where
    //  - sign(i,j) = (i+j) % 2 === 0 ? 1 : -1
    //  - Minor(i, j) is the 3x3 matrix we get by removing row i and column j from current Matrix
    //
    // Here we do that for the 1st row.
    */

    // tslint:disable:variable-name
    const det_22_33 = m22 * m33 - m32 * m23
    const det_21_33 = m21 * m33 - m31 * m23
    const det_21_32 = m21 * m32 - m31 * m22
    const det_20_33 = m20 * m33 - m30 * m23
    const det_20_32 = m20 * m32 - m22 * m30
    const det_20_31 = m20 * m31 - m30 * m21
    const cofact_00 = +(m11 * det_22_33 - m12 * det_21_33 + m13 * det_21_32)
    const cofact_01 = -(m10 * det_22_33 - m12 * det_20_33 + m13 * det_20_32)
    const cofact_02 = +(m10 * det_21_33 - m11 * det_20_33 + m13 * det_20_31)
    const cofact_03 = -(m10 * det_21_32 - m11 * det_20_32 + m12 * det_20_31)
    // tslint:enable:variable-name
    return m00 * cofact_00 + m01 * cofact_01 + m02 * cofact_02 + m03 * cofact_03
  }

  // Methods

  /**
   * Returns the matrix as a FloatArray
   * @returns the matrix underlying array
   */
  public toArray(): Readonly<FloatArray> {
    return this._m
  }
  /**
   * Returns the matrix as a FloatArray
   * @returns the matrix underlying array.
   */
  public asArray(): Readonly<FloatArray> {
    return this._m
  }

  /**
   * Inverts the current matrix in place
   * @returns the current inverted matrix
   */
  public invert(): Matrix {
    this.invertToRef(this)
    return this
  }
  /**
   * Sets all the matrix elements to zero
   * @returns the current matrix
   */
  public reset(): Matrix {
    Matrix.FromValuesToRef(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, this)
    this._updateIdentityStatus(false)
    return this
  }

  /**
   * Adds the current matrix with a second one
   * @param other - defines the matrix to add
   * @returns a new matrix as the addition of the current matrix and the given one
   */
  public add(other: Matrix): Matrix {
    let result = new Matrix()
    this.addToRef(other, result)
    return result
  }

  /**
   * Sets the given matrix "result" to the addition of the current matrix and the given one
   * @param other - defines the matrix to add
   * @param result - defines the target matrix
   * @returns the current matrix
   */
  public addToRef(other: Matrix, result: Matrix): Matrix {
    for (let index = 0; index < 16; index++) {
      result._m[index] = this._m[index] + other._m[index]
    }
    result._markAsUpdated()
    return this
  }

  /**
   * Adds in place the given matrix to the current matrix
   * @param other - defines the second operand
   * @returns the current updated matrix
   */
  public addToSelf(other: Matrix): Matrix {
    for (let index = 0; index < 16; index++) {
      this._m[index] += other._m[index]
    }
    this._markAsUpdated()
    return this
  }

  /**
   * Sets the given matrix to the current inverted Matrix
   * @param other - defines the target matrix
   * @returns the unmodified current matrix
   */
  public invertToRef(other: Matrix): Matrix {
    if (this._isIdentity === true) {
      Matrix.IdentityToRef(other)
      return this
    }

    // the inverse of a Matrix is the transpose of cofactor matrix divided by the determinant
    const m = this._m
    // tslint:disable:one-variable-per-declaration
    const m00 = m[0],
      m01 = m[1],
      m02 = m[2],
      m03 = m[3]
    const m10 = m[4],
      m11 = m[5],
      m12 = m[6],
      m13 = m[7]
    const m20 = m[8],
      m21 = m[9],
      m22 = m[10],
      m23 = m[11]
    const m30 = m[12],
      m31 = m[13],
      m32 = m[14],
      m33 = m[15]
    // tslint:enable:one-variable-per-declaration

    // tslint:disable:variable-name
    const det_22_33 = m22 * m33 - m32 * m23
    const det_21_33 = m21 * m33 - m31 * m23
    const det_21_32 = m21 * m32 - m31 * m22
    const det_20_33 = m20 * m33 - m30 * m23
    const det_20_32 = m20 * m32 - m22 * m30
    const det_20_31 = m20 * m31 - m30 * m21

    const cofact_00 = +(m11 * det_22_33 - m12 * det_21_33 + m13 * det_21_32)
    const cofact_01 = -(m10 * det_22_33 - m12 * det_20_33 + m13 * det_20_32)
    const cofact_02 = +(m10 * det_21_33 - m11 * det_20_33 + m13 * det_20_31)
    const cofact_03 = -(m10 * det_21_32 - m11 * det_20_32 + m12 * det_20_31)

    const det = m00 * cofact_00 + m01 * cofact_01 + m02 * cofact_02 + m03 * cofact_03

    if (det === 0) {
      // not invertible
      other.copyFrom(this)
      return this
    }

    const detInv = 1 / det
    const det_12_33 = m12 * m33 - m32 * m13
    const det_11_33 = m11 * m33 - m31 * m13
    const det_11_32 = m11 * m32 - m31 * m12
    const det_10_33 = m10 * m33 - m30 * m13
    const det_10_32 = m10 * m32 - m30 * m12
    const det_10_31 = m10 * m31 - m30 * m11
    const det_12_23 = m12 * m23 - m22 * m13
    const det_11_23 = m11 * m23 - m21 * m13
    const det_11_22 = m11 * m22 - m21 * m12
    const det_10_23 = m10 * m23 - m20 * m13
    const det_10_22 = m10 * m22 - m20 * m12
    const det_10_21 = m10 * m21 - m20 * m11

    const cofact_10 = -(m01 * det_22_33 - m02 * det_21_33 + m03 * det_21_32)
    const cofact_11 = +(m00 * det_22_33 - m02 * det_20_33 + m03 * det_20_32)
    const cofact_12 = -(m00 * det_21_33 - m01 * det_20_33 + m03 * det_20_31)
    const cofact_13 = +(m00 * det_21_32 - m01 * det_20_32 + m02 * det_20_31)

    const cofact_20 = +(m01 * det_12_33 - m02 * det_11_33 + m03 * det_11_32)
    const cofact_21 = -(m00 * det_12_33 - m02 * det_10_33 + m03 * det_10_32)
    const cofact_22 = +(m00 * det_11_33 - m01 * det_10_33 + m03 * det_10_31)
    const cofact_23 = -(m00 * det_11_32 - m01 * det_10_32 + m02 * det_10_31)

    const cofact_30 = -(m01 * det_12_23 - m02 * det_11_23 + m03 * det_11_22)
    const cofact_31 = +(m00 * det_12_23 - m02 * det_10_23 + m03 * det_10_22)
    const cofact_32 = -(m00 * det_11_23 - m01 * det_10_23 + m03 * det_10_21)
    const cofact_33 = +(m00 * det_11_22 - m01 * det_10_22 + m02 * det_10_21)

    Matrix.FromValuesToRef(
      cofact_00 * detInv,
      cofact_10 * detInv,
      cofact_20 * detInv,
      cofact_30 * detInv,
      cofact_01 * detInv,
      cofact_11 * detInv,
      cofact_21 * detInv,
      cofact_31 * detInv,
      cofact_02 * detInv,
      cofact_12 * detInv,
      cofact_22 * detInv,
      cofact_32 * detInv,
      cofact_03 * detInv,
      cofact_13 * detInv,
      cofact_23 * detInv,
      cofact_33 * detInv,
      other
    )
    // tslint:enable:variable-name

    return this
  }

  /**
   * add a value at the specified position in the current Matrix
   * @param index - the index of the value within the matrix. between 0 and 15.
   * @param value - the value to be added
   * @returns the current updated matrix
   */
  public addAtIndex(index: number, value: number): Matrix {
    this._m[index] += value
    this._markAsUpdated()
    return this
  }

  /**
   * mutiply the specified position in the current Matrix by a value
   * @param index - the index of the value within the matrix. between 0 and 15.
   * @param value - the value to be added
   * @returns the current updated matrix
   */
  public multiplyAtIndex(index: number, value: number): Matrix {
    this._m[index] *= value
    this._markAsUpdated()
    return this
  }

  /**
   * Inserts the translation vector (using 3 floats) in the current matrix
   * @param x - defines the 1st component of the translation
   * @param y - defines the 2nd component of the translation
   * @param z - defines the 3rd component of the translation
   * @returns the current updated matrix
   */
  public setTranslationFromFloats(x: number, y: number, z: number): Matrix {
    this._m[12] = x
    this._m[13] = y
    this._m[14] = z
    this._markAsUpdated()
    return this
  }

  /**
   * Inserts the translation vector in the current matrix
   * @param vector3 - defines the translation to insert
   * @returns the current updated matrix
   */
  public setTranslation(vector3: Vector3): Matrix {
    return this.setTranslationFromFloats(vector3.x, vector3.y, vector3.z)
  }

  /**
   * Gets the translation value of the current matrix
   * @returns a new Vector3 as the extracted translation from the matrix
   */
  public getTranslation(): Vector3 {
    return new Vector3(this._m[12], this._m[13], this._m[14])
  }

  /**
   * Fill a Vector3 with the extracted translation from the matrix
   * @param result - defines the Vector3 where to store the translation
   * @returns the current matrix
   */
  public getTranslationToRef(result: Vector3): Matrix {
    result.x = this._m[12]
    result.y = this._m[13]
    result.z = this._m[14]
    return this
  }

  /**
   * Remove rotation and scaling part from the matrix
   * @returns the updated matrix
   */
  public removeRotationAndScaling(): Matrix {
    const m = this.m
    Matrix.FromValuesToRef(1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0, 1.0, 0.0, m[12], m[13], m[14], m[15], this)
    this._updateIdentityStatus(m[12] === 0 && m[13] === 0 && m[14] === 0 && m[15] === 1)
    return this
  }

  /**
   * Multiply two matrices
   * @param other - defines the second operand
   * @returns a new matrix set with the multiplication result of the current Matrix and the given one
   */
  public multiply(other: Readonly<Matrix>): Matrix {
    let result = new Matrix()
    this.multiplyToRef(other, result)
    return result
  }

  /**
   * Copy the current matrix from the given one
   * @param other - defines the source matrix
   * @returns the current updated matrix
   */
  public copyFrom(other: Readonly<Matrix>): Matrix {
    other.copyToArray(this._m)
    const o = other as Matrix
    this._updateIdentityStatus(o._isIdentity, o._isIdentityDirty, o._isIdentity3x2, o._isIdentity3x2Dirty)
    return this
  }

  /**
   * Populates the given array from the starting index with the current matrix values
   * @param array - defines the target array
   * @param offset - defines the offset in the target array where to start storing values
   * @returns the current matrix
   */
  public copyToArray(array: FloatArray, offset: number = 0): Matrix {
    for (let index = 0; index < 16; index++) {
      array[offset + index] = this._m[index]
    }
    return this
  }

  /**
   * Sets the given matrix "result" with the multiplication result of the current Matrix and the given one
   * @param other - defines the second operand
   * @param result - defines the matrix where to store the multiplication
   * @returns the current matrix
   */
  public multiplyToRef(other: Readonly<Matrix>, result: Matrix): Matrix {
    if (this._isIdentity) {
      result.copyFrom(other)
      return this
    }
    if ((other as Matrix)._isIdentity) {
      result.copyFrom(this)
      return this
    }

    this.multiplyToArray(other, result._m, 0)
    result._markAsUpdated()
    return this
  }

  /**
   * Sets the FloatArray "result" from the given index "offset" with the multiplication of the current matrix and the given one
   * @param other - defines the second operand
   * @param result - defines the array where to store the multiplication
   * @param offset - defines the offset in the target array where to start storing values
   * @returns the current matrix
   */
  public multiplyToArray(other: Readonly<Matrix>, result: FloatArray, offset: number): Matrix {
    const m = this._m
    const otherM = other.m

    // tslint:disable:one-variable-per-declaration
    let tm0 = m[0],
      tm1 = m[1],
      tm2 = m[2],
      tm3 = m[3]
    let tm4 = m[4],
      tm5 = m[5],
      tm6 = m[6],
      tm7 = m[7]
    let tm8 = m[8],
      tm9 = m[9],
      tm10 = m[10],
      tm11 = m[11]
    let tm12 = m[12],
      tm13 = m[13],
      tm14 = m[14],
      tm15 = m[15]

    let om0 = otherM[0],
      om1 = otherM[1],
      om2 = otherM[2],
      om3 = otherM[3]
    let om4 = otherM[4],
      om5 = otherM[5],
      om6 = otherM[6],
      om7 = otherM[7]
    let om8 = otherM[8],
      om9 = otherM[9],
      om10 = otherM[10],
      om11 = otherM[11]
    let om12 = otherM[12],
      om13 = otherM[13],
      om14 = otherM[14],
      om15 = otherM[15]
    // tslint:enable:one-variable-per-declaration
    result[offset] = tm0 * om0 + tm1 * om4 + tm2 * om8 + tm3 * om12
    result[offset + 1] = tm0 * om1 + tm1 * om5 + tm2 * om9 + tm3 * om13
    result[offset + 2] = tm0 * om2 + tm1 * om6 + tm2 * om10 + tm3 * om14
    result[offset + 3] = tm0 * om3 + tm1 * om7 + tm2 * om11 + tm3 * om15

    result[offset + 4] = tm4 * om0 + tm5 * om4 + tm6 * om8 + tm7 * om12
    result[offset + 5] = tm4 * om1 + tm5 * om5 + tm6 * om9 + tm7 * om13
    result[offset + 6] = tm4 * om2 + tm5 * om6 + tm6 * om10 + tm7 * om14
    result[offset + 7] = tm4 * om3 + tm5 * om7 + tm6 * om11 + tm7 * om15

    result[offset + 8] = tm8 * om0 + tm9 * om4 + tm10 * om8 + tm11 * om12
    result[offset + 9] = tm8 * om1 + tm9 * om5 + tm10 * om9 + tm11 * om13
    result[offset + 10] = tm8 * om2 + tm9 * om6 + tm10 * om10 + tm11 * om14
    result[offset + 11] = tm8 * om3 + tm9 * om7 + tm10 * om11 + tm11 * om15

    result[offset + 12] = tm12 * om0 + tm13 * om4 + tm14 * om8 + tm15 * om12
    result[offset + 13] = tm12 * om1 + tm13 * om5 + tm14 * om9 + tm15 * om13
    result[offset + 14] = tm12 * om2 + tm13 * om6 + tm14 * om10 + tm15 * om14
    result[offset + 15] = tm12 * om3 + tm13 * om7 + tm14 * om11 + tm15 * om15
    return this
  }

  /**
   * Check equality between this matrix and a second one
   * @param value - defines the second matrix to compare
   * @returns true is the current matrix and the given one values are strictly equal
   */
  public equals(value: Matrix): boolean {
    const other = value
    if (!other) {
      return false
    }

    if (this._isIdentity || other._isIdentity) {
      if (!this._isIdentityDirty && !other._isIdentityDirty) {
        return this._isIdentity && other._isIdentity
      }
    }

    const m = this.m
    const om = other.m
    return (
      m[0] === om[0] &&
      m[1] === om[1] &&
      m[2] === om[2] &&
      m[3] === om[3] &&
      m[4] === om[4] &&
      m[5] === om[5] &&
      m[6] === om[6] &&
      m[7] === om[7] &&
      m[8] === om[8] &&
      m[9] === om[9] &&
      m[10] === om[10] &&
      m[11] === om[11] &&
      m[12] === om[12] &&
      m[13] === om[13] &&
      m[14] === om[14] &&
      m[15] === om[15]
    )
  }

  /**
   * Clone the current matrix
   * @returns a new matrix from the current matrix
   */
  public clone(): Matrix {
    const matrix = new Matrix()
    matrix.copyFrom(this)
    return matrix
  }

  /**
   * Returns the name of the current matrix class
   * @returns the string "Matrix"
   */
  public getClassName(): string {
    return 'Matrix'
  }

  /**
   * Gets the hash code of the current matrix
   * @returns the hash code
   */
  public getHashCode(): number {
    let hash = this._m[0] || 0
    for (let i = 1; i < 16; i++) {
      hash = (hash * 397) ^ (this._m[i] || 0)
    }
    return hash
  }

  /**
   * Decomposes the current Matrix into a translation, rotation and scaling components
   * @param scale - defines the scale vector3 given as a reference to update
   * @param rotation - defines the rotation quaternion given as a reference to update
   * @param translation - defines the translation vector3 given as a reference to update
   * @returns true if operation was successful
   */
  public decompose(scale?: Vector3, rotation?: Quaternion, translation?: Vector3): boolean {
    if (this._isIdentity) {
      if (translation) {
        translation.setAll(0)
      }
      if (scale) {
        scale.setAll(1)
      }
      if (rotation) {
        rotation.copyFromFloats(0, 0, 0, 1)
      }
      return true
    }

    const m = this._m
    if (translation) {
      translation.copyFromFloats(m[12], m[13], m[14])
    }

    const usedScale = scale || MathTmp.Vector3[0]
    usedScale.x = Math.sqrt(m[0] * m[0] + m[1] * m[1] + m[2] * m[2])
    usedScale.y = Math.sqrt(m[4] * m[4] + m[5] * m[5] + m[6] * m[6])
    usedScale.z = Math.sqrt(m[8] * m[8] + m[9] * m[9] + m[10] * m[10])

    if (this.determinant() <= 0) {
      usedScale.y *= -1
    }

    if (usedScale.x === 0 || usedScale.y === 0 || usedScale.z === 0) {
      if (rotation) {
        rotation.copyFromFloats(0.0, 0.0, 0.0, 1.0)
      }
      return false
    }

    if (rotation) {
      // tslint:disable-next-line:one-variable-per-declaration
      const sx = 1 / usedScale.x,
        sy = 1 / usedScale.y,
        sz = 1 / usedScale.z
      Matrix.FromValuesToRef(
        m[0] * sx,
        m[1] * sx,
        m[2] * sx,
        0.0,
        m[4] * sy,
        m[5] * sy,
        m[6] * sy,
        0.0,
        m[8] * sz,
        m[9] * sz,
        m[10] * sz,
        0.0,
        0.0,
        0.0,
        0.0,
        1.0,
        MathTmp.Matrix[0]
      )

      Quaternion.FromRotationMatrixToRef(MathTmp.Matrix[0], rotation)
    }

    return true
  }

  /**
   * Gets specific row of the matrix
   * @param index - defines the number of the row to get
   * @returns the index-th row of the current matrix as a new Vector4
   */
  public getRow(index: number): Nullable<Vector4> {
    if (index < 0 || index > 3) {
      return null
    }
    let i = index * 4
    return new Vector4(this._m[i + 0], this._m[i + 1], this._m[i + 2], this._m[i + 3])
  }

  /**
   * Sets the index-th row of the current matrix to the vector4 values
   * @param index - defines the number of the row to set
   * @param row - defines the target vector4
   * @returns the updated current matrix
   */
  public setRow(index: number, row: Vector4): Matrix {
    return this.setRowFromFloats(index, row.x, row.y, row.z, row.w)
  }

  /**
   * Compute the transpose of the matrix
   * @returns the new transposed matrix
   */
  public transpose(): Matrix {
    return Matrix.Transpose(this)
  }

  /**
   * Compute the transpose of the matrix and store it in a given matrix
   * @param result - defines the target matrix
   * @returns the current matrix
   */
  public transposeToRef(result: Matrix): Matrix {
    Matrix.TransposeToRef(this, result)
    return this
  }

  /**
   * Sets the index-th row of the current matrix with the given 4 x float values
   * @param index - defines the row index
   * @param x - defines the x component to set
   * @param y - defines the y component to set
   * @param z - defines the z component to set
   * @param w - defines the w component to set
   * @returns the updated current matrix
   */
  public setRowFromFloats(index: number, x: number, y: number, z: number, w: number): Matrix {
    if (index < 0 || index > 3) {
      return this
    }
    let i = index * 4
    this._m[i + 0] = x
    this._m[i + 1] = y
    this._m[i + 2] = z
    this._m[i + 3] = w

    this._markAsUpdated()
    return this
  }

  /**
   * Compute a new matrix set with the current matrix values multiplied by scale (float)
   * @param scale - defines the scale factor
   * @returns a new matrix
   */
  public scale(scale: number): Matrix {
    let result = new Matrix()
    this.scaleToRef(scale, result)
    return result
  }

  /**
   * Scale the current matrix values by a factor to a given result matrix
   * @param scale - defines the scale factor
   * @param result - defines the matrix to store the result
   * @returns the current matrix
   */
  public scaleToRef(scale: number, result: Matrix): Matrix {
    for (let index = 0; index < 16; index++) {
      result._m[index] = this._m[index] * scale
    }
    result._markAsUpdated()
    return this
  }

  /**
   * Scale the current matrix values by a factor and add the result to a given matrix
   * @param scale - defines the scale factor
   * @param result - defines the Matrix to store the result
   * @returns the current matrix
   */
  public scaleAndAddToRef(scale: number, result: Matrix): Matrix {
    for (let index = 0; index < 16; index++) {
      result._m[index] += this._m[index] * scale
    }
    result._markAsUpdated()
    return this
  }

  /**
   * Writes to the given matrix a normal matrix, computed from this one (using values from identity matrix for fourth row and column).
   * @param ref - matrix to store the result
   */
  public toNormalMatrix(ref: Matrix): void {
    const tmp = MathTmp.Matrix[0]
    this.invertToRef(tmp)
    tmp.transposeToRef(ref)
    let m = ref._m
    Matrix.FromValuesToRef(
      m[0],
      m[1],
      m[2],
      0.0,
      m[4],
      m[5],
      m[6],
      0.0,
      m[8],
      m[9],
      m[10],
      0.0,
      0.0,
      0.0,
      0.0,
      1.0,
      ref
    )
  }

  /**
   * Gets only rotation part of the current matrix
   * @returns a new matrix sets to the extracted rotation matrix from the current one
   */
  public getRotationMatrix(): Matrix {
    let result = new Matrix()
    this.getRotationMatrixToRef(result)
    return result
  }

  /**
   * Extracts the rotation matrix from the current one and sets it as the given "result"
   * @param result - defines the target matrix to store data to
   * @returns the current matrix
   */
  public getRotationMatrixToRef(result: Matrix): Matrix {
    const scale = MathTmp.Vector3[0]
    if (!this.decompose(scale)) {
      Matrix.IdentityToRef(result)
      return this
    }

    const m = this._m
    // tslint:disable-next-line:one-variable-per-declaration
    const sx = 1 / scale.x,
      sy = 1 / scale.y,
      sz = 1 / scale.z
    Matrix.FromValuesToRef(
      m[0] * sx,
      m[1] * sx,
      m[2] * sx,
      0.0,
      m[4] * sy,
      m[5] * sy,
      m[6] * sy,
      0.0,
      m[8] * sz,
      m[9] * sz,
      m[10] * sz,
      0.0,
      0.0,
      0.0,
      0.0,
      1.0,
      result
    )
    return this
  }

  /**
   * Toggles model matrix from being right handed to left handed in place and vice versa
   */
  public toggleModelMatrixHandInPlace() {
    const m = this._m
    m[2] *= -1
    m[6] *= -1
    m[8] *= -1
    m[9] *= -1
    m[14] *= -1
    this._markAsUpdated()
  }

  /**
   * Toggles projection matrix from being right handed to left handed in place and vice versa
   */
  public toggleProjectionMatrixHandInPlace() {
    let m = this._m
    m[8] *= -1
    m[9] *= -1
    m[10] *= -1
    m[11] *= -1
    this._markAsUpdated()
  }

  /** @internal */
  private _updateIdentityStatus(
    isIdentity: boolean,
    isIdentityDirty: boolean = false,
    isIdentity3x2: boolean = false,
    isIdentity3x2Dirty: boolean = true
  ) {
    this.updateFlag = Matrix._updateFlagSeed++
    this._isIdentity = isIdentity
    this._isIdentity3x2 = isIdentity || isIdentity3x2
    this._isIdentityDirty = this._isIdentity ? false : isIdentityDirty
    this._isIdentity3x2Dirty = this._isIdentity3x2 ? false : isIdentity3x2Dirty
  }
}
