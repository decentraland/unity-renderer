import { Angle } from './Angle'
import { Vector2 } from './Vector2'
import { Orientation } from './types'

/**
 * This represents an arc in a 2d space.
 * @public
 */
export class Arc2 {
  /**
   * Defines the center point of the arc.
   */
  public centerPoint: Vector2
  /**
   * Defines the radius of the arc.
   */
  public radius: number
  /**
   * Defines the angle of the arc (from mid point to end point).
   */
  public angle: Angle
  /**
   * Defines the start angle of the arc (from start point to middle point).
   */
  public startAngle: Angle
  /**
   * Defines the orientation of the arc (clock wise/counter clock wise).
   */
  public orientation: Orientation

  /**
   * Creates an Arc object from the three given points : start, middle and end.
   * @param startPoint - Defines the start point of the arc
   * @param midPoint - Defines the midlle point of the arc
   * @param endPoint - Defines the end point of the arc
   */
  constructor(
    /** Defines the start point of the arc */
    public startPoint: Vector2,
    /** Defines the mid point of the arc */
    public midPoint: Vector2,
    /** Defines the end point of the arc */
    public endPoint: Vector2
  ) {
    let temp = Math.pow(midPoint.x, 2) + Math.pow(midPoint.y, 2)
    let startToMid = (Math.pow(startPoint.x, 2) + Math.pow(startPoint.y, 2) - temp) / 2
    let midToEnd = (temp - Math.pow(endPoint.x, 2) - Math.pow(endPoint.y, 2)) / 2
    let det =
      (startPoint.x - midPoint.x) * (midPoint.y - endPoint.y) - (midPoint.x - endPoint.x) * (startPoint.y - midPoint.y)

    this.centerPoint = new Vector2(
      (startToMid * (midPoint.y - endPoint.y) - midToEnd * (startPoint.y - midPoint.y)) / det,
      ((startPoint.x - midPoint.x) * midToEnd - (midPoint.x - endPoint.x) * startToMid) / det
    )

    this.radius = this.centerPoint.subtract(this.startPoint).length()

    this.startAngle = Angle.BetweenTwoPoints(this.centerPoint, this.startPoint)

    let a1 = this.startAngle.degrees()
    let a2 = Angle.BetweenTwoPoints(this.centerPoint, this.midPoint).degrees()
    let a3 = Angle.BetweenTwoPoints(this.centerPoint, this.endPoint).degrees()

    // angles correction
    if (a2 - a1 > +180.0) {
      a2 -= 360.0
    }
    if (a2 - a1 < -180.0) {
      a2 += 360.0
    }
    if (a3 - a2 > +180.0) {
      a3 -= 360.0
    }
    if (a3 - a2 < -180.0) {
      a3 += 360.0
    }

    this.orientation = a2 - a1 < 0 ? Orientation.CW : Orientation.CCW
    this.angle = Angle.FromDegrees(this.orientation === Orientation.CW ? a1 - a3 : a3 - a1)
  }
}
