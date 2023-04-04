import { executeTask } from '@dcl/legacy-ecs'
import { avatarMessageObservable } from './avatar/avatarSystem'

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
          avatarMessageObservable.emit('message', JSON.parse(payload))
        } catch (err) {
          console.error(err)
        }
      }
    }
  })
})
