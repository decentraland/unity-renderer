import { getCurrentRealm } from '@decentraland/EnvironmentAPI'

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
  const currentRealm = await getCurrentRealm()
  text.value = `You are in the realm: ${JSON.stringify(currentRealm)}`
})
