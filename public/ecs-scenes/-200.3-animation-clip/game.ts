// 1 import { playSound, pauseSound } from '@decentraland/SoundController'

executeTask(async () => {
  try {
    // 1 await playSound('sneaky_menu.mp3', {
    // 1   loop: true,
    // 1   volume: 100
    // 1 })
  } catch {
    log('failed to play sound')
  }
})

const cube = new Entity()

cube.addComponentOrReplace(
  new Transform({
    position: new Vector3(5, 1, 5)
  })
)
cube.getComponentOrCreate(BoxShape)
cube.addComponentOrReplace(
  new OnPointerDown(() => {
    executeTask(async () => {
      // 1 await pauseSound()
    })
  })
)

engine.addEntity(cube)
