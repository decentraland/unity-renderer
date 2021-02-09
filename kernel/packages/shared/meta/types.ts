import { Vector2Component } from 'atomicHelpers/landHelpers'
import future, { IFuture } from 'fp-future'
import { RenderProfile } from 'shared/types'
import { Color4 } from 'decentraland-ecs/src'

export let USE_UNITY_INDEXED_DB_CACHE: IFuture<boolean> = future()

export type MetaConfiguration = {
  explorer: {
    minBuildNumber: number
    useUnityIndexedDbCache: boolean
  }
  servers: {
    added: string[]
    denied: string[]
    contentWhitelist: string[]
    catalystsNodesEndpoint?: string
  }
  world: WorldConfig
  comms: CommsConfig
}

export type WorldConfig = {
  pois: Vector2Component[]
  renderProfile?: RenderProfile
  messageOfTheDay?: MessageOfTheDayConfig | null
  messageOfTheDayInit?: boolean
}

export type MessageOfTheDayConfig = {
  background_banner: string
  endUnixTimestamp?: number
  title: string
  body: string
  buttons: {
    caption: string
    action?: string
    // NOTE(Brian): The button actions will be global chat's actions,
    // for instance `/goto 0,0`, or 'Close' that will just close the MOTD.
    tint?: Color4
  }[]
}

export type MetaState = {
  initialized: boolean
  config: Partial<MetaConfiguration>
}

export type RootMetaState = {
  meta: MetaState
}

export type CommsConfig = {
  targetConnections?: number
  maxConnections?: number
  relaySuspensionDisabled?: boolean
  relaySuspensionInterval?: number
  relaySuspensionDuration?: number
}
