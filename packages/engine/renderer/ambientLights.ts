import * as BABYLON from 'babylonjs'
import { parcelLimits, DEBUG, visualConfigurations } from 'config'
import { scene, vrHelper } from './init'
import { EnvironmentHelper, skyMaterial2 } from './envHelper'
import { ambientConfigurations } from './ambientConfigurations'
import { createCrossHair } from './controls/fpsCrossHair'

export const hemiLight = new BABYLON.HemisphericLight('default light', ambientConfigurations.sunPosition, scene)

export const probe = new BABYLON.ReflectionProbe('skyReflection', 512, scene, true, true)

export const crossHair = createCrossHair(scene)

export const envHelper = new EnvironmentHelper(
  {
    groundShadowLevel: 0.6,
    createGround: true
  },
  scene
)

let editorEnvHelper: BABYLON.EnvironmentHelper = null

const skybox = BABYLON.MeshBuilder.CreateSphere(
  'skybox',
  { diameter: 2 * visualConfigurations.farDistance - 5, sideOrientation: BABYLON.Mesh.BACKSIDE },
  scene
)

export const checkerboardMaterial = new BABYLON.GridMaterial('checkerboard', scene)

{
  crossHair.parent = vrHelper.deviceOrientationCamera
}

let sunInclination = -0.31

/// --- SIDE EFFECT ---

{
  checkerboardMaterial.gridRatio = 1
  checkerboardMaterial.mainColor = BABYLON.Color3.Gray()
  checkerboardMaterial.lineColor = BABYLON.Color3.White()
  checkerboardMaterial.zOffset = 1
  checkerboardMaterial.fogEnabled = false

  envHelper.skybox.setEnabled(false)

  probe.cubeTexture.onBeforeBindObservable.add(() => envHelper.skybox.setEnabled(true))
  probe.cubeTexture.onAfterUnbindObservable.add(() => envHelper.skybox.setEnabled(false))
  probe.cubeTexture.gammaSpace = false

  probe.cubeTexture.dispose = () => {
    throw new Error('cannot dispose sky reflections')
  }

  probe.renderList.push(envHelper.skybox)

  probe.attachToMesh(envHelper.skybox)
  probe.refreshRate = BABYLON.RenderTargetTexture.REFRESHRATE_RENDER_ONEVERYFRAME

  envHelper.groundMaterial.environmentTexture = probe.cubeTexture

  {
    skybox.material = skyMaterial2(scene)
  }

  hemiLight.diffuse = BABYLON.Color3.White()
  hemiLight.groundColor = ambientConfigurations.groundColor.clone()
  hemiLight.specular = ambientConfigurations.sunColor.clone()

  if (DEBUG) {
    // tslint:disable-next-line:semicolon
    ;(window as any)['setSunInclination'] = setSunInclination
  }
}

/// --- EXPORTS ---

export function setEditorEnvironment(enabled: boolean) {
  if (enabled) {
    if (!editorEnvHelper) {
      const editorColor = BABYLON.Color3.FromHexString('#0e0c12')
      editorEnvHelper = scene.createDefaultEnvironment({
        environmentTexture: null,
        groundColor: editorColor,
        skyboxColor: editorColor
      })
      editorEnvHelper.ground.position.y = 0
      editorEnvHelper.rootMesh.position.y = -0.1
    }

    checkerboardMaterial.mainColor = BABYLON.Color3.FromHexString('#242129')
    checkerboardMaterial.lineColor = BABYLON.Color3.FromHexString('#45404c')

    scene.fogEnabled = false
    skybox.visibility = 0
    envHelper.ground.visibility = 0

    crossHair.visibility = 0
  } else {
    if (editorEnvHelper) {
      editorEnvHelper.ground.dispose()
      editorEnvHelper.dispose()
      editorEnvHelper = null
    }

    checkerboardMaterial.mainColor = BABYLON.Color3.Gray()
    checkerboardMaterial.lineColor = BABYLON.Color3.White()

    scene.fogEnabled = true
    skybox.visibility = 1
    envHelper.ground.visibility = 1

    crossHair.visibility = 1
  }

  reposition()
}

export function setSunInclination(inclination: number) {
  sunInclination = inclination
}

export function reposition() {
  skybox.position = scene.activeCamera.position

  envHelper.rootMesh.position.set(
    Math.floor(scene.activeCamera.position.x / parcelLimits.parcelSize) * parcelLimits.parcelSize,
    0,
    Math.floor(scene.activeCamera.position.z / parcelLimits.parcelSize) * parcelLimits.parcelSize
  )

  envHelper.ground.position.set(scene.activeCamera.position.x, 0, scene.activeCamera.position.z)

  const theta = Math.PI * sunInclination
  const phi = Math.PI * -0.4

  ambientConfigurations.sunPositionColor.r = ambientConfigurations.sunPosition.x = 500 * Math.cos(phi)
  ambientConfigurations.sunPositionColor.g = ambientConfigurations.sunPosition.y = 500 * Math.sin(phi) * Math.sin(theta)
  ambientConfigurations.sunPositionColor.b = ambientConfigurations.sunPosition.z = 500 * Math.sin(phi) * Math.cos(theta)

  const sunfade = 1.0 - Math.min(Math.max(1.0 - Math.exp(ambientConfigurations.sunPosition.y / 10), 0.0), 0.9)
  hemiLight.intensity = sunfade

  hemiLight.diffuse.set(sunfade, sunfade, sunfade)
  hemiLight.groundColor.copyFrom(ambientConfigurations.groundColor).scale(sunfade)
  hemiLight.specular.copyFrom(ambientConfigurations.sunColor).scale(sunfade)
}

reposition()
setEditorEnvironment(false)
