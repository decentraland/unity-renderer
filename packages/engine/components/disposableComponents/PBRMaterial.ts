import { DisposableComponent, BasicShape } from './DisposableComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { validators } from '../helpers/schemaValidator'
import { scene } from '../../renderer'
import { probe } from '../../renderer/ambientLights'
import { CLASS_ID, Observer } from 'decentraland-ecs/src'
import { deleteUnusedTextures } from 'engine/entities/loader'

const defaults = {
  alpha: 1,
  albedoColor: BABYLON.Color3.Gray(),
  emissiveColor: BABYLON.Color3.Black(),
  metallic: 0.5,
  roughness: 0.5,
  ambientColor: BABYLON.Color3.Black(),
  reflectionColor: BABYLON.Color3.White(),
  reflectivityColor: BABYLON.Color3.White(),
  directIntensity: 1,
  microSurface: 1,
  emissiveIntensity: 1,
  environmentIntensity: 1,
  specularIntensity: 1,
  albedoTexture: '',
  alphaTexture: '',
  emissiveTexture: '',
  bumpTexture: '',
  refractionTexture: '',
  disableLighting: false,
  transparencyMode: 0,
  hasAlpha: false
}

export class PBRMaterial extends DisposableComponent {
  material: BABYLON.PBRMaterial
  meshObserver: Observer<{ type: string; object: BABYLON.TransformNode }>

  constructor(ctx, uuid) {
    super(ctx, uuid)
    this.material = new BABYLON.PBRMaterial('#' + this.uuid, scene)
    this.contributions.materials.add(this.material)
    this.loadingDone = false
  }

  updateMeshMaterial = (mesh: BABYLON.Mesh) => {
    if (!mesh) return
    mesh.material = this.material
  }

  removeMeshMaterial = (mesh: BABYLON.Mesh) => {
    if (!mesh) return
    mesh.material = null
  }

  meshObserverCallback = ({ type, object }) => {
    if (type === BasicShape.nameInEntity) {
      this.updateMeshMaterial(object)
    }
  }

  onAttach(entity: BaseEntity): void {
    if (entity.onChangeObject3DObservable) {
      entity.onChangeObject3DObservable.add(this.meshObserverCallback)
    }
    this.updateMeshMaterial(entity.getObject3D(BasicShape.nameInEntity) as any)
  }

  onDetach(entity: BaseEntity): void {
    if (entity.onChangeObject3DObservable) {
      entity.onChangeObject3DObservable.removeCallback(this.meshObserverCallback)
    }
    this.removeMeshMaterial(entity.getObject3D(BasicShape.nameInEntity) as any)
  }

  dispose() {
    this.material.dispose(false, false)
    super.dispose()
    deleteUnusedTextures()
  }

  async updateData(data: any): Promise<void> {
    this.loadingDone = false
    const m = this.material

    m.albedoColor.copyFrom(validators.color(data.albedoColor, defaults.albedoColor))
    m.ambientColor.copyFrom(validators.color(data.ambientColor, defaults.ambientColor))
    m.reflectionColor.copyFrom(validators.color(data.reflectionColor, defaults.reflectionColor))
    m.emissiveColor.copyFrom(validators.color(data.emissiveColor, defaults.emissiveColor))

    m.metallic = Math.min(1, Math.max(0, validators.float(data.metallic, defaults.metallic)))
    m.roughness = Math.min(1, Math.max(0, validators.float(data.roughness, defaults.roughness)))

    m.reflectionTexture = probe.cubeTexture

    if ('alpha' in data) {
      m.alpha = Math.min(1, Math.max(0, validators.float(data.alpha, defaults.alpha)))
    }

    if ('directIntensity' in data) {
      m.directIntensity = Math.min(1, Math.max(0, validators.float(data.directIntensity, defaults.directIntensity)))
    }

    if ('transparencyMode' in data) {
      m.transparencyMode = Math.min(3, Math.max(0, validators.int(data.transparencyMode, defaults.transparencyMode)))
    }

    if ('emissiveIntensity' in data) {
      m.emissiveIntensity = Math.min(
        1,
        Math.max(0, validators.float(data.emissiveIntensity, defaults.emissiveIntensity))
      )
    }

    if ('environmentIntensity' in data) {
      m.environmentIntensity = Math.min(
        1,
        Math.max(0, validators.float(data.environmentIntensity, defaults.environmentIntensity))
      )
    }

    if ('specularIntensity' in data) {
      m.specularIntensity = Math.min(
        1,
        Math.max(0, validators.float(data.specularIntensity, defaults.specularIntensity))
      )
    }

    if ('microSurface' in data) {
      m.microSurface = Math.min(1, Math.max(0, validators.float(data.microSurface, defaults.microSurface)))
    }

    if ('disableLighting' in data) {
      m.disableLighting = validators.boolean(data.disableLighting, defaults.disableLighting)
    }

    if (data.albedoTexture) {
      const src = validators.string(data.albedoTexture, defaults.albedoTexture)
      if (src) {
        const texture = await this.context.getTexture(src)

        if (texture) {
          m.albedoTexture = texture
        }
      }
    }

    if ('hasAlpha' in data) {
      m.useAlphaFromAlbedoTexture = validators.boolean(data.hasAlpha, defaults.hasAlpha)

      if (m.albedoTexture) {
        m.albedoTexture.hasAlpha = m.useAlphaFromAlbedoTexture
      }
    }

    if (data.albedoTexture === data.alphaTexture) {
      m.opacityTexture = m.albedoTexture
    } else if (data.alphaTexture) {
      const src = validators.string(data.alphaTexture, defaults.alphaTexture)
      if (src) {
        const texture = await this.context.getTexture(src)

        if (texture) {
          m.opacityTexture = texture
        }
      }
    }

    if (data.emissiveTexture) {
      const src = validators.string(data.emissiveTexture, defaults.emissiveTexture)
      if (src) {
        const texture = await this.context.getTexture(src)

        if (texture) {
          m.emissiveTexture = texture
        }
      }
    }

    if (data.bumpTexture) {
      const src = validators.string(data.bumpTexture, defaults.bumpTexture)
      if (src) {
        const texture = await this.context.getTexture(src)

        if (texture) {
          m.bumpTexture = texture
        }
      }
    }

    if (data.refractionTexture) {
      const src = validators.string(data.refractionTexture, defaults.refractionTexture)
      if (src) {
        const texture = await this.context.getTexture(src)

        if (texture) {
          m.refractionTexture = texture
        }
      }
    }

    this.contributions.textures.clear()

    m.bumpTexture && this.contributions.textures.add(m.bumpTexture)
    m.albedoTexture && this.contributions.textures.add(m.albedoTexture)
    m.albedoTexture && this.contributions.textures.add(m.albedoTexture)
    m.ambientTexture && this.contributions.textures.add(m.ambientTexture)
    m.opacityTexture && this.contributions.textures.add(m.opacityTexture)
    m.emissiveTexture && this.contributions.textures.add(m.emissiveTexture)
    m.refractionTexture && this.contributions.textures.add(m.refractionTexture)

    this.loadingDone = true

    deleteUnusedTextures()
  }
}

DisposableComponent.registerClassId(CLASS_ID.PRB_MATERIAL, PBRMaterial)
