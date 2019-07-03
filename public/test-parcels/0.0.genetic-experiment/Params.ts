import { Vector3, Color3, Texture, Material, IEntity} from 'decentraland-ecs/src'


//  PROBABILITY
export let MutationProb: number = 0.7
// 0 TO 1   how likely each genome will have some level of mutation

//  1 ->  -0.5 TO + 0.5
export let MutationMaxSpreads: number[] = [
  0.25,  // size
  20,   // temperature
  0.25,  // ears
  0.25,  // eyes
  0.25,  // feet
  0.25,  // mouth
  0.25,  // nose
  0.25,  // tail
  0.25  // wings
]

export let TemperatureButtonValue = 10

export let framesBetweenDamage = 15

// Coefficient to multiply damage done on every frame
export let DamageCoeff = 0.005 * framesBetweenDamage

export let grabbedObject: IEntity | null = null
export function SetGrabbedObject(newGrabbedObject: IEntity | null){
  grabbedObject = newGrabbedObject
}

// Minimum temperature diff for the creature to start receiving damage
export let MinTemperatureDiffForDamage = 8

// Creatures at min temperature (-100) will have this scale factor
export let MinCreatureScale = 0.25

// Creatures at max temperature (100) will have this scale factor
export let MaxCreatureScale = 2.3

export let ColdEnvironmentTemperature = -30
export let HotEnvironmentTemperature = 70

export let neutralEnvironmentPosition = new Vector3(16, 0.01, 40)
export let hotEnvironmentPosition = new Vector3(48, 0.01, 32)
export let coldEnvironmentPosition = new Vector3(48, 0.01, 48)

export let winPanelTex = new Texture("images/YouWin.png")

export let coldIconTex = new Texture("images/cold-thermometer.png")
export let coldIconMaterial = new Material()
coldIconMaterial.alphaTexture = coldIconMaterial.albedoTexture = coldIconTex

export let hotIconTex = new Texture("images/hot-thermometer.png")
export let hotIconMaterial = new Material()
hotIconMaterial.alphaTexture = hotIconMaterial.albedoTexture = hotIconTex

export let neutralIconTex = new Texture("images/thermometer.png")
export let neutralIconMaterial = new Material()
neutralIconMaterial.alphaTexture = neutralIconMaterial.albedoTexture = neutralIconTex

export let chippaIconTex = new Texture("images/Chipaicon.png")
export let chippaIconMaterial = new Material()
chippaIconMaterial.alphaTexture = chippaIconMaterial.albedoTexture = chippaIconTex
export let hotMaterial = new Material()
hotMaterial.albedoColor = Color3.Red()
export let coldMaterial = new Material()
coldMaterial.albedoColor = Color3.Blue()
export let neutralMaterial = new Material()
neutralMaterial.albedoColor = Color3.Gray()
export let redMaterial = new Material()
redMaterial.albedoColor = Color3.Red()
export let yellowMaterial = new Material()
yellowMaterial.albedoColor = Color3.Yellow()
export let greenMaterial = new Material()
greenMaterial.albedoColor = Color3.Green()

export let CheckGameWinConditions!: any
export function SetCheckGameWinConditionsFunction(checkGameWinConditionsCallback: any){
  CheckGameWinConditions = checkGameWinConditionsCallback
}

export let CheckGameLoseConditions!: any
export function SetCheckGameLoseConditionsFunction(checkGameLoseConditionsCallback: any){
  CheckGameLoseConditions = checkGameLoseConditionsCallback
}

export let gameOver = false
export function SetGameOverValue(newGameOverValue: boolean){
  gameOver = newGameOverValue
}
