import { executeTask } from '@dcl/legacy-ecs'
import { avatarMap, avatarMessageObservable } from './avatar/avatarSystem'

declare const dcl: DecentralandInterface

// Initialize avatar profile scene

void executeTask(async () => {
  const [_, socialController] = await Promise.all([
    dcl.loadModule('@decentraland/Identity', {}),
    dcl.loadModule('@decentraland/SocialController', {})
  ])

  dcl.onUpdate(async (_dt) => {
    const ret: { events: { event: string; payload: string }[] } = await dcl.callRpc(
      socialController.rpcHandle,
      'pullAvatarEvents',
      []
    )

    let lastProcessed = ''
    for (const { payload } of ret.events) {
      if (payload !== lastProcessed) {
        try {
          lastProcessed = payload
          const msg = JSON.parse(payload)
          const invisible = avatarMap.get(msg.userId)?.visible === false && msg.visible === false
          if (!invisible) avatarMessageObservable.emit('message', msg)
        } catch (err) {
          console.error(err)
        }
      }
    }
  })
})
