import { DisposableComponent, BasicShape } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { cleanupAssetContainer, processColliders } from 'engine/entities/utils/processModels'
import { resolveUrl } from 'atomicHelpers/parseUrl'
import { scene } from 'engine/renderer'
import { probe } from 'engine/renderer/ambientLights'
import { DEBUG } from 'config'
import { log } from 'util'

export class GLTFShape extends DisposableComponent {
  src: string | null = null
  loadingDone = false
  assetContainerEntity = new Map<string, BABYLON.AssetContainer>()

  onAttach(entity: BaseEntity): void {
    if (this.src) {
      const url = resolveUrl(this.context.internalBaseUrl, this.src)
      const baseUrl = url.substr(0, url.lastIndexOf('/') + 1)

      const file = url.replace(baseUrl, '')

      const loader: BABYLON.GLTFFileLoader = BABYLON.SceneLoader.LoadAssetContainer(
        baseUrl,
        file,
        scene,
        assetContainer => {
          if (this.assetContainerEntity.has(entity.uuid)) {
            this.onDetach(entity)
          }

          // Find all the materials from all the meshes and add to $.materials
          assetContainer.meshes.forEach(mesh => {
            if (mesh.material) {
              if (!assetContainer.materials.includes(mesh.material)) {
                assetContainer.materials.push(mesh.material)
              }
            }
          })

          // Find the textures in the materials that share the same domain as the context
          // then add the textures to the $.textures
          assetContainer.materials.forEach((material: BABYLON.Material | BABYLON.PBRMaterial) => {
            for (let i in material) {
              const t = material[i]

              if (i.endsWith('Texture') && t instanceof BABYLON.Texture && t !== probe.cubeTexture) {
                if (!assetContainer.textures.includes(t)) {
                  if (
                    (this.context && t.url.includes(this.context.domain)) ||
                    (t.url.startsWith('data:/') && t.url.includes(file))
                  ) {
                    assetContainer.textures.push(t)
                    this.context && this.context.registerTexture(t)
                  }
                }
              }
            }

            if ('reflectionTexture' in material) {
              material.reflectionTexture = probe.cubeTexture
            }
          })

          // TODO(menduz): what happens if the load ends when the entity got removed?
          if (!entity.isDisposed()) {
            processColliders(assetContainer, entity.getActionManager())

            // Fin the main mesh and add it as the BasicShape.nameInEntity component.
            assetContainer.meshes.filter($ => $.name === '__root__').forEach($ => {
              entity.setObject3D(BasicShape.nameInEntity, $)
              $.rotation.set(0, Math.PI, 0)
            })

            this.assetContainerEntity.set(entity.uuid, assetContainer)

            // This is weird. Verify what does this do.
            assetContainer.transformNodes.filter($ => $.name === '__root__').forEach($ => {
              entity.setObject3D(BasicShape.nameInEntity, $)
            })

            assetContainer.addAllToScene()

            entity.sendUpdatePositions()
            entity.sendUpdateMetrics()

            for (let ag of assetContainer.animationGroups) {
              for (let animatable of ag.animatables) {
                animatable.weight = 0
              }
            }
          } else {
            cleanupAssetContainer(assetContainer)
          }

          this.loadingDone = true
        },
        null,
        (_scene, message, exception) => {
          this.context.logger.error('Error loading GLTF', message || exception)
          this.onDetach(entity)
          this.loadingDone = true
        }
      ) as any

      loader.animationStartMode = 0
    } else {
      debugger
    }
  }

  onDetach(entity: BaseEntity): void {
    const mesh = entity.getObject3D(BasicShape.nameInEntity)

    if (mesh) {
      entity.removeObject3D(BasicShape.nameInEntity)
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
        log('Cannot set OBJShape.src twice')
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

DisposableComponent.registerClassId(CLASS_ID.GLTF_SHAPE, GLTFShape)
