import { DisposableComponent, BasicShape } from './DisposableComponent'
import { BaseEntity } from '../../entities/BaseEntity'
import { validators } from '../helpers/schemaValidator'
import { scene } from '../../renderer'
import { CLASS_ID } from 'decentraland-ecs/src'
import { TextureSamplingMode, TextureWrapping } from 'shared/types'
import { deleteUnusedTextures } from 'engine/renderer/monkeyLoader'
import { Texture } from './Texture'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'

BABYLON.Effect.ShadersStore['dclShadelessVertexShader'] = `
  precision highp float;

  // Attributes
  attribute vec3 position;
  attribute vec2 uv;

  // Uniforms
  uniform mat4 worldViewProjection;

  // Varying
  varying vec2 vUV;

  void main(void) {
      gl_Position = worldViewProjection * vec4(position, 1.0);

      vUV = uv;
  }
`

BABYLON.Effect.ShadersStore['dclShadelessFragmentShader'] = `
  precision highp float;

  varying vec2 vUV;

  uniform sampler2D textureSampler;
  uniform float alphaTest;

  void main(void) {
    vec4 color = texture2D(textureSampler, vUV);

    if(color.a < alphaTest)
      discard;

    if (color.rgb == vec3(1.0,0.0,1.0))
      discard;

    gl_FragColor = color;
  }
`

const defaults = {
  texture: '',
  alphaTest: 0.5,
  samplingMode: TextureSamplingMode.BILINEAR,
  wrap: TextureWrapping.CLAMP
}

export class BasicMaterial extends DisposableComponent {
  material: BABYLON.ShaderMaterial
  loadingDonePrivate: boolean = false
  constructor(ctx: SharedSceneContext, uuid: string) {
    super(ctx, uuid)

    this.material = new BABYLON.ShaderMaterial(
      '#' + this.uuid,
      scene,
      {
        vertex: 'dclShadeless',
        fragment: 'dclShadeless'
      },
      {
        attributes: ['position', 'normal', 'uv'],
        uniforms: ['world', 'worldView', 'worldViewProjection', 'view', 'projection', 'alphaTest'],
        samplers: ['textureSampler']
      }
    )

    this.contributions.materials.add(this.material)
    this.loadingDonePrivate = false
  }

  updateMeshMaterial = (mesh: BABYLON.Mesh | null) => {
    if (!mesh) return
    mesh.material = this.material
  }

  removeMeshMaterial = (mesh: BABYLON.Mesh) => {
    if (!mesh) return
    mesh.material = null
  }

  meshObserverCallback = ({ type, object }: { type: string; object: BABYLON.TransformNode | null }) => {
    if (type === BasicShape.nameInEntity) {
      this.updateMeshMaterial(object as any)
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
    deleteUnusedTextures()
    super.dispose()
  }

  async updateData(data: any): Promise<void> {
    this.loadingDonePrivate = false
    if (data.texture) {
      const texture = await Texture.getFromComponent(this.context, data.texture)

      if (texture) {
        this.material.setTexture('textureSampler', texture)
      }
    }

    const alphaTest = validators.float(data.alphaTest, defaults.alphaTest)
    this.material.setFloat('alphaTest', alphaTest)

    deleteUnusedTextures()

    this.loadingDonePrivate = true
  }

  loadingDone() {
    return this.loadingDonePrivate
  }
}

DisposableComponent.registerClassId(CLASS_ID.BASIC_MATERIAL, BasicMaterial)
