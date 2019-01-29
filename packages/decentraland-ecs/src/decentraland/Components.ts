import { Component, ObservableComponent, DisposableComponent } from '../ecs/Component'
import { Vector3, Quaternion, Matrix, MathTmp, Color3 } from './math'
import { AnimationClip } from './AnimationClip'
import { uuid } from '../ecs/helpers'

/**
 * @public
 */
export interface IInteractionEvent {
  pointerId: number
}

export type TranformConstructorArgs = {
  position?: Vector3
  rotation?: Quaternion
  scale?: Vector3
}

/**
 * @internal
 */
export enum CLASS_ID {
  TRANSFORM = 1,
  UUID_CALLBACK = 8,
  BOX_SHAPE = 16,
  SPHERE_SHAPE = 17,
  PLANE_SHAPE = 18,
  CONE_SHAPE = 19,
  CYLINDER_SHAPE = 20,
  TEXT_SHAPE = 21,

  NFT_SHAPE = 22,
  UI_WORLD_SPACE_SHAPE = 23,
  UI_SCREEN_SPACE_SHAPE = 24,
  UI_CONTAINER_RECT = 25,
  UI_CONTAINER_STACK = 26,
  UI_TEXT_SHAPE = 27,
  UI_INPUT_TEXT_SHAPE = 28,
  UI_IMAGE_SHAPE = 29,
  UI_SLIDER_SHAPE = 30,
  CIRCLE_SHAPE = 31,
  BILLBOARD = 32,

  ANIMATION = 33,

  GLTF_SHAPE = 54,
  OBJ_SHAPE = 55,
  BASIC_MATERIAL = 64,
  PRB_MATERIAL = 65,

  HIGHLIGHT_ENTITY = 66,
  SOUND = 67,

  AUDIO_CLIP = 200,
  AUDIO_SOURCE = 201,
  GIZMOS = 203
}

/**
 * @public
 */
@Component('engine.transform', CLASS_ID.TRANSFORM)
export class Transform extends ObservableComponent {
  @ObservableComponent.field
  position!: Vector3

  @ObservableComponent.field
  rotation!: Quaternion

  @ObservableComponent.field
  scale!: Vector3

  constructor(args: TranformConstructorArgs = {}) {
    super()
    this.position = args.position || Vector3.Zero()
    this.rotation = args.rotation || Quaternion.Identity
    this.scale = args.scale || new Vector3(1, 1, 1)
  }

  /**
   * @public
   * The rotation as Euler angles in degrees.
   */
  get eulerAngles() {
    return this.rotation.eulerAngles
  }

  /**
   * @public
   * Rotates the transform so the forward vector points at target's current position.
   */
  lookAt(target: Vector3, worldUp: Vector3 = MathTmp.staticUp) {
    const result = new Matrix()
    Matrix.LookAtLHToRef(this.position, target, worldUp, result)
    result.invert()
    Quaternion.FromRotationMatrixToRef(result, this.rotation)
  }

  /**
   * @public
   * Applies a rotation of euler angles around the x, y and z axis.
   */
  rotate(axis: Vector3, angle: number) {
    this.rotation.multiplyInPlace(this.rotation.angleAxis(angle, axis))
  }

  /**
   * @public
   * Moves the transform in the direction and distance of translation.
   */
  translate(vec: Vector3) {
    this.position.addInPlace(vec)
  }
}

/**
 * Billboard defines a behavior that makes the entity face the camera in any moment.
 * @public
 */
@Component('engine.billboard', CLASS_ID.BILLBOARD)
export class Billboard extends ObservableComponent {
  @ObservableComponent.field
  x: boolean = true

  @ObservableComponent.field
  y: boolean = true

  @ObservableComponent.field
  z: boolean = true

  constructor(x: boolean = true, y: boolean = true, z: boolean = true) {
    super()
    this.x = x
    this.y = y
    this.z = z
  }
}

/**
 * @public
 */
export class Shape extends ObservableComponent {
  /**
   * Set to true to turn on the collider for the entity.
   * @alpha
   */
  @ObservableComponent.field
  withCollisions: boolean = false

  /**
   * Defines if the entity and its children should be rendered
   * @alpha
   */
  @ObservableComponent.field
  visible: boolean = true
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.BOX_SHAPE)
export class BoxShape extends Shape {}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.SPHERE_SHAPE)
export class SphereShape extends Shape {}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.CIRCLE_SHAPE)
export class CircleShape extends Shape {
  @ObservableComponent.field
  segments?: number

  @ObservableComponent.field
  arc?: number
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.PLANE_SHAPE)
export class PlaneShape extends Shape {
  /**
   * Sets the horizontal length of the plane. Defaults to 1.
   */
  @ObservableComponent.field
  width: number = 1

  /**
   * Sets the vertical length of the plane. Defaults to 1.
   */
  @ObservableComponent.field
  height: number = 1

  /**
   * Sets the UV coordinates for the plane.
   * Used to map specific pieces of a Material's texture into the plane's geometry.
   */
  @ObservableComponent.field
  uvs: number[] = []
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.CONE_SHAPE)
export class ConeShape extends Shape {
  /**
   * The radius of the top of a truncated cone. Defaults to 0.
   */
  @ObservableComponent.field
  radiusTop: number = 0

  /**
   * The radius of the base of the cone. Defaults to 1.
   */
  @ObservableComponent.field
  radiusBottom: number = 1

  /**
   * Sets the number of rings along the cone height (positive integer). Defaults to 1.
   */
  @ObservableComponent.field
  segmentsHeight: number = 1

  /**
   * Sets the number of cone sides (positive integer). Defaults to 36.
   */
  @ObservableComponent.field
  segmentsRadial: number = 36

  /**
   * Adds two extra faces per subdivision to enclose the cone around its height axis.
   * Defaults to false.
   */
  @ObservableComponent.field
  openEnded: boolean = false

  /**
   * Sets the radius of the top and bottom caps at once.
   *
   * Properties `radiusTop` and `radiusBottom` are prioritized over this one.
   */
  @ObservableComponent.field
  radius: number | null = null

  /**
   * Sets the ratio (max 1) to apply to the circumference to slice the cone. Defaults to 360.
   */
  @ObservableComponent.field
  arc: number = 360
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.CYLINDER_SHAPE)
export class CylinderShape extends Shape {
  /**
   * The radius of the top of the cylinder. Defaults to 0.
   */
  @ObservableComponent.field
  radiusTop: number = 0

  /**
   * The radius of the base of the cylinder. Defaults to 1.
   */
  @ObservableComponent.field
  radiusBottom: number = 1

  /**
   * Sets the number of rings along the cylinder height (positive integer). Defaults to 1.
   */
  @ObservableComponent.field
  segmentsHeight: number = 1

  /**
   * Sets the number of cylinder sides (positive integer). Defaults to 36.
   */
  @ObservableComponent.field
  segmentsRadial: number = 36

  /**
   * Adds two extra faces per subdivision to enclose the cylinder around its height axis.
   * Defaults to false.
   */
  @ObservableComponent.field
  openEnded: boolean = false

  /**
   * Sets the radius of the top and bottom caps at once.
   *
   * Properties `radiusTop` and `radiusBottom` are prioritized over this one.
   */
  @ObservableComponent.field
  radius: number | null = null

  /**
   * Sets the ratio (max 1) to apply to the circumference to slice the cylinder. Defaults to 360.
   */
  @ObservableComponent.field
  arc: number = 360
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.GLTF_SHAPE)
export class GLTFShape extends Shape {
  @Shape.readonly
  readonly src!: string

  constructor(src: string) {
    super()
    this.src = src
  }
}

/**
 * @public
 */
@Component('engine.animator', CLASS_ID.ANIMATION)
export class Animator extends Shape {
  @ObservableComponent.readonly
  private states: AnimationClip[] = []

  /**
   * Adds an AnimationClip to the animation lists.
   */
  addClip(clip: AnimationClip) {
    this.states.push(clip)
    clip.onChange(() => {
      this.dirty = true
    })
  }

  /**
   * Gets the animation clip instance for the specified clip name.
   * If the clip doesn't exist a new one will be created.
   */
  getClip(clipName: string): AnimationClip {
    for (let i = 0; i < this.states.length; i++) {
      const clip = this.states[i]
      if (clip.clip === clipName) {
        return clip
      }
    }

    const newClip = new AnimationClip(clipName)
    this.addClip(newClip)
    return newClip
  }
}

/**
 * @public
 */
@DisposableComponent('engine.shape', CLASS_ID.OBJ_SHAPE)
export class OBJShape extends Shape {
  @ObservableComponent.readonly
  readonly src!: string

  constructor(src: string) {
    super()
    this.src = src
  }
}

/**
 * @public
 */
@Component('engine.text', CLASS_ID.TEXT_SHAPE)
export class TextShape extends Shape {
  @ObservableComponent.field
  outlineWidth: number = 0

  @ObservableComponent.field
  outlineColor: Color3 = new Color3(1, 1, 1)

  @ObservableComponent.field
  color: Color3 = new Color3(1, 1, 1)

  @ObservableComponent.field
  fontFamily: string = 'Arial'

  @ObservableComponent.field
  fontSize: number = 100

  @ObservableComponent.field
  fontWeight: string = 'normal'

  @ObservableComponent.field
  opacity: number = 1.0

  @ObservableComponent.field
  value: string = ''

  @ObservableComponent.field
  lineSpacing: string = '0px'

  @ObservableComponent.field
  lineCount: number = 0

  @ObservableComponent.field
  resizeToFit: boolean = false

  @ObservableComponent.field
  textWrapping: boolean = false

  @ObservableComponent.field
  shadowBlur: number = 0

  @ObservableComponent.field
  shadowOffsetX: number = 0

  @ObservableComponent.field
  shadowOffsetY: number = 0

  @ObservableComponent.field
  shadowColor: Color3 = new Color3(1, 1, 1)

  @ObservableComponent.field
  zIndex: number = 0

  @ObservableComponent.field
  hAlign: string = 'center'

  @ObservableComponent.field
  vAlign: string = 'center'

  @ObservableComponent.field
  width: number = 1

  @ObservableComponent.field
  height: number = 1

  @ObservableComponent.field
  paddingTop: number = 0

  @ObservableComponent.field
  paddingRight: number = 0

  @ObservableComponent.field
  paddingBottom: number = 0

  @ObservableComponent.field
  paddingLeft: number = 0

  @ObservableComponent.field
  isPickable: boolean = false

  @ObservableComponent.field
  billboard: boolean = false

  constructor(value?: string) {
    super()

    if (value) {
      this.value = value
    }
  }
}

/**
 * @public
 */
@DisposableComponent('engine.material', CLASS_ID.PRB_MATERIAL)
export class Material extends ObservableComponent {
  /**
   * Opacity level between 0 and 1.
   * Defaults to 1.
   */
  @ObservableComponent.field
  alpha?: number

  /**
   * AKA Diffuse Color in other nomenclature.
   * Defaults to #CCCCCC.
   */
  @ObservableComponent.field
  albedoColor?: Color3

  /**
   * The color emitted from the material.
   * Defaults to black.
   */
  @ObservableComponent.field
  emissiveColor?: Color3

  /**
   * Specifies the metallic scalar of the metallic/roughness workflow.
   * Can also be used to scale the metalness values of the metallic texture.
   * Defaults to  0.5.
   */
  @ObservableComponent.field
  metallic?: number

  /**
   * Specifies the roughness scalar of the metallic/roughness workflow.
   * Can also be used to scale the roughness values of the metallic texture.
   * Defaults to  0.5.
   */
  @ObservableComponent.field
  roughness?: number

  /**
   * AKA Diffuse Color in other nomenclature.
   * Defaults to black.
   */
  @ObservableComponent.field
  ambientColor?: Color3

  /**
   * The color reflected from the material.
   * Defaults to white.
   */
  @ObservableComponent.field
  reflectionColor?: Color3

  /**
   * AKA Specular Color in other nomenclature.
   * Defaults to white.
   */
  @ObservableComponent.field
  reflectivityColor?: Color3

  /**
   * Intensity of the direct lights e.g. the four lights available in scene.
   * This impacts both the direct diffuse and specular highlights.
   * Defaults to 1.
   */
  @ObservableComponent.field
  directIntensity?: number

  /**
   * AKA Glossiness in other nomenclature.
   * Defaults to 1.
   */
  @ObservableComponent.field
  microSurface?: number

  /**
   * Intensity of the emissive part of the material.
   * This helps controlling the emissive effect without modifying the emissive color.
   * Defaults to 1.
   */
  @ObservableComponent.field
  emissiveIntensity?: number

  /**
   * Intensity of the environment e.g. how much the environment will light the object
   * either through harmonics for rough material or through the refelction for shiny ones.
   * Defaults to 1.
   */
  @ObservableComponent.field
  environmentIntensity?: number

  /**
   * This is a special control allowing the reduction of the specular highlights coming from the
   * four lights of the scene. Those highlights may not be needed in full environment lighting.
   * Defaults to 1.
   */
  @ObservableComponent.field
  specularIntensity?: number

  /**
   * Texture applied as material.
   */
  @ObservableComponent.field
  albedoTexture?: string

  /**
   * Texture applied as opacity. Default: the same texture used in albedoTexture.
   */
  @ObservableComponent.field
  alphaTexture?: string

  /**
   * Emissive texture.
   */
  @ObservableComponent.field
  emissiveTexture?: string

  /**
   * Stores surface normal data used to displace a mesh in a texture.
   */
  @ObservableComponent.field
  bumpTexture?: string

  /**
   * Stores the refracted light information in a texture.
   */
  @ObservableComponent.field
  refractionTexture?: string

  /**
   * If sets to true, disables all the lights affecting the material.
   * Defaults to false.
   */
  @ObservableComponent.field
  disableLighting?: boolean

  /**
   * Sets the transparency mode of the material.
   * Defauts to 0.
   *
   * | Value | Type                                |
   * | ----- | ----------------------------------- |
   * | 0     | OPAQUE  (default)                   |
   * | 1     | ALPHATEST                           |
   * | 2     | ALPHABLEND                          |
   * | 3     | ALPHATESTANDBLEND                   |
   */
  @ObservableComponent.field
  transparencyMode?: number

  /**
   * Does the albedo texture has alpha?
   * Defaults to false.
   */
  @ObservableComponent.field
  hasAlpha?: boolean
}

/**
 * @public
 */
@DisposableComponent('engine.material', CLASS_ID.BASIC_MATERIAL)
export class BasicMaterial extends ObservableComponent {
  /**
   * The source of the texture image.
   */
  @ObservableComponent.field
  texture: string = ''

  /**
   * A number between 0 and 1.
   * Any pixel with an alpha lower than this value will be shown as transparent.
   */
  @ObservableComponent.field
  alphaTest: number = 0.5

  /**
   * Enables crisper images based on the provided sampling mode.
   * | Value | Type      |
   * |-------|-----------|
   * |     1 | NEAREST   |
   * |     2 | BILINEAR  |
   * |     3 | TRILINEAR |
   */
  @ObservableComponent.field
  samplingMode: number = 2

  /**
   * Enables texture wrapping for this material.
   * | Value | Type      |
   * |-------|-----------|
   * |     1 | CLAMP     |
   * |     2 | WRAP      |
   * |     3 | MIRROR    |
   */
  @ObservableComponent.field
  wrap: number = 0
}

/**
 * @public
 */
export class OnUUIDEvent extends ObservableComponent {
  readonly type: string | undefined

  readonly uuid: string = uuid()

  @ObservableComponent.field
  callback!: (event: any) => void

  constructor(callback: (event: IInteractionEvent) => void) {
    super()

    if (!callback || !('apply' in callback) || !('call' in callback)) {
      debugger
      throw new Error('Callback is not a function')
    }

    this.callback = callback
  }

  toJSON() {
    return { uuid: this.uuid, type: this.type }
  }
}

/**
 * @public
 */
@Component('engine.onClick', CLASS_ID.UUID_CALLBACK)
export class OnClick extends OnUUIDEvent {
  @ObservableComponent.readonly
  readonly type: string = 'onClick'
}

/**
 * @public
 */
@Component('engine.onChange', CLASS_ID.UUID_CALLBACK)
export class OnChanged extends OnUUIDEvent {
  @ObservableComponent.readonly
  readonly type: string = 'onChange'
}

/**
 * @public
 */
@Component('engine.onFocus', CLASS_ID.UUID_CALLBACK)
export class OnFocus extends OnUUIDEvent {
  @ObservableComponent.readonly
  readonly type: string = 'onFocus'
}

/**
 * @public
 */
@Component('engine.onBlur', CLASS_ID.UUID_CALLBACK)
export class OnBlur extends OnUUIDEvent {
  @ObservableComponent.readonly
  readonly type: string = 'onBlur'
}
