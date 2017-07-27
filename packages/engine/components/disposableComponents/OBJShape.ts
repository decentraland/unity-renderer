import { DisposableComponent, BasicShape } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { cleanupAssetContainer, processColliders } from 'engine/entities/utils/processModels'
import { resolveUrl } from 'atomicHelpers/parseUrl'
import { scene } from 'engine/renderer'
import { probe } from 'engine/renderer/ambientLights'
import { DEBUG } from 'config'

export class OBJShape extends DisposableComponent {
  assetContainerEntity = new Map<string, BABYLON.AssetContainer>()
  src: string | null = null
  loadingDone = false

  onAttach(entity: BaseEntity): void {
    if (this.src) {
      const url = resolveUrl(this.context.internalBaseUrl, this.src)
      const baseUrl = url.substr(0, url.lastIndexOf('/') + 1)

      BABYLON.SceneLoader.LoadAssetContainer(
        baseUrl,
        url.replace(baseUrl, ''),
        scene,
        assetContainer => {
          if (this.assetContainerEntity.has(entity.uuid)) {
            this.onDetach(entity)
          }

          assetContainer.meshes.forEach(mesh => {
            if (!assetContainer.materials.includes(mesh.material)) {
              assetContainer.materials.push(mesh.material)
            }
          })

          assetContainer.materials.forEach((material: BABYLON.Material | BABYLON.PBRMaterial) => {
            for (let i in material) {
              const t = material[i]

              if (i.endsWith('Texture') && t instanceof BABYLON.Texture && t !== probe.cubeTexture) {
                if (!assetContainer.textures.includes(t)) {
                  if (t.url.includes(this.context.domain)) {
                    assetContainer.textures.push(t)
                    this.context.registerTexture(t)
                  }
                }
              }
            }
          })

          // TODO(menduz): what happens if the load ends when the entity got removed?
          if (!entity.isDisposed()) {
            this.assetContainerEntity.set(entity.uuid, assetContainer)

            processColliders(assetContainer, entity.getActionManager())

            const node = new BABYLON.AbstractMesh('obj')

            assetContainer.addAllToScene()
            // BABYLON doesn't return the materials in the asset container
            // TODO(menduz): raise an issue
            assetContainer.transformNodes.forEach($ => ($.parent = node))

            assetContainer.meshes.forEach($ => {
              $.parent = node
            })

            node.scaling.set(1, 1, -1)

            entity.setObject3D(BasicShape.nameInEntity, node)

            entity.sendUpdatePositions()
            entity.sendUpdateMetrics()
          } else {
            cleanupAssetContainer(assetContainer)
          }
          this.loadingDone = true
          this.context.logger.log('obj loaded')
        },
        null,
        (_scene, message, exception) => {
          this.context.logger.error('Error loading OBJ', message || exception)
          this.onDetach(entity)
          this.loadingDone = true
          debugger
        },
        '.obj'
      )
    }
  }

  onDetach(entity: BaseEntity): void {
    const mesh = entity.getObject3D(BasicShape.nameInEntity)

    if (mesh) {
      entity.setObject3D(BasicShape.nameInEntity, null)
      mesh.dispose(true, false)
    }

    const assetContainer = this.assetContainerEntity.get(entity.uuid)

    if (assetContainer) {
      cleanupAssetContainer(assetContainer)
      this.assetContainerEntity.delete(entity.uuid)
    }
  }

  async updateData(data: any): Promise<void> {
    if ('src' in data) {
      if (this.src !== null && this.src !== data.src && DEBUG) {
        this.context.logger.log('Cannot set OBJShape.src twice')
      }
      if (this.src === null) {
        this.src = data.src

        if ('visible' in data) {
          if (data.visible === false) {
            this.entities.forEach($ => this.onDetach($))
          } else {
            this.entities.forEach($ => this.attachTo($))
          }
        } else {
          this.entities.forEach($ => this.attachTo($))
        }
      }
    }
  }

  dispose() {
    super.dispose()
    this.assetContainerEntity.forEach($ => cleanupAssetContainer($))
  }
}

DisposableComponent.registerClassId(CLASS_ID.OBJ_SHAPE, OBJShape)
