import { DisposableComponent, BasicShape } from './DisposableComponent'
import { CLASS_ID } from 'decentraland-ecs/src'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { cleanupAssetContainer } from 'engine/entities/utils/processModels'
import { scene, engine } from 'engine/renderer'
import { DEBUG } from 'config'
import { log } from 'util'
import { Animator } from '../ephemeralComponents/Animator'
import { deleteUnusedTextures } from 'engine/renderer/monkeyLoader'
import { processGLTFAssetContainer, loadingShape } from './GLTFShape'

let noise: BABYLON.Texture | null = null

function getNoiseTexture() {
  if (!noise) {
    noise = new BABYLON.FireProceduralTexture('perlin', 256, scene)
  }
  return noise
}

function parseProtocolUrl(url: string): { protocol: string; registry: string; asset: string } {
  const parsedUrl = /([^:]+):\/\/([^/]+)(?:\/(.+))?/.exec(url)

  if (!parsedUrl) throw new Error('The provided URL is not valid: ' + url)

  const result = {
    asset: parsedUrl[3],
    registry: parsedUrl[2],
    protocol: parsedUrl[1]
  }

  if (result.protocol.endsWith(':')) {
    result.protocol = result.protocol.replace(/:$/, '')
  }

  if (result.protocol !== 'ethereum') {
    throw new Error('Invalid protocol: ' + result.protocol)
  }

  return result
}

async function fetchDARAsset(registry: string, assetId: string): Promise<{ image: string; files: any }> {
  const req = await fetch(`https://schema-api-staging.now.sh/dar/${registry}/asset/${assetId}`)
  return req.json()
}

async function fetchBase64Image(url: string) {
  const response = await fetch(url, {
    method: 'GET',
    headers: {},
    mode: 'cors',
    cache: 'default'
  })

  const buffer = await response.arrayBuffer()
  let base64Flag = `data:${response.headers.get('content-type')};base64,`
  let imageStr = arrayBufferToBase64(buffer)

  return base64Flag + imageStr
}

function arrayBufferToBase64(buffer: ArrayBuffer) {
  let binary = ''
  let bytes = [].slice.call(new Uint8Array(buffer))

  bytes.forEach((b: number) => (binary += String.fromCharCode(b)))

  return btoa(binary)
}

export class NFTShape extends DisposableComponent {
  src: string | null = null
  assetContainerEntity = new Map<string, BABYLON.AssetContainer>()
  entityIsLoading = new Set<string>()
  error = false
  tex: BABYLON.Texture | null = null
  private didFillContributions = false

  loadingDone(entity: BaseEntity): boolean {
    if (this.error) {
      return true
    }
    if (this.entities.has(entity)) {
      if (this.entityIsLoading.has(entity.uuid)) {
        return false
      }
      return true
    }
    return false
  }

  onAttach(entity: BaseEntity): void {
    if (!this.entityIsLoading.has(entity.uuid)) {
      this.entityIsLoading.add(entity.uuid)

      const loadingEntity = new BABYLON.TransformNode('loading-padding')

      entity.setObject3D(BasicShape.nameInEntity, loadingEntity)

      {
        const loadingInstance = loadingShape.createInstance('nft-loader')
        loadingInstance.parent = loadingEntity
        loadingInstance.position.y = 0.8
        const animationDisposable = scene.onAfterRenderObservable.add(() => {
          loadingInstance.rotation.set(0, 0, loadingInstance.rotation.z - engine.getDeltaTime() * 0.01)
          loadingInstance.billboardMode = 7
        })

        loadingEntity.onDisposeObservable.add(() => {
          loadingEntity.parent = null
          loadingInstance.parent = null
          loadingInstance.dispose()
          scene.onAfterRenderObservable.remove(animationDisposable)
        })
      }

      BABYLON.SceneLoader.LoadAssetContainer(
        '/models/frames/',
        'basic.glb',
        scene,
        assetContainer => {
          this.entityIsLoading.delete(entity.uuid)

          if (this.assetContainerEntity.has(entity.uuid)) {
            this.onDetach(entity)
          }

          processGLTFAssetContainer(assetContainer)

          loadingEntity.dispose(false, false)

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

          const pictureMaterial = assetContainer.materials[1] as BABYLON.PBRMaterial
          const frameMaterial = assetContainer.materials[0] as BABYLON.PBRMaterial

          frameMaterial.emissiveTexture = getNoiseTexture()
          frameMaterial.albedoTexture = getNoiseTexture()
          frameMaterial.emissiveIntensity = 1

          if (this.tex) {
            pictureMaterial.useAlphaFromAlbedoTexture = true
            pictureMaterial.forceAlphaTest = true
            pictureMaterial.albedoTexture = this.tex
            pictureMaterial.emissiveIntensity = 0
            pictureMaterial.metallic = 0
            pictureMaterial.roughness = 1
            pictureMaterial.albedoColor = BABYLON.Color3.White()
            pictureMaterial.enableSpecularAntiAliasing = true
          }

          if (this.isStillValid(entity)) {
            // Fin the main mesh and add it as the BasicShape.nameInEntity component.
            assetContainer.meshes
              .filter($ => $.name === '__root__')
              .forEach(mesh => {
                entity.setObject3D(BasicShape.nameInEntity, mesh)
              })

            this.assetContainerEntity.set(entity.uuid, assetContainer)

            entity.assetContainer = assetContainer

            const animator: Animator = entity.getBehaviorByName('animator') as Animator

            if (animator) {
              animator.transformValue(animator.value!)
            }
          } else {
            cleanupAssetContainer(assetContainer)
            deleteUnusedTextures()
          }
        },
        null,
        (_scene, message, exception) => {
          this.entityIsLoading.delete(entity.uuid)

          this.context.logger.error('Error loading GLTF', message, exception)
          this.onDetach(entity)
          entity.assetContainer = void 0

          const animator: Animator = entity.getBehaviorByName('animator') as Animator

          if (animator) {
            animator.transformValue(animator.value!)
          }
        }
      )
    }
  }

  onDetach(entity: BaseEntity): void {
    this.entityIsLoading.delete(entity.uuid)

    const mesh = entity.getObject3D(BasicShape.nameInEntity)

    if (mesh) {
      entity.removeObject3D(BasicShape.nameInEntity)
    }

    entity.assetContainer = void 0

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
        log('Cannot set NFTShape.src twice')
        return
      }
      if (this.src === null) {
        this.src = data.src
      }
      if (this.src) {
        const { registry, asset } = parseProtocolUrl(this.src)

        const assetData = await fetchDARAsset(registry, asset)

        // by default, thumbnail should work
        let image = assetData.image

        // anyways, we search the original file that is supposed to have a better resolution
        for (let file of assetData.files) {
          if (file.role === 'dcl-picture-frame-image' && file.name.endsWith('.png')) {
            image = file.url
          }
        }

        const realImage = await fetchBase64Image(image)

        this.tex = new BABYLON.Texture(realImage, scene)
        this.tex.hasAlpha = true

        this.contributions.textures.add(this.tex)

        this.entities.forEach($ => this.onAttach($))
      }
    }
  }
}

DisposableComponent.registerClassId(CLASS_ID.NFT_SHAPE, NFTShape)
