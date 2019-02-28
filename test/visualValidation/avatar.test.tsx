import { saveScreenshot, enableVisualTests, wait, waitToBeLoaded } from '../testHelpers'
import { AvatarEntity, avatarContext } from 'dcl/entities/utils/AvatarEntity'
import { initHudSystem } from 'dcl/widgets/ui'
import { gridToWorld } from 'atomicHelpers/parcelScenePositions'

enableVisualTests('Avatar visual validation', function(root) {
  const playerProfile = {
    displayName: 'Test Avatar',
    publicKey: '0x55ed2910cc807e4596024266ebdf7b1753405a11',
    username: 'tester',
    status: 'Testing!!! 🎉🔥🚀'
  }

  let avatar1: AvatarEntity = null
  let avatar2: AvatarEntity = null
  let avatar3: AvatarEntity = null

  avatarContext.baseUrl = 'http://localhost:8080/'

  let hud: ReturnType<typeof initHudSystem>

  it('initHudSystem', async () => {
    hud = initHudSystem()
    await hud
  })

  it('wait for hud system', async () => {
    await (await hud).worker.system
  })

  it('creates a test scene with avatars', async () => {
    avatar1 = new AvatarEntity('avatar1')
    avatar1.regenerateAvatar('square-robot')
    avatar1.rotation.set(0, 0, 0)
    gridToWorld(50, 0.3, avatar1.position)
    avatar1.position.y = 1.5
    avatar1.parent = root

    avatar2 = new AvatarEntity('avatar2')

    avatar2.regenerateAvatar('round-robot')
    avatar2.rotation.set(0, 0, 0)
    gridToWorld(51, 0.3, avatar2.position)
    avatar2.position.y = 1.5
    avatar2.parent = root

    avatar3 = new AvatarEntity('avatar3')

    avatar3.regenerateAvatar('fox')
    avatar3.rotation.set(0, 0, 0)
    gridToWorld(52, 0.3, avatar3.position)
    avatar3.position.y = 1.5

    avatar3.parent = root
  })

  it('waits avatar2 to be loaded', async () => await waitToBeLoaded(avatar2))
  saveScreenshot(`avatar-round-robot.png`, { from: [51.0, 1.8, 0.6], lookAt: [51.0, 1.3, 0.3] })

  it('waits avatar1 to be loaded', async function() {
    await waitToBeLoaded(avatar1)
  })

  saveScreenshot(`avatar-square-robot.png`, {
    from: [50.0, 1.8, 0.6],
    lookAt: [50.0, 1.3, 0.3]
  })

  it('waits avatar3 to be loaded', async () => await waitToBeLoaded(avatar3))
  saveScreenshot(`avatar-fox.png`, { from: [52.0, 1.8, 0.6], lookAt: [52.0, 1.3, 0.3] })

  it('open profile ui for avatar1', async () => {
    avatar1.setAttributes(playerProfile)
    avatar1.dispatchUUIDEvent('onClick', {
      entityId: avatar1.uuid
    })
  })

  wait(2000)

  saveScreenshot(`avatar-profile-ui.png`, { from: [50.0, 1.8, 0.7], lookAt: [49.9, 1.3, 0.3] })

  wait(2000)

  it('disposes all avatars', async () => {
    avatar1.dispose()
    avatar1 = null

    avatar2.dispose()
    avatar2 = null

    avatar3.dispose()
    avatar3 = null

    avatarContext.disposableComponents.forEach((_, id) => {
      avatarContext.ComponentDisposed({ id })
    })

    // avatarContext.dispose()
    ;(await (await hud).worker).dispose()
  })
})
