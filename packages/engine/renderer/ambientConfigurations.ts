import * as BABYLON from 'babylonjs'

export namespace ambientConfigurations {
  // TODO: move this configurations inside EnvironmentHelper(options)
  export const groundColor = new BABYLON.Color3(0.1, 0.1, 0.1)
  export const sunColor = new BABYLON.Color3(1, 1, 1)
  export const sunPosition = new BABYLON.Vector3(-1, 0.01, 0.3).scaleInPlace(500)
  export const sunPositionColor = new BABYLON.Color3(sunPosition.x, sunPosition.y, sunPosition.z)

  export const RED = BABYLON.Color3.FromHexString('#ff004f')
  export const GREEN = BABYLON.Color3.FromHexString('#00e57a')
  export const BLUE = BABYLON.Color3.FromHexString('#00beff')
}
