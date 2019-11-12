import { Vector2 } from './Vector2'
import { Arc2 } from './Arc2'
import { Orientation } from './types'

/**
 * Represents a 2D path made up of multiple 2D points
 * @public
 */
export class Path2 {
  /**
   * If the path start and end point are the same
   */
  public closed = false
  private _points = new Array<Vector2>()
  private _length = 0.0

  /**
   * Creates a Path2 object from the starting 2D coordinates x and y.
   * @param x - the starting points x value
   * @param y - the starting points y value
   */
  constructor(x: number, y: number) {
    this._points.push(new Vector2(x, y))
  }

  /**
   * Creates a new path starting from an x and y position
   * @param x - starting x value
   * @param y - starting y value
   * @returns a new Path2 starting at the coordinates (x, y).
   */
  public static StartingAt(x: number, y: number): Path2 {
    return new Path2(x, y)
  }

  /**
   * Adds a new segment until the given coordinates (x, y) to the current Path2.
   * @param x - the added points x value
   * @param y - the added points y value
   * @returns the updated Path2.
   */
  public addLineTo(x: number, y: number): Path2 {
    if (this.closed) {
      return this
    }
    let newPoint = new Vector2(x, y)
    let previousPoint = this._points[this._points.length - 1]
    this._points.push(newPoint)
    this._length += newPoint.subtract(previousPoint).length()
    return this
  }

  /**
   * Adds _numberOfSegments_ segments according to the arc definition (middle point coordinates, end point coordinates, the arc start point being the current Path2 last point) to the current Path2.
   * @param midX - middle point x value
   * @param midY - middle point y value
   * @param endX - end point x value
   * @param endY - end point y value
   * @param numberOfSegments - (default: 36)
   * @returns the updated Path2.
   */
  public addArcTo(midX: number, midY: number, endX: number, endY: number, numberOfSegments = 36): Path2 {
    if (this.closed) {
      return this
    }
    let startPoint = this._points[this._points.length - 1]
    let midPoint = new Vector2(midX, midY)
    let endPoint = new Vector2(endX, endY)

    let arc = new Arc2(startPoint, midPoint, endPoint)

    let increment = arc.angle.radians() / numberOfSegments
    if (arc.orientation === Orientation.CW) {
      increment *= -1
    }
    let currentAngle = arc.startAngle.radians() + increment

    for (let i = 0; i < numberOfSegments; i++) {
      let x = Math.cos(currentAngle) * arc.radius + arc.centerPoint.x
      let y = Math.sin(currentAngle) * arc.radius + arc.centerPoint.y
      this.addLineTo(x, y)
      currentAngle += increment
    }
    return this
  }
  /**
   * Closes the Path2.
   * @returns the Path2.
   */
  public close(): Path2 {
    this.closed = true
    return this
  }
  /**
   * Gets the sum of the distance between each sequential point in the path
   * @returns the Path2 total length (float).
   */
  public length(): number {
    let result = this._length

    if (!this.closed) {
      let lastPoint = this._points[this._points.length - 1]
      let firstPoint = this._points[0]
      result += firstPoint.subtract(lastPoint).length()
    }
    return result
  }

  /**
   * Gets the points which construct the path
   * @returns the Path2 internal array of points.
   */
  public getPoints(): Vector2[] {
    return this._points
  }

  /**
   * Retreives the point at the distance aways from the starting point
   * @param normalizedLengthPosition - the length along the path to retreive the point from
   * @returns a new Vector2 located at a percentage of the Path2 total length on this path.
   */
  public getPointAtLengthPosition(normalizedLengthPosition: number): Vector2 {
    if (normalizedLengthPosition < 0 || normalizedLengthPosition > 1) {
      return Vector2.Zero()
    }

    let lengthPosition = normalizedLengthPosition * this.length()

    let previousOffset = 0
    for (let i = 0; i < this._points.length; i++) {
      let j = (i + 1) % this._points.length

      let a = this._points[i]
      let b = this._points[j]
      let bToA = b.subtract(a)

      let nextOffset = bToA.length() + previousOffset
      if (lengthPosition >= previousOffset && lengthPosition <= nextOffset) {
        let dir = bToA.normalize()
        let localOffset = lengthPosition - previousOffset

        return new Vector2(a.x + dir.x * localOffset, a.y + dir.y * localOffset)
      }
      previousOffset = nextOffset
    }

    return Vector2.Zero()
  }
}
