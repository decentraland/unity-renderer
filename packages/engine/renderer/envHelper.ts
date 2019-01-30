import * as BABYLON from 'babylonjs'

import { visualConfigurations, parcelLimits } from 'config'
import { ambientConfigurations } from './ambientConfigurations'

BABYLON.Effect.ShadersStore['dclskyVertexShader'] = `
  // Attributes
  attribute vec3 position; //vertex x,y,z

  // Uniforms
  uniform mat4 world; // convert from model space to world space

  uniform mat4 viewProjection;
  uniform vec3 cameraPosition; // camera x,y,z
  varying vec3 vWorldPosition;
  uniform mat4 view;

  #include<fogVertexDeclaration>

  void main() {
    vec4 worldPos = world * vec4(position, 1.0);
    vWorldPosition = position;

    gl_Position = viewProjection * worldPos;

    #include<fogVertex>
  }
`

BABYLON.Effect.ShadersStore['dclskyFragmentShader'] = `
  precision highp float;
  varying vec2 vUV;
  varying vec3 vWorldPosition;

  #include<fogFragmentDeclaration>

  void main() {
    vec4 result = vec4(GetSky(normalize(vWorldPosition), 1.0), 1.0);

    gl_FragColor = result;
  }
`

BABYLON.Effect.ShadersStore['dclsky2VertexShader'] = `
  // Attributes
  attribute vec3 position; //vertex x,y,z

  // Uniforms
  uniform mat4 world; // convert from model space to world space

  uniform mat4 viewProjection;
  uniform vec3 cameraPosition; // camera x,y,z
  varying vec3 vWorldPosition;
  uniform mat4 view;

  #include<fogVertexDeclaration>

  void main() {
    vec4 worldPos = world * vec4(position, 1.0);
    vWorldPosition = position;

    gl_Position = viewProjection * worldPos;

    #include<fogVertex>
  }
`

BABYLON.Effect.ShadersStore['dclsky2FragmentShader'] = `
  precision highp float;
  varying vec2 vUV;
  varying vec3 vWorldPosition;

  #include<fogFragmentDeclaration>

  const float LinearEncodePowerApprox = 2.2;
  const float GammaEncodePowerApprox = 1.0 / LinearEncodePowerApprox;

  vec3 toGammaSpace(vec3 color){
      return pow(color, vec3(GammaEncodePowerApprox));
  }

  void main() {
    vec4 result = vec4(toGammaSpace(GetSky(normalize(vWorldPosition), 1.0)), 1.0);

    gl_FragColor = result;
  }
`

const gridSize = parcelLimits.visibleRadius * 2

/**
 * Represents the different options available during the creation of
 * a Environment helper.
 *
 * This can control the default ground, skybox and image processing setup of your scene.
 */
export interface IEnvironmentHelperOptions {
  /**
   * Specifies wether or not to create a ground.
   * True by default.
   */
  createGround: boolean
  /**
   * Specifies the ground size.
   * 15 by default.
   */
  groundSize: number
  /**
   * The color mixed in the ground texture by default.
   * BabylonJS clearColor by default.
   */
  groundColor: BABYLON.Color3
  /**
   * Specifies the ground opacity.
   * 1 by default.
   */
  groundOpacity: number
  /**
   * Enables the ground to receive shadows.
   * True by default.
   */
  enableGroundShadow: boolean
  /**
   * Helps preventing the shadow to be fully black on the ground.
   * 0.5 by default.
   */
  groundShadowLevel: number
  /**
   * Creates a mirror texture attach to the ground.
   * false by default.
   */
  enableGroundMirror: boolean
  /**
   * Specifies the ground mirror size ratio.
   * 0.3 by default as the default kernel is 64.
   */
  groundMirrorSizeRatio: number
  /**
   * Specifies the ground mirror blur kernel size.
   * 64 by default.
   */
  groundMirrorBlurKernel: number
  /**
   * Specifies the ground mirror visibility amount.
   * 1 by default
   */
  groundMirrorAmount: number
  /**
   * Specifies the ground mirror reflectance weight.
   * This uses the standard weight of the background material to setup the fresnel effect
   * of the mirror.
   * 1 by default.
   */
  groundMirrorFresnelWeight: number
  /**
   * Specifies the ground mirror Falloff distance.
   * This can helps reducing the size of the reflection.
   * 0 by Default.
   */
  groundMirrorFallOffDistance: number
  /**
   * Specifies the ground mirror texture type.
   * Unsigned Int by Default.
   */
  groundMirrorTextureType: number
  /**
   * Specifies a bias applied to the ground vertical position to prevent z-fighting with
   * the shown objects.
   */
  groundYBias: number

  /**
   * Specifies wether or not to create a skybox.
   * True by default.
   */
  createSkybox: boolean

  /**
   * The background rotation around the Y axis of the scene.
   * This helps aligning the key lights of your scene with the background.
   * 0 by default.
   */
  backgroundYRotation: number

  /**
   * Default position of the rootMesh if autoSize is not true.
   */
  rootPosition: BABYLON.Vector3

  /**
   * Sets up the image processing in the scene.
   * true by default.
   */
  setupImageProcessing: boolean

  /**
   * The value of the exposure to apply to the scene.
   * 0.6 by default if setupImageProcessing is true.
   */
  cameraExposure: number

  /**
   * The value of the contrast to apply to the scene.
   * 1.6 by default if setupImageProcessing is true.
   */
  cameraContrast: number

  /**
   * Specifies wether or not tonemapping should be enabled in the scene.
   * true by default if setupImageProcessing is true.
   */
  toneMappingEnabled: boolean
}

/**
 * The Environment helper class can be used to add a fully featuread none expensive background to your scene.
 * It includes by default a skybox and a ground relying on the BackgroundMaterial.
 * It also helps with the default setup of your imageProcessing configuration.
 */
export class EnvironmentHelper {
  /**
   * Gets the root mesh created by the helper.
   */
  public get rootMesh(): BABYLON.Mesh {
    return this._rootMesh
  }
  /**
   * Gets the skybox created by the helper.
   */
  public get skybox(): BABYLON.Nullable<BABYLON.Mesh> {
    return this._skybox
  }
  /**
   * Gets the skybox material created by the helper.
   */
  public get skyboxMaterial(): BABYLON.Nullable<BABYLON.ShaderMaterial> {
    return this._skyboxMaterial
  }
  /**
   * Gets the ground mesh created by the helper.
   */
  public get ground(): BABYLON.Nullable<BABYLON.Mesh> {
    return this._ground
  }
  /**
   * Gets the ground mirror created by the helper.
   */
  public get groundMirror(): BABYLON.Nullable<BABYLON.MirrorTexture> {
    return this._groundMirror
  }

  /**
   * Gets the ground mirror render list to helps pushing the meshes
   * you wish in the ground reflection.
   */
  public get groundMirrorRenderList(): BABYLON.Nullable<BABYLON.AbstractMesh[]> {
    if (this._groundMirror) {
      return this._groundMirror.renderList
    }
    return null
  }
  /**
   * Gets the ground material created by the helper.
   */
  public get groundMaterial(): BABYLON.Nullable<BABYLON.PBRMetallicRoughnessMaterial> {
    return this._groundMaterial
  }

  /**
   * This observable will be notified with any error during the creation of the environment,
   * mainly texture creation errors.
   */
  public onErrorObservable: BABYLON.Observable<{ message?: string; exception?: any }>

  private _rootMesh: BABYLON.Mesh

  private _skybox: BABYLON.Nullable<BABYLON.Mesh>

  private _skyboxMaterial: BABYLON.Nullable<BABYLON.ShaderMaterial>

  private _ground: BABYLON.Nullable<BABYLON.Mesh>

  private _groundMirror: BABYLON.Nullable<BABYLON.MirrorTexture>

  private _groundMaterial: BABYLON.Nullable<BABYLON.PBRMetallicRoughnessMaterial>

  /**
   * Stores the creation options.
   */
  private readonly _scene: BABYLON.Scene
  private _options: IEnvironmentHelperOptions

  /**
   * constructor
   * @param options
   * @param scene The scene to add the material to
   */
  constructor(options: Partial<IEnvironmentHelperOptions>, scene: BABYLON.Scene) {
    this._options = {
      ...EnvironmentHelper._getDefaultOptions(),
      ...options
    }
    this._scene = scene
    this.onErrorObservable = new BABYLON.Observable()

    this._setupBackground()
    this._setupImageProcessing()
  }

  /**
   * Creates the default options for the helper.
   */
  private static _getDefaultOptions(): IEnvironmentHelperOptions {
    return {
      createGround: true,
      groundSize: 15,
      groundColor: BABYLON.Color3.White(),
      groundOpacity: 1,
      enableGroundShadow: true,
      groundShadowLevel: 0.5,

      enableGroundMirror: false,
      groundMirrorSizeRatio: 0.3,
      groundMirrorBlurKernel: 64,
      groundMirrorAmount: 1,
      groundMirrorFresnelWeight: 1,
      groundMirrorFallOffDistance: 0,
      groundMirrorTextureType: BABYLON.Engine.TEXTURETYPE_UNSIGNED_INT,

      groundYBias: 0.00001,

      createSkybox: true,

      backgroundYRotation: 0,
      rootPosition: BABYLON.Vector3.Zero(),

      setupImageProcessing: false,
      cameraExposure: 0.8,
      cameraContrast: 1.2,
      toneMappingEnabled: false
    }
  }

  /**
   * Updates the background according to the new options
   * @param options
   */
  public updateOptions(options: Partial<IEnvironmentHelperOptions>) {
    const newOptions = {
      ...this._options,
      ...options
    }

    if (this._ground && !newOptions.createGround) {
      this._ground.dispose()
      this._ground = null
    }

    if (this._groundMaterial && !newOptions.createGround) {
      this._groundMaterial.dispose()
      this._groundMaterial = null
    }

    if (this._skybox && !newOptions.createSkybox) {
      this._skybox.dispose()
      this._skybox = null
    }

    if (this._skyboxMaterial && !newOptions.createSkybox) {
      this._skyboxMaterial.dispose()
      this._skyboxMaterial = null
    }

    if (this._groundMirror && !newOptions.enableGroundMirror) {
      this._groundMirror.dispose()
      this._groundMirror = null
    }

    this._options = newOptions

    this._setupBackground()
    this._setupImageProcessing()
  }

  /**
   * Dispose all the elements created by the Helper.
   */
  public dispose(): void {
    if (this._groundMaterial) {
      this._groundMaterial.dispose(true, true)
    }
    if (this._skyboxMaterial) {
      this._skyboxMaterial.dispose(true, true)
    }
    this._rootMesh.dispose(false)
  }

  /**
   * Setup the image processing according to the specified options.
   */
  private _setupImageProcessing(): void {
    if (this._options.setupImageProcessing) {
      this._scene.imageProcessingConfiguration.contrast = this._options.cameraContrast
      this._scene.imageProcessingConfiguration.exposure = this._options.cameraExposure
      this._scene.imageProcessingConfiguration.toneMappingEnabled = this._options.toneMappingEnabled
    }
  }

  /**
   * Setup the background according to the specified options.
   */
  private _setupBackground(): void {
    if (!this._rootMesh) {
      this._rootMesh = new BABYLON.Mesh('BackgroundHelper', this._scene)
    }
    this._rootMesh.rotation.y = this._options.backgroundYRotation

    if (this._options.createGround) {
      this._setupGround()
      this._setupGroundMaterial()
    }

    if (this._options.createSkybox) {
      this._setupSkybox()

      // Initialize scene
      this._scene.fogColor = ambientConfigurations.sunPositionColor
      this._scene.fogStart = visualConfigurations.farDistance / 2
      this._scene.fogEnd = visualConfigurations.farDistance
      this._scene.fogEnabled = true
      this._scene.fogMode = BABYLON.Scene.FOGMODE_LINEAR

      this._setupSkyboxMaterial()
    }
  }

  /**
   * Setup the ground according to the specified options.
   */
  private _setupGround(): void {
    if (!this._ground || this._ground.isDisposed()) {
      this._ground = BABYLON.MeshBuilder.CreateGround(
        'ground',
        {
          width: gridSize * parcelLimits.parcelSize,
          height: gridSize * parcelLimits.parcelSize,
          subdivisionsX: gridSize + 1,
          subdivisionsY: gridSize + 1
        },
        this._scene
      )
      this._ground.alphaIndex = -1
      this._ground.checkCollisions = true

      this._ground.onDisposeObservable.add(() => {
        this._ground = null
      })
    }

    this._ground.receiveShadows = this._options.enableGroundShadow
  }

  /**
   * Setup the ground material according to the specified options.
   */
  private _setupGroundMaterial(): void {
    if (!this._groundMaterial) {
      this._groundMaterial = new BABYLON.PBRMetallicRoughnessMaterial('ground', this._scene)
    }

    this._groundMaterial.roughness = 1
    this._groundMaterial.metallic = 0.4
    this._groundMaterial.zOffset = 2
    this._groundMaterial.baseColor = this._options.groundColor

    if (this._ground) {
      this._ground.material = this._groundMaterial
    }
  }

  /**
   * Setup the skybox according to the specified options.
   */
  private _setupSkybox(): void {
    if (!this._skybox || this._skybox.isDisposed()) {
      this._skybox = BABYLON.MeshBuilder.CreateSphere(
        'sky',
        { diameter: 2 * visualConfigurations.farDistance - 10, sideOrientation: BABYLON.Mesh.BACKSIDE },
        this._scene
      )

      this._skybox.applyFog = false
      this._skybox.alwaysSelectAsActiveMesh = true

      this._skybox.material = this.skyboxMaterial

      this._skybox.onDisposeObservable.add(() => {
        this._skybox = null
      })
    }
  }

  /**
   * Setup the skybox material according to the specified options.
   */
  private _setupSkyboxMaterial(): void {
    if (!this._skybox) {
      return
    }

    this._skyboxMaterial = new BABYLON.ShaderMaterial(
      'skyShader',
      this._scene,
      {
        vertex: 'dclsky',
        fragment: 'dclsky'
      },
      {
        attributes: ['position', 'normal', 'uv'],
        uniforms: [
          'vFogInfos',
          'vFogColor',
          'cameraPosition',
          'world',
          'worldView',
          'worldViewProjection',
          'viewProjection',
          'view',
          'projection'
        ],
        defines: ['#define FOG']
      }
    )

    this._skyboxMaterial.fogEnabled = false
    this._skyboxMaterial.disableDepthWrite = true

    this._skyboxMaterial.setColor4(
      'vFogInfos',
      new BABYLON.Color4(this._scene.fogMode, this._scene.fogStart, this._scene.fogEnd, this._scene.fogDensity)
    )
    this._skyboxMaterial.setColor3('vFogColor', this._scene.fogColor)

    this._skybox.material = this._skyboxMaterial
  }
}

export function skyMaterial2(scene: BABYLON.Scene) {
  const _skyboxMaterial = new BABYLON.ShaderMaterial(
    'skyShader',
    scene,
    {
      vertex: 'dclsky2',
      fragment: 'dclsky2'
    },
    {
      attributes: ['position', 'normal', 'uv'],
      uniforms: [
        'vFogInfos',
        'vFogColor',
        'cameraPosition',
        'world',
        'worldView',
        'worldViewProjection',
        'viewProjection',
        'view',
        'projection'
      ],
      defines: ['#define FOG']
    }
  )

  _skyboxMaterial.fogEnabled = false
  _skyboxMaterial.disableDepthWrite = true

  _skyboxMaterial.setColor4(
    'vFogInfos',
    new BABYLON.Color4(scene.fogMode, scene.fogStart, scene.fogEnd, scene.fogDensity)
  )
  _skyboxMaterial.setColor3('vFogColor', scene.fogColor)

  return _skyboxMaterial
}
