import type { Vector3 } from 'lib/math/Vector3'
import { Avatar } from '@dcl/schemas'
import { getPerformanceInfo } from '../session/getPerformanceInfo'
import { ChatMessagePlayerType, ChatMessageType } from '../types'

type PositionTrackEvents = {
  ['Scene Spawn']: { parcel: string; spawnpoint: Vector3 }
}

export type TrackEvents = PositionTrackEvents & {
  // Comms & Chat Events
  ['Send chat message']: {
    messageId: string
    from: ChatMessagePlayerType
    to?: ChatMessagePlayerType
    length: number
    messageType: ChatMessageType
    source?: string
    receiverId: string | undefined
    channelName: string | undefined
  }
  ['Comms Status v2']: Record<string, any>
  ['bff_auth_already_connected']: { address: string }

  // Info logs, such as networks or things we want to track
  ['SNAPSHOT_IMAGE_NOT_FOUND']: { userId: string }
  ['fetchWearablesFromCatalyst_failed']: { wearableId: string }
  ['avatar_edit_success']: { userId: string; version: number; wearables: string[] }
  ['Move to Parcel']: { newParcel: string; oldParcel: string | null; exactPosition: Vector3 }
  ['motd_failed']: Record<string, unknown> // {}
  ['TermsOfServiceResponse']: { sceneId: string; accepted: boolean; dontShowAgain: boolean }
  ['error']: { context: string; message: string; stack: string; saga_stack?: string }
  ['error_fatal']: { context: string; message: string; stack: string; saga_stack?: string }
  ['long_chat_message_ignored']: { message: string; sender?: string }
  ['renderer_initialization_error']: { message: string }

  // Performance
  ['scene_loading_failed']: {
    sceneId: string
    contentServer: string
    contentServerBundles: string
    rootUrl: string
  }
  ['SceneLoadTimes']: {
    position: Vector3
    elapsed: number
    success: boolean
    sceneId: string
    userId: string
  }
  ['renderer_initializing_start']: Record<string, unknown> // {}
  ['renderer_initializing_end']: { loading_time: number }
  ['renderer_set_threw']: { object: string; method: string; payload: string; stack: string }
  ['lifecycle event']: { stage: string; retries?: unknown }
  ['performance report']: ReturnType<typeof getPerformanceInfo>
  ['system info report']: {
    graphicsDeviceName: string
    graphicsDeviceVersion: string
    graphicsMemorySize: number
    processorType: string
    processorCount: number
    systemMemorySize: number
  }
  ['unity_loader_downloading_start']: { renderer_version: string }
  ['unity_loader_downloading_end']: { renderer_version: string; loading_time: number }
  ['unity_downloading_start']: { renderer_version: string }
  ['unity_downloading_end']: { renderer_version: string; loading_time: number }
  ['unity_initializing_start']: { renderer_version: string }
  ['unity_initializing_end']: { renderer_version: string; loading_time: number }
  ['scene_start_event']: { scene_id: string; time_since_creation: number; base: string }
  ['invalid_schema']: { schema: string; payload: any; errors: string }
  // TODO - these are reintroduced for control, remove asap - moliva - 2022/06/01
  ['Control Friend request approved']: Record<string, never> // {}
  ['Control Friend request rejected']: Record<string, never> // {}
  ['Control Friend request cancelled']: Record<string, never> // {}
  ['Control Friend request received']: Record<string, never> // {}
  ['Control Friend request sent']: Record<string, never> // {}
  ['Control Friend deleted']: Record<string, never> // {}
  // TODO - the above metrics are reintroduced for control, remove asap - moliva - 2022/06/01
  ['Remote avatar for profile is invalid']: { avatar: Avatar }

  ['pickedRealm']: { algorithm: string; domain: string }
  ['errorInSceneWorker']: { message: string; scene: string; pointers: string[] }
  ['disconnect_lighthouse']: { message: string; reason: string; url: string }
  ['Invalid user version']: { address: string; version: number }
  ['non_json_message_from_engine']: { type: string; payload: string }
  ['invalid_denied_catalyst_url']: { url: string }
  ['invalid_comms_message_too_big']: { message: string }
  ['onboarding_started']: { onboardingRealm: string }
  ['disconnection_cause']: { context: string; message: string; liveKitRoomSid: string; liveKitParticipantSid: string }

  // Seamless login A/B
  ['seamless_login show tos']: Record<string, never> // {}
  ['seamless_login tos shown']: Record<string, never> // {}
  ['seamless_login tos accepted']: Record<string, never> // {}
  ['seamless_login tos rejected']: Record<string, never> // {}
  ['seamless_login go to tos']: Record<string, never> // {}
}
