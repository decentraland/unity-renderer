import { Component, engine, Vector3 } from 'decentraland-ecs/src'

import { Creature } from "./Creature";

@Component("environment")
export class Environment {
  temperature: number
  position: Vector3
  onCreaturesCountUpdated!: any
  size: number

  creatures: Creature[]

  constructor(temp: number, pos: Vector3, size: number) {
    this.temperature = temp
    this.position = pos
    this.size = size

    this.creatures = new Array()
  }

  addCreature(newCreature: Creature) {
    const newCreaturesCount = this.creatures.push(newCreature)

    if(this.onCreaturesCountUpdated)
      this.onCreaturesCountUpdated(newCreaturesCount)
  }

  removeCreature(newCreature: Creature) {

    // removes the creature from the array. This probably leads to memory leaks T_T, but needed right now XD
    this.creatures = this.creatures.filter(x => x != newCreature)

    if(this.onCreaturesCountUpdated)
      this.onCreaturesCountUpdated(this.creatures.length)
  }
}

export const environments = engine.getComponentGroup(Environment)
