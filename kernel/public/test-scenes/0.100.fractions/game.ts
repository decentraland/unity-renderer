import { Entity, engine, Vector3, Transform, BoxShape, log } from 'decentraland-ecs/src'

/// --- Spawner function ---

function spawnCube(x: number, y: number, z: number) {
  // create the entity
  const cube = new Entity()

  // add a transform to the entity
  cube.addComponent(
    new Transform({
      position: new Vector3(x, y, z),
      scale: new Vector3(0.1, 0.1, 0.1)
    })
  )

  // add a shape to the entity
  cube.addComponent(new BoxShape())

  // add the entity to the engine
  engine.addEntity(cube)

  return cube
}

/// --- Spawn cubes ---
let y = 0
let x = 0.125
let a = 0
for (let i = 0; i < 12; i = i + 0.1) {
  spawnCube(x, y, 8)
  log(`${a}: y value: ${y}`)
  x = x + 0.125
  y = y + 0.1
  a++
}
