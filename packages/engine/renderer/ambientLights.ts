import * as BABYLON from 'babylonjs'
import { parcelLimits, DEBUG, visualConfigurations } from 'config'
import { scene } from './init'
import { EnvironmentHelper, skyMaterial2 } from './envHelper'
import { ambientConfigurations } from './ambientConfigurations'

export const hemiLight = new BABYLON.HemisphericLight('default light', ambientConfigurations.sunPosition, scene)

export const probe = new BABYLON.ReflectionProbe('skyReflection', 512, scene, true, true)

export const envHelper = new EnvironmentHelper(
  {
    groundShadowLevel: 0.6,
    createGround: true
  },
  scene
)

const skybox = BABYLON.MeshBuilder.CreateSphere(
  'skybox',
  { diameter: 2 * visualConfigurations.farDistance - 5, sideOrientation: BABYLON.Mesh.BACKSIDE },
  scene
)

let sunInclination = -0.31

/// --- SIDE EFFECT ---

{
  envHelper.skybox.setEnabled(false)

  probe.cubeTexture.onBeforeBindObservable.add(() => envHelper.skybox.setEnabled(true))
  probe.cubeTexture.onAfterUnbindObservable.add(() => envHelper.skybox.setEnabled(false))
  probe.cubeTexture.gammaSpace = false

  probe.cubeTexture.dispose = () => {
    throw new Error('cannot dispose sky reflections')
  }

  probe.renderList.push(envHelper.skybox)

  probe.attachToMesh(envHelper.skybox)
  probe.refreshRate = BABYLON.RenderTargetTexture.REFRESHRATE_RENDER_ONEVERYTWOFRAMES

  envHelper.groundMaterial.environmentTexture = probe.cubeTexture

  {
    skybox.material = skyMaterial2(scene)
  }

  hemiLight.diffuse = BABYLON.Color3.White()
  hemiLight.groundColor = ambientConfigurations.groundColor
  hemiLight.specular = ambientConfigurations.sunColor

  if (DEBUG) {
    // tslint:disable-next-line:semicolon
    ;(window as any)['setSunInclination'] = setSunInclination
  }
}

/// --- EXPORTS ---

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
