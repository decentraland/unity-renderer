import { Vector3, Quaternion } from './math'
import { DecentralandInterface } from './Types'

declare let dcl: DecentralandInterface | void

/**
 * @public
 */
export class Camera {
  // @internal
  private static _instance: Camera

  static get instance(): Camera {
    if (!Camera._instance) {
      Camera._instance = new Camera()
    }
    return Camera._instance
  }

  public readonly position: Vector3 = new Vector3()
  public readonly rotation: Quaternion = new Quaternion()

  // @internal
  private lastEventPosition: { x: number; y: number; z: number } = { x: 0, y: 0, z: 0 }

  // @internal
  private lastEventRotation: { x: number; y: number; z: number; w: number } = { x: 0, y: 0, z: 0, w: 1.0 }

  constructor() {
    if (typeof dcl !== 'undefined') {
      dcl.subscribe('positionChanged')
      dcl.subscribe('rotationChanged')

      dcl.onEvent(event => {
        if (event.type === 'positionChanged') {
          this.positionChanged(event.data as any)
        } else if (event.type === 'rotationChanged') {
          this.rotationChanged(event.data as any)
        }
      })
    }

    Object.defineProperty(this.position, 'x', {
      get: () => this.lastEventPosition.x
    })

    Object.defineProperty(this.position, 'y', {
      get: () => this.lastEventPosition.y
    })

    Object.defineProperty(this.position, 'z', {
      get: () => this.lastEventPosition.z
    })

    Object.defineProperty(this.rotation, 'x', {
      get: () => this.lastEventRotation.x
    })

    Object.defineProperty(this.rotation, 'y', {
      get: () => this.lastEventRotation.y
    })

    Object.defineProperty(this.rotation, 'z', {
      get: () => this.lastEventRotation.z
    })

    Object.defineProperty(this.rotation, 'w', {
      get: () => this.lastEventRotation.w
    })
  }

  // @internal
  private positionChanged(e: { position: { x: number; y: number; z: number } }) {
    this.lastEventPosition = e.position
  }

  // @internal
  private rotationChanged(e: { quaternion: { x: number; y: number; z: number; w: number } }) {
    this.lastEventRotation = e.quaternion
  }
}
