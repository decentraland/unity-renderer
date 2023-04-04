import { Entity } from '@dcl/schemas'
import future, { IFuture } from 'fp-future'
import { LoadableScene } from 'shared/types'
import { EmptyParcelController } from './emptyParcelController'

export class SceneDataDownloadManager {
  pointerToEntity: Map<string, IFuture<LoadableScene | null>> = new Map()
  idToEntity: Map<string, IFuture<LoadableScene | null>> = new Map()

  constructor(
    public options: {
      contentServer: string
      emptyParcelController?: EmptyParcelController
    }
  ) {}

  async resolveEntitiesByPointer(pointers: string[]): Promise<Set<LoadableScene>> {
    const futures: Promise<LoadableScene | null>[] = []

    const missingPointers: string[] = []

    for (const pointer of pointers) {
      let promise: IFuture<LoadableScene | null>

      if (this.pointerToEntity.has(pointer)) {
        promise = this.pointerToEntity.get(pointer)!
      } else {
        promise = future<LoadableScene | null>()
        this.pointerToEntity.set(pointer, promise)
        missingPointers.push(pointer)
      }

      futures.push(promise.then((entity) => entity))
    }

    if (missingPointers.length > 0) {
      const activeEntities = this.options.contentServer + '/entities/active'

      const scenesResponse = await fetch(activeEntities, {
        method: 'post',
        headers: { 'content-type': 'application/json' },
        body: JSON.stringify({ pointers: missingPointers })
      })

      if (scenesResponse.ok) {
        const entities: Entity[] = await scenesResponse.json()
        // resolve promises
        for (const entity of entities) {
          const entityWithBaseUrl: LoadableScene = {
            id: entity.id,
            baseUrl: (entity as any).baseUrl || this.options.contentServer + '/contents/',
            entity
          }
          for (const tile of entity.pointers) {
            if (this.pointerToEntity.has(tile)) {
              const promise = this.pointerToEntity.get(tile)
              promise!.resolve(entityWithBaseUrl)
            } else {
              // if we get back a pointer/tile that was not pending => create the future and resolve
              const promise = future<LoadableScene | null>()
              promise.resolve(entityWithBaseUrl)
              this.pointerToEntity.set(tile, promise)
            }
          }

          const pendingSceneData: IFuture<LoadableScene | null> = this.idToEntity.get(entity.id) || future()

          if (pendingSceneData.isPending) {
            pendingSceneData.resolve(entityWithBaseUrl)
          }

          if (!this.idToEntity.has(entity.id)) {
            this.idToEntity.set(entity.id, pendingSceneData)
          }
        }

        // missing tiles will correspond to empty parcels
        for (const pointer of missingPointers) {
          const promise = this.pointerToEntity.get(pointer)
          if (promise?.isPending) {
            if (this.options.emptyParcelController) {
              const emptyParcel = await this.options.emptyParcelController.createFakeEntity(pointer)
              promise!.resolve(emptyParcel)
            } else {
              promise!.resolve(null)
            }
          }
        }
      }
    }

    const ret = await Promise.all(futures)

    return new Set(ret.filter(Boolean) as LoadableScene[])
  }
}
