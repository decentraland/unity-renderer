import { engine, Vector3 } from 'decentraland-ecs/src'
import { Cube } from './cube'
import { RotationComponent } from './rotationComponent'
import { RotationSystem } from './rotationSystem'

const cubesCount = 740
const rotationRation = 0.25

for (let i = 0; i < cubesCount; i++) {
  let cube = new Cube(GetRandomPosition(), GetRandomScale())
  if (ShouldRotate()) {
    cube.addComponent(new RotationComponent(GetRandomRotationAxis(), GetRandomRotationSpd()))
  }
  engine.addEntity(cube)
}

engine.addSystem(new RotationSystem())

function GetRandomPosition(): Vector3 {
  let x = 1.5 + Math.random() * 45
  let z = 1.5 + Math.random() * 61
  let y = Math.random() * 10

  return new Vector3(x, y, z)
}

function GetRandomScale(): Vector3 {
  let r = Math.random() + 0.5
  return new Vector3(r, r, r)
}

function ShouldRotate(): boolean {
  return Math.random() <= rotationRation
}

function GetRandomRotationAxis(): Vector3 {
  let v = new Vector3(Math.random() * 20, Math.random() * 20, Math.random() * 20)
  return v
}

function GetRandomRotationSpd(): number {
  let mul = 1
  if (Math.random() < 0.5) {
    mul = -1
  }
  let spd = 180 + Math.random() * 180
  return spd * mul
}
