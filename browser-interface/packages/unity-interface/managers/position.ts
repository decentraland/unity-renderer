import { defaultLogger } from 'lib/logger'
import { notifyStatusThroughChat } from 'shared/chat'
import { changeRealm } from 'shared/dao'
import { WorldPosition } from 'shared/types'
import { TeleportController } from 'shared/world/TeleportController'
import { getUnityInstance } from 'unity-interface/IUnityInterface'

export function handleJumpIn(data: WorldPosition) {
  const {
    gridPosition: { x, y },
    realm: { serverName }
  } = data

  notifyStatusThroughChat(`Jumping to ${serverName} at ${x},${y}...`)

  changeRealm(serverName).then(
    () => {
      const successMessage = `Welcome to realm ${serverName}!`
      notifyStatusThroughChat(successMessage)
      getUnityInstance().ConnectionToRealmSuccess(data)
      TeleportController.goTo(x, y, successMessage).then(
        () => {},
        () => {}
      )
    },
    (e) => {
      const cause = e === 'realm-full' ? ' The requested realm is full.' : ''
      notifyStatusThroughChat('changerealm: Could not join realm.' + cause)
      getUnityInstance().ConnectionToRealmFailed(data)
      defaultLogger.error(e)
    }
  )
}
