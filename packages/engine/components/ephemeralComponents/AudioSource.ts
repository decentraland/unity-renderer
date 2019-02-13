import { BaseComponent } from '../BaseComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { AudioClip } from '../disposableComponents/AudioClip'
import future from 'fp-future'
import { scene } from '../../renderer/init'

const defaultValue = {
  loop: true,
  volume: 1,
  playing: true,
  pitch: 1,
  audioClipId: null,
  clip: null as AudioClip | null
}

export class AudioSource extends BaseComponent<typeof defaultValue> {
  sound = future<BABYLON.Sound>()

  transformValue(value: typeof defaultValue) {
    let clip = this.entity.context.disposableComponents.get(value.audioClipId) as AudioClip

    if (!(clip instanceof AudioClip)) {
      clip = null
    }

    // TODO: Sanitize this

    return {
      loop: value.loop,
      volume: value.volume,
      playing: value.playing,
      pitch: value.pitch,
      audioClipId: value.audioClipId,
      clip,
      nonce: (value as any).nonce
    }
  }

  disposeSound() {
    this.sound
      .then($ => {
        $.stop()
        $.dispose()
      })
      .catch(() => void 0)

    this.sound = future()
  }

  updateSound() {
    if (!this.sound.isPending) {
      this.sound
        .then($ => {
          $.updateOptions({
            loop: this.value.loop
          })
          $.setPosition(this.entity.absolutePosition)
          if (!this.value.loop && this.value.playing && $.isPlaying) {
            $.stop()
          }
          if (!$.isPlaying && this.value.playing) {
            $.play()
          } else if ($.isPlaying && !this.value.playing) {
            $.stop()
          }
        })
        .catch(() => void 0)
    }
  }

  update(oldProperties: typeof defaultValue, currentProperties: typeof defaultValue) {
    if (!oldProperties || oldProperties.clip !== currentProperties.clip) {
      this.disposeSound()
      if (currentProperties.clip) {
        currentProperties.clip.arrayBuffer
          .then($ => {
            const sound = new BABYLON.Sound(
              'AudioSource',
              $,
              scene,
              () => {
                this.sound.resolve(sound)
                this.sound
                  .then(() => {
                    this.updateSound()
                  })
                  .catch(() => void 0)
              },
              {
                autoplay: false,
                spatialSound: true,
                distanceModel: 'exponential',
                rolloffFactor: 1
              }
            )
          })
          .catch(() => void 0)
      }
    } else {
      this.sound.then(() => this.updateSound()).catch(() => void 0)
    }
  }

  attach(entity: BaseEntity) {
    super.attach(entity)
    entity.registerAfterWorldMatrixUpdate(this.updatePosition)
  }

  detach() {
    super.detach()
    this.disposeSound()
    this.entity.unregisterAfterWorldMatrixUpdate(this.updatePosition)
  }

  private updatePosition = () => {
    this.sound.then($ => $.setPosition(this.entity.absolutePosition)).catch(() => void 0)
  }
}
