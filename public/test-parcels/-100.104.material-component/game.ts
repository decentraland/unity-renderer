import {
  engine,
  Material,
  BoxShape,
  Entity,
  Transform,
  Color3,
  OnPointerDown,
  getComponentId,
  log
} from 'decentraland-ecs/src'

const box = new BoxShape()

const niceMaterial = new Material()
niceMaterial.albedoColor = Color3.FromHexString('#FF0000')
niceMaterial.metallic = 0.9
niceMaterial.roughness = 0.1
log('niceMaterial', getComponentId(niceMaterial as any))

const niceNewMaterial = new Material()
niceNewMaterial.albedoColor = Color3.FromHexString('#00FFFF')
niceNewMaterial.metallic = 0.9
niceNewMaterial.roughness = 1
log('niceNewMaterial', getComponentId(niceNewMaterial as any))

function spawn(x: number, y: number, z: number) {
  const ent = new Entity()
  const transform = new Transform()
  transform.position.x = x
  transform.position.y = y
  transform.position.z = z

  ent.addComponentOrReplace(box)
  ent.addComponentOrReplace(niceMaterial)
  ent.addComponentOrReplace(transform)

  ent.addComponentOrReplace(
    new OnPointerDown(() => {
      log('setting ' + ent.uuid + ' <- ' + getComponentId(niceNewMaterial as any))
      ent.addComponentOrReplace(niceNewMaterial)
    })
  )

  engine.addEntity(ent)

  return ent
}

declare var dcl: any
const ent = spawn(5, 1, 5)

dcl.onEvent((evt: any) => {
  if (evt.type === 'TEST_TRIGGER') {
    log('TEST_TRIGGER', evt)
    ent.getComponent(OnPointerDown).callback({} as any)
  }
})

for (let i = 0; i < 10; i++) {
  spawn(Math.random() * 8, Math.random() * 8 + 1, Math.random() * 8)
}
