import { DisposableComponent, ObservableComponent, Component, getComponentId } from '../ecs/Component'
import { CLASS_ID } from './Components'

/**
 * @public
 */
@DisposableComponent('engine.AudioClip', CLASS_ID.AUDIO_CLIP)
export class AudioClip extends ObservableComponent {
  @ObservableComponent.readonly
  readonly url: string

  /**
   * Is this clip looping by default?
   */
  @ObservableComponent.field
  loop: boolean = false

  // @internal
  @ObservableComponent.field
  loadingCompleteEventId?: string

  /**
   * Clip's master volume. This volume affects all the AudioSources.
   * Valid ranges from 0 to 1
   */
  @ObservableComponent.field
  volume: number = 1

  constructor(url: string) {
    super()
    this.url = url
  }
}

/**
 * @public
 */
@Component('engine.AudioSource', CLASS_ID.AUDIO_SOURCE)
export class AudioSource extends ObservableComponent {
  @ObservableComponent.readonly
  readonly audioClipId: string

  /**
   * Is this clip looping by default?
   */
  @ObservableComponent.field
  loop: boolean = false

  /**
   * Clip's master volume. This volume affects all the AudioSources.
   * Valid ranges from 0 to 1
   */
  @ObservableComponent.field
  volume: number = 1

  /**
   * Is this AudioSource playing?
   */
  @ObservableComponent.field
  playing: boolean = false

  /**
   * Pitch, default: 1.0, range from 0.0 to MaxFloat
   */
  @ObservableComponent.field
  pitch: number = 1.0

  constructor(public readonly audioClip: AudioClip) {
    super()
    if (!(audioClip instanceof AudioClip)) {
      throw new Error(`Trying to create AudioSource(AudioClip) with an invalid AudioClip`)
    }
    this.audioClipId = getComponentId(audioClip as any)
  }

  /**
   * Disables the looping and plays the current source once.
   * If the sound was playing, it stops and starts over.
   */
  playOnce() {
    this.playing = true
    this.dirty = true
    this.data.nonce = Math.random()
    return this
  }
}
