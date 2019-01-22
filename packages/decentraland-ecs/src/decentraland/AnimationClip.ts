import { ObservableComponent } from '../ecs/Component'
import { uuid } from '../ecs/helpers'

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
export class AnimationClip extends ObservableComponent {
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
   * The animation speed
   */
  @ObservableComponent.field
  public speed: number = defaultParams.speed

  // @internal
  @ObservableComponent.readonly
  readonly name: string = uuid()

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
}
