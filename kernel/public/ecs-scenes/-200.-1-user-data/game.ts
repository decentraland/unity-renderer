import { getUserData } from '@decentraland/Identity'

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
  const { userId, displayName, publicKey, hasConnectedWeb3 } = (await getUserData())!
  text.value = `Hi ${displayName}! Your id is ${userId}, your public key is ${publicKey} and is ${
    hasConnectedWeb3 ? '' : 'not'
  } connected to Ethereum`
})
