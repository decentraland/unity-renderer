import { resolveUrl } from 'atomicHelpers/parseUrl'

import { warn } from '../../logger'
import { createSchemaValidator } from '../helpers/schemaValidator'
import { scene } from '../../renderer'
import { BaseComponent } from '../BaseComponent'
import { SoundComponent } from 'shared/types'

export class Sound extends BaseComponent<SoundComponent> {
  static schemaValidator = createSchemaValidator({
    distanceModel: { default: 'inverse', type: 'string' },
    loop: { default: false, type: 'boolean' },
    src: { type: 'string', default: '' },
    volume: { default: 1, type: 'number' },
    rolloffFactor: { default: 1, type: 'number' },
    playing: { default: true, type: 'boolean' }
  })

  sound: BABYLON.Sound
  soundUrl: string

  _onAfterWorldMatrixUpdate = () => this.sound && this.sound.setPosition(this.entity.absolutePosition)

  transformValue(value: SoundComponent) {
    return Sound.schemaValidator(value)
  }

  detach() {
    this.disposeAudio()
  }

  disposeAudio() {
    if (this.sound) {
      this.entity.unregisterAfterWorldMatrixUpdate(this._onAfterWorldMatrixUpdate)
      this.sound.stop()
      this.sound.dispose()
      delete this.sound
    }
  }

  shouldSceneUpdate(newValue) {
    return true
  }

  update(oldProps: SoundComponent, newProps: SoundComponent) {
    const src = resolveUrl(this.entity.context.internalBaseUrl, newProps.src)

    if (!src) {
      warn('Audio source was not specified with `src`')
      return
    }

    if (!this.sound || this.soundUrl !== src) {
      this.disposeAudio()
      this.sound = new BABYLON.Sound('sound-component', src, scene, null, {
        loop: !!newProps.loop,
        autoplay: !!newProps.playing,
        spatialSound: true,
        distanceModel: newProps.distanceModel,
        rolloffFactor: newProps.rolloffFactor
      })
      this.soundUrl = src

      this.entity.registerAfterWorldMatrixUpdate(this._onAfterWorldMatrixUpdate)
      this._onAfterWorldMatrixUpdate()
    }

    if (this.sound.isPlaying !== newProps.playing) {
      if (newProps.playing) {
        this.sound.play()
      } else {
        this.sound.stop()
      }
    }

    this.sound.setVolume(newProps.volume)
  }
}
