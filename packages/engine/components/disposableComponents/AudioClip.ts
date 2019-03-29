import { DisposableComponent } from './DisposableComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { CLASS_ID } from 'decentraland-ecs/src'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import future from 'fp-future'

const defaults = {
  loop: false,
  volume: 1,
  url: null as string | null
}

export class AudioClip extends DisposableComponent {
  readonly arrayBuffer = future<ArrayBuffer>()
  loadingDonePrivate: boolean = false
  data = Object.create(defaults) as typeof defaults

  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)
    this.contributions.audioClips.add(this)

    this.arrayBuffer.catch($ => this.context.logger.error($))
  }

  onAttach(_: BaseEntity): void {
    // noop
  }

  onDetach(_: BaseEntity): void {
    // noop
  }

  async updateData(data: typeof defaults): Promise<void> {
    this.loadingDonePrivate = false
    this.data.volume = data.volume || 1
    this.data.loop = !!data.loop
    if (data.url && !this.data.url) {
      this.data.url = data.url

      this.context
        .getArrayBuffer(data.url)
        .then(ab => {
          this.arrayBuffer.resolve(ab)
          this.loadingDonePrivate = true
        })
        .catch($ => {
          this.arrayBuffer.reject($)
          this.loadingDonePrivate = true
        })
    }
  }

  loadingDone() {
    return this.loadingDonePrivate
  }
}

DisposableComponent.registerClassId(CLASS_ID.AUDIO_CLIP, AudioClip)
