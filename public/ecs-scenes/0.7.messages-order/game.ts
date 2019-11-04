import { getParcel } from '@decentraland/ParcelIdentity'

const cube = new Entity()

cube.addComponentOrReplace(
  new Transform({
    position: new Vector3(5, 1, 5)
  })
)

const text = new TextShape('Wait for it')
text.billboard = true
text.isPickable = true
cube.addComponentOrReplace(text)

engine.addEntity(cube)

executeTask(async () => {
  const { land } = await getParcel()
  text.value = `Scene with parcels ${JSON.stringify(land.scene.scene.parcels)}`
})
