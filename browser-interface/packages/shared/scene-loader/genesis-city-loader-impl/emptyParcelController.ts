import type { ContentMapping, Scene } from '@dcl/schemas'
import { EntityType } from '@dcl/schemas'
import { jsonFetch } from 'lib/javascript/jsonFetch'
import type { LoadableScene } from 'shared/types'
import { unsignedCRC32 } from 'lib/encoding/crc32'

const encoder = new TextEncoder()

export class EmptyParcelController {
  emptyScenesPromise: Promise<Record<string, ContentMapping[]>>
  baseUrl: string = ''

  constructor(public options: { rootUrl: string }) {
    this.baseUrl = `${options.rootUrl}loader/empty-scenes/`

    this.emptyScenesPromise = jsonFetch(this.baseUrl + 'mappings.json')
  }

  async createFakeEntity(coordinates: string): Promise<LoadableScene> {
    const emptyScenes = await this.emptyScenesPromise
    const names = Object.keys(emptyScenes)
    const hash = unsignedCRC32(encoder.encode(coordinates))
    const sceneName = names[hash % names.length]
    const entityId = `Qm${coordinates}m`

    const metadata: Scene = {
      display: { title: 'Empty parcel' },
      contact: { name: 'Decentraland' },
      owner: '',
      main: `bin/game.js`,
      tags: [],
      scene: { parcels: [coordinates], base: coordinates }
    }

    return {
      id: entityId,
      baseUrl: this.baseUrl + 'contents/',
      entity: {
        content: emptyScenes[sceneName]!,
        pointers: [coordinates],
        timestamp: Date.now(),
        type: EntityType.SCENE,
        metadata,
        version: 'v3'
      }
    }
  }
}
