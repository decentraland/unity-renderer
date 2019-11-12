import { Vector3, Quaternion, ReadOnlyVector3, ReadOnlyQuaternion } from './math'
import { DecentralandInterface, IEvents } from './Types'

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

  /** Camera position, relative to the parcel. */
  public readonly position: Vector3 = new Vector3()
  /** Camera rotation */
  public readonly rotation: Quaternion = new Quaternion()

  /** Feet position, relative to the parcel.  */
  public readonly feetPosition: Vector3 = new Vector3()
  /** Camera position, absolute. */
  public readonly worldPosition: Vector3 = new Vector3()

  /** Player height. */
  get playerHeight(): number {
    return this._playerHeight
  }

  // @internal
  private lastEventPosition: ReadOnlyVector3 = { x: 0, y: 0, z: 0 }
  // @internal
  private lastEventWorldPosition: ReadOnlyVector3 = { x: 0, y: 0, z: 0 }

  // @internal
  private lastEventRotation: ReadOnlyQuaternion = { x: 0, y: 0, z: 0, w: 1.0 }

  // @internal
  private _playerHeight: number = 1.6

  constructor() {
    if (typeof dcl !== 'undefined') {
      dcl.subscribe('positionChanged')
      dcl.subscribe('rotationChanged')

      dcl.onEvent(event => {
        switch (event.type) {
          case 'positionChanged':
            this.positionChanged(event.data as any)
            break
          case 'rotationChanged':
            this.rotationChanged(event.data as any)
            break
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

    Object.defineProperty(this.worldPosition, 'x', {
      get: () => this.lastEventWorldPosition.x
    })

    Object.defineProperty(this.worldPosition, 'y', {
      get: () => this.lastEventWorldPosition.y
    })

    Object.defineProperty(this.worldPosition, 'z', {
      get: () => this.lastEventWorldPosition.z
    })

    Object.defineProperty(this.feetPosition, 'x', {
      get: () => this.lastEventPosition.x
    })

    Object.defineProperty(this.feetPosition, 'y', {
      get: () => this.lastEventPosition.y - this.playerHeight
    })

    Object.defineProperty(this.feetPosition, 'z', {
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
  private positionChanged(e: IEvents['positionChanged']) {
    this.lastEventPosition = e.position
    this.lastEventWorldPosition = e.cameraPosition
    this._playerHeight = e.playerHeight
  }

  // @internal
  private rotationChanged(e: IEvents['rotationChanged']) {
    this.lastEventRotation = e.quaternion
  }
}
