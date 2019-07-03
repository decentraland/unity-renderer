import { OnClick, engine, Vector3, TextShape, Transform, GLTFShape, Entity, Quaternion, PlaneShape, Color3, UICanvas, UIContainerRect, UIText, Color4, log, UIImage } from 'decentraland-ecs/src'

import { Creature, chipaPool, creatures, BuildBody } from "./Creature"
import { Environment, environments } from "./Environment"
import { ButtonData, PushButton } from "./Button"
import { ObjectGrabberSystem, dropObject } from "./grabableObjects";
import { HotEnvironmentTemperature, hotEnvironmentPosition, hotMaterial, ColdEnvironmentTemperature, coldEnvironmentPosition, neutralEnvironmentPosition, neutralMaterial, coldMaterial,  CheckGameWinConditions, TemperatureButtonValue, gameOver, neutralIconMaterial, chippaIconMaterial, winPanelTex, SetGameOverValue, SetCheckGameWinConditionsFunction, SetCheckGameLoseConditionsFunction } from './Params';

// systems
engine.addSystem(new PushButton())

engine.addSystem(new ObjectGrabberSystem())

// Instanciar Terreno
let parkEntity = new Entity()
parkEntity.addComponent(new GLTFShape("models/Environment_01.glb"))
parkEntity.addComponent(
  new Transform()
)
engine.addEntity(parkEntity)

// Instanciar environments
let hotEnvironmentEntity = new Entity()
export let hotEnvironment = new Environment(HotEnvironmentTemperature, hotEnvironmentPosition, 4)
hotEnvironmentEntity.addComponent(hotEnvironment)
hotEnvironmentEntity.addComponent(new PlaneShape())
hotEnvironmentEntity.addComponent(
  new Transform({
    position: hotEnvironmentPosition,
    scale: new Vector3(8, 8, 8),
    rotation: Quaternion.Euler(90, 0, 0)
  })
  )

hotEnvironmentEntity.addComponent(hotMaterial)

hotEnvironmentEntity.addComponent(
  new OnClick(e => {
    dropObject(hotEnvironment)

    CheckGameWinConditions()
  })
)
engine.addEntity(hotEnvironmentEntity)

// Instanciar environments
let coldEnvironmentEntity = new Entity()
export let coldEnvironment = new Environment(ColdEnvironmentTemperature, coldEnvironmentPosition, 4)
coldEnvironmentEntity.addComponent(coldEnvironment)
coldEnvironmentEntity.addComponent(new PlaneShape())
coldEnvironmentEntity.addComponent(
  new Transform({
    position: coldEnvironmentPosition,
    scale: new Vector3(8, 8, 8),
    rotation: Quaternion.Euler(90, 0, 0)
  })
)

coldEnvironmentEntity.addComponent(coldMaterial)

coldEnvironmentEntity.addComponent(
  new OnClick(e => {
    dropObject(coldEnvironment)

    CheckGameWinConditions()
  })
)
engine.addEntity(coldEnvironmentEntity)

// neutral environment
let neutralEnvironmentEntity = new Entity()
export let neutralEnvironment = new Environment(20, neutralEnvironmentPosition, 8)
neutralEnvironmentEntity.addComponent(neutralEnvironment)
neutralEnvironmentEntity.addComponent(new PlaneShape())
neutralEnvironmentEntity.addComponent(
  new Transform({
    position: neutralEnvironmentPosition,
    scale: new Vector3(16, 16, 16),
    rotation: Quaternion.Euler(270, 0, 0)
  })
)

neutralEnvironmentEntity.addComponent(
  new OnClick(e => {
    dropObject(neutralEnvironment)
  })
)
engine.addEntity(neutralEnvironmentEntity)

// Instantiate first creature
let adamEntity = chipaPool.getEntity()

if(adamEntity){
  let adam = new Creature(adamEntity, neutralEnvironment)

  adamEntity!.addComponent(adam)
  adam.transform.position = new Vector3(24, 0, 24)
  adam.TargetRandomPosition()
  BuildBody(adamEntity)
  adam.UpdateTemperatureText()
  adam.UpdateScale()
}

// Console Machine
let machine = new Entity()
machine.addComponent(
  new Transform({
    position: new Vector3(26, -0.5, 40),
    scale: new Vector3(2, 3, 6),
    rotation: Quaternion.Euler(0, 0, -35)
  })
)
engine.addEntity(machine)

let tempUp = new Entity()
tempUp.addComponent(
  new Transform({
    position: new Vector3(-0.1, 0.35, -0.325),
    scale: new Vector3(0.7, 0.5, 0.2),
    rotation: Quaternion.Euler(0, 0, 0)
  })
)
tempUp.addComponent(new GLTFShape("models/Button.glb"))
tempUp.addComponent(hotMaterial)
tempUp.addComponent(
  new OnClick(e => {
    neutralEnvironment.temperature += TemperatureButtonValue
    if (neutralEnvironment.temperature > 100) neutralEnvironment.temperature = 100

    tempUp.getComponent(ButtonData).pressed = true
    let b = neutralMaterial.albedoColor!.b
    let r = neutralMaterial.albedoColor!.r
    neutralMaterial.albedoColor = new Color3(r + 0.6, 0.5, b - 0.6)
    // neutralEnvironment.removeComponent(Material)
    neutralEnvironmentEntity.addComponentOrReplace(neutralMaterial)

    let tempInC = neutralEnvironment.temperature.toString()
    temperatureText.value = tempInC + "°"
    monitorTempText.value = tempInC + "°"

    for (let entity of creatures.entities) {
      let creature = entity.getComponent(Creature)

      creature.UpdateTemperatureText()
      creature.UpdateTemperatureIcons()
    }
  })
)
tempUp.addComponent(new ButtonData(14.5, 14.7))
engine.addEntity(tempUp)
tempUp.setParent(machine)

let tempDown = new Entity()
tempDown.addComponent(
  new Transform({
    position: new Vector3(0.3, 0.35, -0.325),
    scale: new Vector3(0.7, 0.5, 0.2),
    rotation: Quaternion.Euler(0, 0, 0)
  })
)
tempDown.addComponent(new GLTFShape("models/Button.glb"))
tempDown.addComponent(coldMaterial)
tempDown.addComponent(
  new OnClick(e => {
    neutralEnvironment.temperature -= TemperatureButtonValue
    if (neutralEnvironment.temperature < -100) neutralEnvironment.temperature = -100

    tempDown.getComponent(ButtonData).pressed = true
    //neutralMaterial.albedoColor.g = 85
    let b = neutralMaterial.albedoColor!.b
    let r = neutralMaterial.albedoColor!.r
    neutralMaterial.albedoColor = new Color3(r - 0.6, 0.5, b + 0.6)
    // neutralEnvironment.removeComponent(Material)
    neutralEnvironmentEntity.addComponentOrReplace(neutralMaterial)

    let tempInC = neutralEnvironment.temperature.toString()
    temperatureText.value = tempInC + "°"
    monitorTempText.value = tempInC + "°"

    for (let entity of creatures.entities) {
      let creature = entity.getComponent(Creature)

      creature.UpdateTemperatureText()
      creature.UpdateTemperatureIcons()
    }
  })
)
tempDown.addComponent(new ButtonData(14.5, 14.7))
engine.addEntity(tempDown)
tempDown.setParent(machine)

let thermometer = new Entity()
let temperatureText = new TextShape(neutralEnvironment.temperature.toString() + "°")
temperatureText.fontSize = 4
temperatureText.hTextAlign = "center"
temperatureText.vTextAlign = "center"
thermometer.addComponent(temperatureText)

thermometer.addComponent(
  new Transform({
    position: new Vector3(0, 0.575, 0.15),
    scale: new Vector3(0.2, 0.5, 0.8),
    rotation: Quaternion.Euler(90, 0, 90)
  })
)
engine.addEntity(thermometer)
thermometer.setParent(machine)

let thermometerIconEntity = new Entity()
thermometerIconEntity.setParent(thermometer)
thermometerIconEntity.addComponent(new Transform({
  position: new Vector3(-1, 0, 0),
  rotation: Quaternion.Euler(0, 0, 0),
  scale: new Vector3(0.5, 0.5, 0)
}))
thermometerIconEntity.addComponent(new PlaneShape())
thermometerIconEntity.addComponent(neutralIconMaterial)
engine.addEntity(thermometerIconEntity)

let creaturemeter = new Entity()
export let creaturemeterText = new TextShape("1/10")
creaturemeterText.fontSize = 4
creaturemeterText.hTextAlign = "center"
creaturemeterText.vTextAlign = "center"
creaturemeter.addComponent(creaturemeterText)

creaturemeter.addComponent(
  new Transform({
    position: new Vector3(0.325, 0.57, 0.15),
    scale: new Vector3(0.2, 0.5, 0.8),
    rotation: Quaternion.Euler(90, 0, 90)
  })
)
engine.addEntity(creaturemeter)
creaturemeter.setParent(machine)

let creaturemeterIconEntity = new Entity()
creaturemeterIconEntity.setParent(creaturemeter)
creaturemeterIconEntity.addComponent(new Transform({
  position: new Vector3(-1, 0, 0),
  scale: new Vector3(0.5, 0.5, 0),
  rotation: Quaternion.Euler(0, 0, 180)
}))
creaturemeterIconEntity.addComponent(new PlaneShape())
creaturemeterIconEntity.addComponent(chippaIconMaterial)
engine.addEntity(creaturemeterIconEntity)

// Big Monitor Info
let monitorThermometer = new Entity()
let monitorTempText = new TextShape(neutralEnvironment.temperature.toString() + "°")
monitorTempText.fontSize = 15
monitorTempText.hTextAlign = "center"
monitorTempText.vTextAlign = "center"
monitorThermometer.addComponent(monitorTempText)

monitorThermometer.addComponent(
  new Transform({
    position: new Vector3(3.1, 6.7, 38.5),
    rotation: Quaternion.Euler(0, -90, 0)
  })
)
engine.addEntity(monitorThermometer)

let monitorThermometerIconEntity = new Entity()
monitorThermometerIconEntity.setParent(monitorThermometer)
monitorThermometerIconEntity.addComponent(new Transform({
  position: new Vector3(-3, 0, 0),
  scale: new Vector3(1.5, 1.5, 0)
}))
monitorThermometerIconEntity.addComponent(new PlaneShape())
monitorThermometerIconEntity.addComponent(neutralIconMaterial)
engine.addEntity(monitorThermometerIconEntity)

let monitorCreaturemeter = new Entity()
export let monitorCreaturemeterText = new TextShape("1/10")
monitorCreaturemeterText.fontSize = 15
monitorCreaturemeterText.hTextAlign = "center"
monitorCreaturemeterText.vTextAlign = "center"
monitorCreaturemeter.addComponent(monitorCreaturemeterText)

monitorCreaturemeter.addComponent(
  new Transform({
    position: new Vector3(3.1, 4.7, 38.5),
    rotation: Quaternion.Euler(0, -90, 0)
  })
)
engine.addEntity(monitorCreaturemeter)

let monitorCreaturemeterIconEntity = new Entity()
monitorCreaturemeterIconEntity.setParent(monitorCreaturemeter)
monitorCreaturemeterIconEntity.addComponent(new Transform({
  position: new Vector3(-3, 0, 0),
  scale: new Vector3(1.5, 1.5, 0),
  rotation: Quaternion.Euler(0, 0, 180)
}))
monitorCreaturemeterIconEntity.addComponent(new PlaneShape())
monitorCreaturemeterIconEntity.addComponent(chippaIconMaterial)
engine.addEntity(monitorCreaturemeterIconEntity)

SetCheckGameWinConditionsFunction(function() {
  if(gameOver) return

	if(coldEnvironment.creatures.length >= 3) {
    let comfyCreatures: number = 0

    for (let creature of coldEnvironment.creatures) {
      if(creature.IsAtIdealTemperature()) comfyCreatures++
    }

    if(comfyCreatures < 3 || hotEnvironment.creatures.length < 3) return

    for (let creature of hotEnvironment.creatures) {
      if(creature.IsAtIdealTemperature()) comfyCreatures++
    }

    if(comfyCreatures < 6) return

    // YOU WIN!!!
    SetGameOverValue(true)

    let uiCanvas = new UICanvas()
    let winImage = new UIImage(uiCanvas, winPanelTex)
    winImage.width = 512
    winImage.height = 512
    winImage.sourceWidth = 512
    winImage.sourceHeight = 512
    winImage.hAlign = "center"
    winImage.vAlign = "center"
  }
})

  SetCheckGameLoseConditionsFunction(function() {
  if(gameOver) return

  log("begin environments check")
	for (let environment of environments.entities) {

    log(environment.getComponent(Environment).creatures.length)
    if(environment.getComponent(Environment).creatures.length > 0) return
  }

  // YOU LOST - REFRESH THE TAB AND GIVE IT ANOTHER SHOT!
  SetGameOverValue(true)

  let uiCanvas = new UICanvas()
  let panel = new UIContainerRect(uiCanvas)
  panel.width = "100%"
  panel.height = "50%"
  panel.positionY = 5
  panel.color = new Color4(0, 0, 0, 0.75)
  let lostText = new UIText(panel)
  lostText.value = "YOU LOST!\n\nREFRESH THE TAB AND GIVE IT ANOTHER SHOT!"
  lostText.color = Color4.Yellow()
  lostText.outlineColor = Color4.Magenta()
  lostText.fontSize = 45
  lostText.outlineWidth = 0.1
  lostText.vAlign = "center"
  lostText.hAlign = "center"
  lostText.vTextAlign = "center"
  lostText.hTextAlign = "center"
})

// Configure callback for updating texts when the creatures count change
neutralEnvironment.onCreaturesCountUpdated = function(creaturesCount: number) {
  if(creaturemeterText){
    creaturemeterText.value = creaturesCount + "/10"
    monitorCreaturemeterText.value = creaturesCount + "/10"
  }
}
