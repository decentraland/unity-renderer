import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'

import type {
  GetAvatarEventsResponse,
  SocialEvent
} from 'shared/protocol/decentraland/kernel/apis/social_controller.gen'
import { SocialControllerServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/social_controller.gen'
import { avatarMessageObservable } from 'shared/comms/peers'

export function registerSocialControllerServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, SocialControllerServiceDefinition, async (port, _ctx) => {
    let pendingAvatarEvents: SocialEvent[] = []
    const observer = avatarMessageObservable.add((event) => {
      pendingAvatarEvents.push({
        event: event.type,
        payload: JSON.stringify(event)
      })
    })

    port.on('close', () => {
      avatarMessageObservable.remove(observer)
    })

    return {
      async pullAvatarEvents(): Promise<GetAvatarEventsResponse> {
        const events = pendingAvatarEvents
        pendingAvatarEvents = []
        return { events }
      }
    }
  })
}
