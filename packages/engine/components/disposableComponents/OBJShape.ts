import { DisposableComponent, BasicShape } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { cleanupAssetContainer, processColliders } from 'engine/entities/utils/processModels'
import { resolveUrl } from 'atomicHelpers/parseUrl'
import { scene } from 'engine/renderer'
import { probe } from 'engine/renderer/ambientLights'
import { DEBUG } from 'config'
import { isSceneTexture, deleteUnusedTextures } from 'engine/renderer/monkeyLoader'

export class OBJShape extends DisposableComponent {
  assetContainerEntity = new Map<string, BABYLON.AssetContainer>()
  src: string | null = null
  error = false

  private didFillContributions = false

  loadingDone(entity: BaseEntity): boolean {
    if (this.error) {
      return true
    }
    if (this.entities.has(entity)) {
      return this.assetContainerEntity.has(entity.uuid)
    }
    return false
  }

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
            if (mesh instanceof BABYLON.Mesh) {
              if (mesh.geometry && !assetContainer.geometries.includes(mesh.geometry)) {
                assetContainer.geometries.push(mesh.geometry)
              }
            }
            mesh.subMeshes &&
              mesh.subMeshes.forEach(subMesh => {
                const mesh = subMesh.getMesh()
                if (mesh instanceof BABYLON.Mesh) {
                  if (mesh.geometry && !assetContainer.geometries.includes(mesh.geometry)) {
                    assetContainer.geometries.push(mesh.geometry)
                  }
                }
              })
          })

          processColliders(assetContainer)

          // Find all the materials from all the meshes and add to $.materials
          assetContainer.meshes.forEach(mesh => {
            mesh.cullingStrategy = BABYLON.AbstractMesh.CULLINGSTRATEGY_BOUNDINGSPHERE_ONLY
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
              const t = (material as any)[i]

              if (i.endsWith('Texture') && t instanceof BABYLON.Texture && t !== probe.cubeTexture) {
                if (!assetContainer.textures.includes(t)) {
                  if (isSceneTexture(t)) {
                    assetContainer.textures.push(t)
                  }
                }
              }
            }
          })

          if (this.isStillValid(entity)) {
            this.assetContainerEntity.set(entity.uuid, assetContainer)

            const node = new BABYLON.AbstractMesh('obj')

            assetContainer.addAllToScene()
            // BABYLON doesn't return the materials in the asset container
            // TODO(menduz): raise an issue
            assetContainer.transformNodes.forEach($ => ($.parent = node))

            assetContainer.meshes.forEach($ => {
              $.parent = node
            })

            // TODO: Remove this if after we instantiate GLTF
            if (!this.didFillContributions) {
              this.didFillContributions = true
              assetContainer.materials.forEach($ => {
                this.contributions.materials.add($)
              })
              assetContainer.geometries.forEach($ => {
                this.contributions.geometries.add($)
              })
              assetContainer.textures.forEach($ => {
                this.contributions.textures.add($)
              })
            }

            node.scaling.set(1, 1, -1)

            entity.setObject3D(BasicShape.nameInEntity, node)
          } else {
            cleanupAssetContainer(assetContainer)
            deleteUnusedTextures()
          }
        },
        null,
        (_scene, message, exception) => {
          this.error = true
          this.context.logger.error('Error loading OBJ', message, exception)
          this.onDetach(entity)
        },
        '.obj'
      )
    }
  }

  onDetach(entity: BaseEntity): void {
    const mesh = entity.getObject3D(BasicShape.nameInEntity)

    if (mesh) {
      entity.removeObject3D(BasicShape.nameInEntity)
      mesh.dispose(true, false)
    }

    const assetContainer = this.assetContainerEntity.get(entity.uuid)

    if (assetContainer) {
      cleanupAssetContainer(assetContainer)
      this.assetContainerEntity.delete(entity.uuid)
    }

    deleteUnusedTextures()
  }

  isStillValid(entity: BaseEntity) {
    return !entity.isDisposed() && this.entities.has(entity)
  }

  async updateData(data: any): Promise<void> {
    if ('src' in data) {
      if (this.src !== null && this.src !== data.src && DEBUG) {
        this.context.logger.log('Cannot set OBJShape.src twice')
      }
      if (this.src === null) {
        this.src = data.src
      }
      if (this.src) {
        if ('visible' in data) {
          if (data.visible === false) {
            this.entities.forEach($ => this.onDetach($))
          } else {
            this.entities.forEach($ => this.onAttach($))
          }
        } else {
          this.entities.forEach($ => this.onAttach($))
        }
      }
    }
  }
}

DisposableComponent.registerClassId(CLASS_ID.OBJ_SHAPE, OBJShape)
