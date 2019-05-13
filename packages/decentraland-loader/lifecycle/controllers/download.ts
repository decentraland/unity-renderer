import { ILand, MappingsResponse, IScene, normalizeContentMappings } from 'shared/types'
import { jsonFetch } from 'atomicHelpers/jsonFetch'
import { error } from 'util'

export class SceneDataDownloadManager {
  options: {
    contentServer: string
  }

  constructor(options: { contentServer: string }) {
    this.options = options
  }
  cache: Map<string, Promise<ILand | null>> = new Map()
  parcelData: Map<string, ILand> = new Map()
  positionData: Map<string, ILand> = new Map()

  async loadParcelData(pos: string): Promise<ILand | null> {
    const nw = pos.split(',').map($ => parseInt($, 10))
    const responseContent = await fetch(
      this.options.contentServer + `/mappings?nw=${nw[0]},${nw[1]}&se=${nw[0] + 1},${nw[1] - 1}`
    )

    if (!responseContent.ok) {
      error(`Error in ${this.options.contentServer}/mappings response!`, responseContent)
      throw new Error(`Error in ${this.options.contentServer}/mappings response!`)
    } else {
      const contents = (await responseContent.json()) as MappingsResponse[]
      if (!contents.length) {
        return null
      }
      for (const content of contents) {
        content.contents = normalizeContentMappings(content.contents)
        const sceneJsonMapping = content.contents.find($ => $.file === 'scene.json')

        if (!sceneJsonMapping) return null

        const baseUrl = this.options.contentServer + '/contents/'
        const scene = (await jsonFetch(baseUrl + sceneJsonMapping.hash)) as IScene

        const data: ILand = {
          baseUrl,
          scene,
          mappingsResponse: content
        }

        this.parcelData.set(sceneJsonMapping.hash, data)
        for (const posParcel of data.scene.scene.parcels) {
          this.positionData.set(posParcel, data)
        }
      }
      return this.positionData.get(pos)!
    }
  }

  async getParcelDataByCID(cid: string): Promise<ILand | null> {
    return this.parcelData.get(cid) || null
  }

  async getParcelData(position: string): Promise<ILand | null> {
    if (this.positionData.has(position)) {
      return this.positionData.get(position)!
    }
    if (this.cache.has(position)) {
      return this.cache.get(position)!
    }

    const promise = this.loadParcelData(position)

    this.cache.set(position, promise)

    return promise
  }
}
