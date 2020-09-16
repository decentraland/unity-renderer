import { getUserPublicKey } from '@decentraland/Identity'

const cube = new Entity()
const shape = new BoxShape()
const material = new Material()

material.roughness = 1
material.metallic = 0

executeTask(async () => {
  try {
    const pub = await getUserPublicKey()

    if (!pub || pub.indexOf('0x') !== 0) {
      throw new Error('invalid public key')
    }

    material.albedoColor = Color3.FromHexString('#00FF00')
  } catch {
    material.albedoColor = Color3.FromHexString('#FF0000')
  }
})

cube.addComponentOrReplace(material)
cube.addComponentOrReplace(shape)
cube.addComponentOrReplace(
  new Transform({
    position: new Vector3(5, 1, 5)
  })
)

engine.addEntity(cube)
