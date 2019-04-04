import { DisposableComponent } from './DisposableComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { CLASS_ID, Observer } from 'decentraland-ecs/src'
import { deleteUnusedTextures } from 'engine/renderer/monkeyLoader'
import { validators } from '../helpers/schemaValidator'
import { TextureSamplingMode, TextureWrapping } from 'shared/types'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'

const defaults = {
  samplingMode: TextureSamplingMode.BILINEAR,
  wrap: TextureWrapping.CLAMP,
  hasAlpha: false,
  invertY: true
}

export class Texture extends DisposableComponent {
  texture?: BABYLON.Texture
  meshObserver?: Observer<{ type: string; object: BABYLON.TransformNode }>

  private didLoad = false

  static async getFromComponent(context: SharedSceneContext, id: string): Promise<BABYLON.Texture | null> {
    const component = context.getComponent(id)

    if (component && component instanceof Texture) {
      return component.texture || null
    }

    return null
  }

  onAttach(entity: BaseEntity): void {
    // stub
  }

  onDetach(entity: BaseEntity): void {
    // stub
  }

  dispose() {
    if (this.texture) {
      this.contributions.textures.delete(this.texture)
      delete this.texture
    }
    super.dispose()
    deleteUnusedTextures()
  }

  async updateData(data: any): Promise<void> {
    this.didLoad = false
    const validatedSamplingMode = validators.number(data.samplingMode, defaults.samplingMode)
    const validatedWrap = validators.number(data.wrap, defaults.wrap)
    const samplingMode = Math.max(Math.min(3, Math.floor(validators.int(validatedSamplingMode, 1))), 1)
    const wrap = Math.max(Math.min(2, Math.floor(validators.int(validatedWrap, 0))), 0)

    if (!this.texture) {
      const src = data.src
      const url = src.match(/^base64,/i) && !src.startsWith('data:') ? `data:image/png;${src}` : src

      this.texture = this.context.getTexture(url, {
        noMipMaps: false,
        samplingMode: samplingMode,
        wrapU: wrap,
        wrapV: wrap,
        invertY: false
      })

      this.contributions.textures.add(this.texture)
    }

    if (this.texture) {
      this.texture.updateSamplingMode(samplingMode)
      this.texture.wrapU = wrap
      this.texture.wrapV = wrap

      this.texture.hasAlpha = validators.boolean(data.hasAlpha, defaults.hasAlpha)
    }

    this.didLoad = true
  }

  loadingDone() {
    return this.didLoad
  }
}

DisposableComponent.registerClassId(CLASS_ID.TEXTURE, Texture)
