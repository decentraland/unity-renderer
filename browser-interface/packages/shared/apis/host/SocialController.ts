import { RpcServerPort } from '@dcl/rpc'
import { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import {
  GetAvatarEventsResponse,
  SocialControllerServiceDefinition,
  SocialEvent
} from '@dcl/protocol/out-ts/decentraland/kernel/apis/social_controller.gen'
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
