import { saveScreenshot, enableVisualTests, wait, waitToBeLoaded } from '../testHelpers'
import { AvatarEntity, avatarContext } from 'dcl/entities/utils/AvatarEntity'
import { initHudSystem } from 'dcl/widgets/ui'

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
    avatar1.position.set(500, 1.5, 3.1)
    avatar1.parent = root

    avatar2 = new AvatarEntity('avatar2')

    avatar2.regenerateAvatar('round-robot')
    avatar2.rotation.set(0, 0, 0)
    avatar2.position.set(510, 1.5, 3.1)

    avatar2.parent = root

    avatar3 = new AvatarEntity('avatar3')

    avatar3.regenerateAvatar('fox')
    avatar3.rotation.set(0, 0, 0)
    avatar3.position.set(520, 1.5, 3.1)

    avatar3.parent = root
  })

  it('waits avatar2 to be loaded', async () => await waitToBeLoaded(avatar2))
  saveScreenshot(`avatar-round-robot.png`, { from: [510, 1.8, 6], lookAt: [510, 1.3, 3] })

  it('waits avatar1 to be loaded', async function() {
    await waitToBeLoaded(avatar1)
  })

  saveScreenshot(`avatar-square-robot.png`, {
    from: [500, 1.8, 6],
    lookAt: [500, 1.3, 3]
  })

  it('waits avatar3 to be loaded', async () => await waitToBeLoaded(avatar3))
  saveScreenshot(`avatar-fox.png`, { from: [520, 1.8, 6], lookAt: [520, 1.3, 3] })

  it('open profile ui for avatar1', async () => {
    avatar1.setAttributes(playerProfile)
    avatar1.dispatchUUIDEvent('onClick', {
      pointerId: 1
    })
  })

  wait(2000)

  saveScreenshot(`avatar-profile-ui.png`, { from: [500, 1.8, 7], lookAt: [499, 1.3, 3] })

  wait(2000)

  it('disposes all avatars', async () => {
    avatar1.dispose()
    avatar1 = null

    avatar2.dispose()
    avatar2 = null

    avatar3.dispose()
    avatar3 = null

    // avatarContext.dispose()
    ;(await (await hud).worker).dispose()
  })
})
