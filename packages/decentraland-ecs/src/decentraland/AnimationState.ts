import { ObservableComponent } from '../ecs/Component'
import { newId } from '../ecs/helpers'

export type AnimationParams = {
  looping?: boolean
  speed?: number
  weight?: number
}

const defaultParams: Required<Pick<AnimationParams, 'looping' | 'speed' | 'weight'>> = {
  looping: true,
  speed: 1.0,
  weight: 1.0
}

/**
 * @public
 */
export class AnimationState extends ObservableComponent {
  // @internal
  public isAnimationClip: boolean = true

  /**
   * Name of the animation in the model
   */
  @ObservableComponent.readonly
  public readonly clip: string

  /**
   * Does the animation loop?, default: true
   */
  @ObservableComponent.field
  public looping: boolean = defaultParams.looping

  /**
   * Weight of the animation, values from 0 to 1, used to blend several animations. default: 1
   */
  @ObservableComponent.field
  public weight: number = defaultParams.weight

  /**
   * Is the animation playing? default: true
   */
  @ObservableComponent.field
  public playing: boolean = false

  /**
   * Does any anyone asked to reset the animation? default: false
   */
  @ObservableComponent.field
  public shouldReset: boolean = false

  /**
   * The animation speed
   */
  @ObservableComponent.field
  public speed: number = defaultParams.speed

  // @internal
  @ObservableComponent.readonly
  readonly name: string = newId('AnimClip')

  constructor(clip: string, params: AnimationParams = defaultParams) {
    super()
    this.clip = clip
    this.setParams({ ...params })
  }

  /**
   * Sets the clip parameters
   */
  setParams(params: AnimationParams) {
    this.looping = params.looping !== undefined ? params.looping : this.looping
    this.speed = params.speed || this.speed
  }

  toJSON() {
    const ret = JSON.parse(JSON.stringify(super.toJSON()))
    if (this.shouldReset) {
      this.shouldReset = false
    }
    return ret
  }

  /**
   * Starts the animation
   */
  play() {
    this.playing = true
  }

  /**
   * Pauses the animation
   */
  pause() {
    this.playing = false
  }

  /**
   * Resets the animation state to the frame 0
   */
  reset() {
    this.shouldReset = true
  }

  /**
   * Resets and pauses the animation
   */
  stop() {
    this.reset()
    this.pause()
  }
}
