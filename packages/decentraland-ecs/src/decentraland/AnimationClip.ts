import { ObservableComponent } from '../ecs/Component'

export type AnimationParams = {
  clip?: string
  loop?: boolean
  speed?: number
  weight?: number
}

const defaultParams: Required<Pick<AnimationParams, 'loop' | 'speed' | 'weight'>> = {
  loop: true,
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
  @ObservableComponent.field
  public clip!: string

  /**
   * Does the animation loop?, default: true
   */
  @ObservableComponent.field
  public loop: boolean = defaultParams.loop

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

  constructor(clip: string, params: AnimationParams = defaultParams) {
    super()
    this.setParams({ clip, ...params })
  }

  /**
   * Sets the clip parameters
   */
  setParams(params: AnimationParams) {
    this.clip = params.clip || this.clip
    this.loop = params.loop !== undefined ? params.loop : this.loop
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

export default AnimationClip
