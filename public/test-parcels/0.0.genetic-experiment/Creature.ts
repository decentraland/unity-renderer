import { Component, OnClick, ISystem, engine, Vector3, TextShape, IEntity, Transform, GLTFShape, Entity, Quaternion, PlaneShape, Color3, log, Scalar } from 'decentraland-ecs/src'

import { Genome, GeneType } from "./Genome"
import { ProgressBar } from "./ProgressBar"
import { Environment } from "./Environment"
import { Pool } from "./ObjectPool"
import { GrabableObjectComponent, grabObject } from "./grabableObjects";
import { framesBetweenDamage, coldIconMaterial, hotIconMaterial, MaxCreatureScale, MinCreatureScale, TemperatureButtonValue, MinTemperatureDiffForDamage, CheckGameLoseConditions, DamageCoeff } from './Params';

const MAX_CREATURES_AMOUNT = 10

export let chipaPool = new Pool(MAX_CREATURES_AMOUNT)

// Components
@Component("creature")
export class Creature {
  health: number = 100
  damageCounter: number = framesBetweenDamage
  healthBar: ProgressBar
  name: string
  oldPos: Vector3 = Vector3.Zero()
  nextPos: Vector3 = Vector3.Zero()
  movementFraction: number = 1
  movementPauseTimer: number = 0
  transform: Transform
  genome: Genome
  shape: GLTFShape | null
  temperatureText: TextShape
  entity: IEntity
  coldIconEntityTransform: Transform
  hotIconEntityTransform: Transform
  environment: Environment | null
  grabableObjectComponent: GrabableObjectComponent

  constructor(entity: IEntity, environment: Environment | null) {
    this.shape = null
    this.environment = null
    this.entity = entity

    this.transform = new Transform()

	  entity.addComponent(this.transform)

    this.grabableObjectComponent = new GrabableObjectComponent()
	  entity.addComponent(this.grabableObjectComponent)

    //let speed = 0.5
    let size = 0.5
    let temperature = 20
    let ears = 0.5
    let eyes = 0.5
    let feet = 0.5
    let mouth = 0.5
    let nose = 0.5
    let tail = 0.5
    let wings = 0.5

    this.genome = new Genome([size, temperature, ears, eyes, feet, mouth, nose, tail, wings])
    entity.addComponent(this.genome)

    let nameTextEntity = new Entity()
    nameTextEntity.setParent(entity)
    this.name = RandomizeName()
    let nameText = new TextShape(this.name)
    nameText.fontSize = 3
    nameText.color = Color3.White()
    nameText.hTextAlign = "center"
    nameText.vTextAlign = "center"
    nameText.billboard = true
    nameTextEntity.addComponent(nameText)
    nameTextEntity.addComponent(
      new Transform({
        position: new Vector3(0, 1.8, 0)
      })
	  )
    // engine.addEntity(nameTextEntity)

    let temperatureTextEntity = new Entity()
    temperatureTextEntity.setParent(nameTextEntity)
    this.temperatureText = new TextShape(temperature + "°")
    this.temperatureText.fontSize = 2.75
    this.temperatureText.color = Color3.Green()
    this.temperatureText.hTextAlign = "center"
    this.temperatureText.vTextAlign = "center"
    this.temperatureText.billboard = true
    temperatureTextEntity.addComponent(this.temperatureText)
    temperatureTextEntity.addComponent(
      new Transform({
        position: new Vector3(0, -0.3, 0)
      })
    )
  // engine.addEntity(temperatureTextEntity)

    let healthBarEntity = new Entity()
    healthBarEntity.setParent(nameTextEntity)
    healthBarEntity.addComponent(
      new Transform({
        position: new Vector3(0, -0.6, 0),
        rotation: Quaternion.Euler(0, 180, 0)
      })
    )
    this.healthBar = new ProgressBar(healthBarEntity)
    healthBarEntity.addComponent(this.healthBar)
    // engine.addEntity(healthBarEntity)

    let coldIconEntity = new Entity()
    coldIconEntity.setParent(temperatureTextEntity)
    this.coldIconEntityTransform = new Transform({
                                  position: new Vector3(0.45, 0, 0),
                                  rotation: Quaternion.Euler(0, 0, 0),
                                  scale: new Vector3(0, 0, 0)
                                })
    coldIconEntity.addComponent(this.coldIconEntityTransform)
    coldIconEntity.addComponent(new PlaneShape())
    coldIconEntity.addComponent(coldIconMaterial)
    // engine.addEntity(this.coldIconEntity)

    let hotIconEntity = new Entity()
    hotIconEntity.setParent(temperatureTextEntity)
    this.hotIconEntityTransform = new Transform({
                                position: new Vector3(-0.45, 0, 0),
                                rotation: Quaternion.Euler(0, 0, 0),
                                scale: new Vector3(0, 0, 0)
                              })
    hotIconEntity.addComponent(this.hotIconEntityTransform)
    hotIconEntity.addComponent(new PlaneShape())
    hotIconEntity.addComponent(hotIconMaterial)
    // engine.addEntity(this.coldIconEntity)

    this.SetEnvironment(environment)

    this.UpdateScale()

    engine.addEntity(entity)
  }

  TargetRandomPosition() {
    if(!this.environment) return

    this.oldPos = this.transform.position
	  this.oldPos.y = 0
	  this.nextPos = newCenteredRandomPos(this.environment.position , this.environment.size)

    this.movementFraction = 0

    this.transform.lookAt(this.nextPos)
  }

  SpawnChild() {
    if (creatures.entities.length >= MAX_CREATURES_AMOUNT) return

    let sonEntity = chipaPool.getEntity()
    if (!sonEntity) return

    let childCreature = new Creature(sonEntity, this.environment ? this.environment : null)
    sonEntity.addComponentOrReplace(childCreature)

	  childCreature.transform.position = this.transform.position.clone()

    childCreature.TargetRandomPosition()

    childCreature.genome.CopyFrom(this.genome)
    childCreature.Mutate()
    BuildBody(sonEntity)

    childCreature.movementPauseTimer = Math.random() * 5
  }

  Mutate() {
    this.genome.Mutate()
    this.UpdateTemperatureText()

    this.UpdateScale()

    this.UpdateTemperatureIcons()
  }

  UpdateScale(){
    let sizeFactor = Math.abs((-1*(this.genome.genes[GeneType.speed]))+ 1)
    let size = Scalar.Lerp(MinCreatureScale, MaxCreatureScale, sizeFactor)
    this.transform.scale.x = size
    this.transform.scale.y = size
    this.transform.scale.z = size
  }

  UpdateHealthbar() {
    this.healthBar.UpdateNormalizedValue(this.health / 100)
  }

  UpdateTemperatureText() {
    this.temperatureText.value = this.genome.genes[GeneType.temperature] + "°"

    if(this.IsAtIdealTemperature())
      this.temperatureText.color = Color3.Green()
    else
      this.temperatureText.color = Color3.Red()
  }

  UpdateTemperatureIcons(){
    if(!this.environment){
      this.hotIconEntityTransform.scale.set(0, 0, 0)
      this.coldIconEntityTransform.scale.set(0, 0, 0)
      return
    }

    let hotterEnvironmentTemp = this.environment.temperature + TemperatureButtonValue
    let colderEnvironmentTemp = this.environment.temperature - TemperatureButtonValue

    if(Math.abs(hotterEnvironmentTemp - this.genome.genes[GeneType.temperature]) <= MinTemperatureDiffForDamage)
      this.hotIconEntityTransform.scale.set(0.25, 0.25, 0.25)
    else
      this.hotIconEntityTransform.scale.set(0, 0, 0)

    if(Math.abs(colderEnvironmentTemp - this.genome.genes[GeneType.temperature]) <= MinTemperatureDiffForDamage)
      this.coldIconEntityTransform.scale.set(0.25, 0.25, 0.25)
    else
      this.coldIconEntityTransform.scale.set(0, 0, 0)
  }

  takeDamage() {
    if(this.IsAtIdealTemperature()) return

    let temperatureDif = this.GetTemperatureDif()

	  let temperatureDamage = temperatureDif * temperatureDif * DamageCoeff

    //  to make damage proportional to speed  (more speed, smaller, more fragile)
    let damageForSize = temperatureDamage * this.genome.genes[GeneType.speed]

    this.health -= damageForSize

    if (this.health < 0) this.health = 0

    this.UpdateHealthbar()

  }

  IsAtIdealTemperature(): boolean {
    return this.GetTemperatureDif() <= MinTemperatureDiffForDamage
  }

  GetTemperatureDif(){
    if(!this.environment)
      return 0

    return Math.abs(this.environment.temperature - this.genome.genes[GeneType.temperature])
  }

  SetEnvironment(newEnvironment: Environment | null){
    if(newEnvironment == this.environment) return

    if(this.environment) this.environment.removeCreature(this)

    this.environment = newEnvironment

    if(this.environment)
      this.environment.addCreature(this)

    this.UpdateTemperatureIcons()
  }
}
export const creatures = engine.getComponentGroup(Creature)

// Systems
export class DieSLowly implements ISystem {
  update(dt: number) {
    for (let entity of creatures.entities) {
      let creature = entity.getComponent(Creature)

	    if (creature.environment == null) continue

	    creature.damageCounter -= 1

	    if (creature.damageCounter < 0) {
        creature.takeDamage()

        if (creature.health <= 0) {
          log("RIP")
          creature.environment.removeCreature(creature)
          ClearCreatureEntity(entity)
          engine.removeEntity(entity)

          CheckGameLoseConditions()
        }

        creature.damageCounter = framesBetweenDamage
      }
    }
  }
}
engine.addSystem(new DieSLowly())

export class Wander implements ISystem {
  sinTime: number = 0

  update(dt: number) {
    for (let entity of creatures.entities) {

      let creature = entity.getComponent(Creature)

      if (creature.grabableObjectComponent.grabbed) continue

      if (creature.movementPauseTimer > 0) {
        creature.movementPauseTimer -= dt

        if (creature.movementPauseTimer > 0) continue
      }

      if (creature.movementFraction >= 1) continue

	    let speed = creature.genome.genes[GeneType.speed]

      creature.movementFraction += speed * dt
      if (creature.movementFraction > 1) {
        creature.movementFraction = 1
      }

      creature.transform.position = Vector3.Lerp(
        creature.oldPos,
        creature.nextPos,
        creature.movementFraction
        )

      this.sinTime += dt * speed * 4
      let verticalOffset = Math.abs(Math.sin(this.sinTime)) * Math.abs(creature.genome.genes[GeneType.temperature]/30)
      creature.transform.position.y = verticalOffset

      // reached destination
      if (creature.movementFraction == 1) {
        creature.movementPauseTimer = Math.random() * 20
        creature.transform.position.y = 0

        if (
          Math.random() < 0.7 // 70% chance of spawning a child
          && creature.environment && creature.environment.size == 8
        ) {
          creature.SpawnChild()
        }

        creature.TargetRandomPosition()
      }
    }
  }
}
engine.addSystem(new Wander())

// Extra functions
export function newCenteredRandomPos(centerPos: Vector3, radius: number) {
  let randomPos = new Vector3(Math.random() * radius, 0, Math.random() * radius)

  if (Math.random() < 0.5) randomPos.x *= -1

  if (Math.random() < 0.5) randomPos.z *= -1

  return Vector3.Add(centerPos, randomPos)
}

function RandomizeName() {
  let randomNumber = Math.random()

  if (randomNumber < 0.01) {
    return "Pumbi"
  } else if (randomNumber < 0.05) {
    return "Sasha"
  } else if (randomNumber < 0.1) {
    return "Pumpi"
  } else if (randomNumber < 0.15) {
    return "Bimbo"
  } else if (randomNumber < 0.175) {
    return "Falopon"
  } else if (randomNumber < 0.2) {
    return "Troncho"
  } else if (randomNumber < 0.25) {
    return "Mika"
  } else if (randomNumber < 0.3) {
    return "Plinky"
  } else if (randomNumber < 0.35) {
    return "Faloppy"
  } else if (randomNumber < 0.4) {
    return "Sputnik"
  } else if (randomNumber < 0.45) {
    return "Satoshi"
  } else if (randomNumber < 0.475) {
    return "Bilbo"
  } else if (randomNumber < 0.5) {
    return "Falchor"
  } else if (randomNumber < 0.55) {
    return "Pipo"
  } else if (randomNumber < 0.575) {
    return "Keanu"
  } else if (randomNumber < 0.6) {
    return "Kinky"
  } else if (randomNumber < 0.65) {
    return "Buddy"
  } else if (randomNumber < 0.675) {
    return "Ryan"
  } else if (randomNumber < 0.7) {
    return "Slimy"
  } else if (randomNumber < 0.75) {
    return "JoJo"
  } else if (randomNumber < 0.775) {
    return "OraOraOra"
  } else if (randomNumber < 0.8) {
    return "Chippy"
  } else if (randomNumber < 0.85) {
    return "Chiffy"
  } else if (randomNumber < 0.875) {
    return "Satatus"
  } else if (randomNumber < 0.9) {
    return "Chippu"
  } else if (randomNumber < 0.95) {
    return "Kurtnus"
  } else {
    return "Kax"
  }
}

function ClearCreatureEntity(entity: IEntity) {
  for (const key in entity.components) {
    entity.removeComponent(entity.components[key])
  }

  for (const key in entity.children) {
    ClearCreatureEntity(entity.children[key])

    engine.removeEntity(entity.children[key])
  }
}

///  GLTF declarations

//body
let neutralChipaBody = new GLTFShape("models/Creature/Body.glb")
let winterChipaBody1 = new GLTFShape("models/Creature/Winter_Lv1.glb")
let winterChipaBody2 = new GLTFShape("models/Creature/Winter_Lv2.glb")
let summerChipaBody1 = new GLTFShape("models/Creature/Heat_Lv1.glb")
let summerChipaBody2 = new GLTFShape("models/Creature/Heat_Lv2.glb")



//feet
let feet_spider = new GLTFShape("models/Creature/Feet_Spider.glb")
let feet_big = new GLTFShape("models/Creature/Feet_Big.glb")
let feet_centi = new GLTFShape("models/Creature/Feet_Centipede.glb")

//ears
let ears_acua = new GLTFShape("models/Creature/Ears_AcuaticFin.glb")
let ears_cat = new GLTFShape("models/Creature/Ears_Cat.glb")
let ears_bear = new GLTFShape("models/Creature/Ears_Bear.glb")
let ears_cute = new GLTFShape("models/Creature/Ears_Cute.glb")
let ears_bunny = new GLTFShape("models/Creature/Ears_Bunny.glb")

//eyes
let eyes_cyclop = new GLTFShape("models/Creature/Eyes_Cyclop.glb")
let eyes_biclop = new GLTFShape("models/Creature/Eyes_Biclop.glb")
let eyes_nerd = new GLTFShape("models/Creature/Eyes_Nerd.glb")
let eyes_nerdor = new GLTFShape("models/Creature/Eyes_Nerdor.glb")
let eyes_spider = new GLTFShape("models/Creature/Eyes_Spider.glb")

// mouth
let mouth_acuatic = new GLTFShape("models/Creature/Mouth_Acuatic.glb")
let mouth_smile = new GLTFShape("models/Creature/Mouth_Smile.glb")
let mouth_fangs = new GLTFShape("models/Creature/Mouth_Fangs.glb")

// nose
let nose_bear = new GLTFShape("models/Creature/Nose_Bear.glb")
let nose_horn = new GLTFShape("models/Creature/Nose_Horn.glb")

// tail
let tail_acuatic = new GLTFShape("models/Creature/Tail_Acuatic.glb")
let tail_pig = new GLTFShape("models/Creature/Tail_Pig.glb")

// wings
let wings_acuatic = new GLTFShape("models/Creature/Wings_Acuatic.glb")
let wings_bat = new GLTFShape("models/Creature/Wings_Bat.glb")
let wings_dragon = new GLTFShape("models/Creature/Wings_Dragon.glb")

export function BuildBody(creature: IEntity){
	let genes = creature.getComponent(Genome).genes

	let temperature = genes[GeneType.temperature]
	let body = new Entity()
	body.setParent(creature)
	body.addComponent(neutralChipaBody)




	body.addComponentOrReplace(
		new OnClick(() => {
      let parent = body.getParent()
      if(!parent) return

			if (!parent.getComponent(GrabableObjectComponent).grabbed ){
			  grabObject(parent)
      }
		})
	  )


	if (temperature < -30) {
		let coat = new Entity()
		coat.addComponent(winterChipaBody2)
		coat.setParent(creature)
	  } else if (temperature < 5) {
		let coat = new Entity()
		coat.addComponent(winterChipaBody1)
		coat.setParent(creature)
	  } else if (temperature < 30) {
		// normal chipa
	  } else if (temperature < 70) {
		// heat 1
		let coat = new Entity()
		coat.addComponent(summerChipaBody1)
		coat.setParent(creature)
	  } else if (temperature >= 70) {
		// heat 2
		let coat = new Entity()
		coat.addComponent(summerChipaBody2)
		coat.setParent(creature)
	}

	let feetGene = genes[GeneType.feet]
	let feet = new Entity()
	feet.setParent(creature)

	if (feetGene < 0.3) {
		feet.addComponent(feet_centi)
	  } else if (feetGene < 0.7) {
		feet.addComponent(feet_big)
	  } else if (feetGene <= 1) {
		feet.addComponent(feet_spider)
	}

	let earsGene = genes[GeneType.ears]
	let ears = new Entity()
	ears.setParent(creature)

	if (earsGene < 0.15) {
		ears.addComponent(ears_acua)
	  } else if (earsGene < 0.30) {
		ears.addComponent(ears_cat)
	  } else if (earsGene < 0.45) {
		ears.addComponent(ears_bear)
	  } else if (earsGene < 0.70) {
		// no ears
	  } else if (earsGene < 0.85) {
		ears.addComponent(ears_cute)
	  } else if (earsGene <= 1) {
		ears.addComponent(ears_bunny)
	}


	let eyesGene = genes[GeneType.eyes]
	let eyes = new Entity()
	eyes.setParent(creature)

	if (eyesGene < 0.2) {
		eyes.addComponent(eyes_cyclop)
	  } else if (eyesGene < 0.4) {
		eyes.addComponent(eyes_biclop)
	  } else if (eyesGene < 0.6) {
		eyes.addComponent(eyes_nerd)
	  } else if (eyesGene < 0.8) {
		eyes.addComponent(eyes_nerdor)
	  } else if (eyesGene <= 1) {
		eyes.addComponent(eyes_spider)
	}


	let mouthGene = genes[GeneType.mouth]
	let mouth = new Entity()
	mouth.setParent(creature)

	if (mouthGene < 0.2) {
		mouth.addComponent(mouth_acuatic)
	  } else if (mouthGene < 0.4) {
		// no mouth
	  } else if (mouthGene < 0.6) {
		mouth.addComponent(mouth_smile)
	  } else if (mouthGene < 0.8) {
		mouth.addComponent(mouth_fangs)
	}

	let noseGene = genes[GeneType.nose]
	let nose = new Entity()
	nose.setParent(creature)

	if (noseGene < 0.3) {
		nose.addComponent(nose_bear)
	  } else if (noseGene < 0.7) {
		// no nose
	  } else if (noseGene <= 1) {
		nose.addComponent(nose_horn)
	}

	let tailGene = genes[GeneType.tail]
	let tail = new Entity()
	tail.setParent(creature)

	if (tailGene < 0.3) {
		tail.addComponent(tail_acuatic)
	  } else if (tailGene < 0.7) {
		// no tail
	  } else if (tailGene <= 1) {
		tail.addComponent(tail_pig)
	}

	let wingsGene = genes[GeneType.wings]
	let wings = new Entity()
	wings.setParent(creature)

	if (wingsGene < 0.2) {
		wings.addComponent(wings_acuatic)
	  } else if (wingsGene < 0.6) {
		// no mouth
	  } else if (wingsGene < 0.8) {
		wings.addComponent(wings_bat)
	  } else if (wingsGene <= 1) {
		wings.addComponent(wings_dragon)
	}



}

