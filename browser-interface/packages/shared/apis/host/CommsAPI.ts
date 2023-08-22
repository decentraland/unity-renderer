import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'
import { VideoTracksActiveStreamsRequest, VideoTracksActiveStreamsData, CommsApiServiceDefinition, VideoTrackSourceType } from 'shared/protocol/decentraland/kernel/apis/comms_api.gen'
import { isWorldLoaderActive } from 'shared/realm/selectors'
import { store } from 'shared/store/isolatedStore'
import { getLivekitActiveVideoStreams } from 'shared/comms/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { ActiveVideoStreams } from 'shared/comms/adapters/types'
import { Track } from 'livekit-client'

export function registerCommsApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
    codegen.registerService(port, CommsApiServiceDefinition, async () => ({
        async getActiveVideoStreams(req: VideoTracksActiveStreamsRequest, ctx: PortContext) {

            const realmAdapter = await ensureRealmAdapter()
            const isWorld = isWorldLoaderActive(realmAdapter)
            if (!isWorld) {
                ctx.logger.error('API only available for Worlds')
                return { streams: [] }
            }

            const activeVideoStreams: Map<string, ActiveVideoStreams> | undefined = getLivekitActiveVideoStreams(store.getState())

            if (!activeVideoStreams)
                return { streams: [] }

            let streams: VideoTracksActiveStreamsData[] = []

            for (const [sid, videoStreamData] of activeVideoStreams) {
                if (videoStreamData.videoTracks.size > 0) {
                    for (const [videoSid, trackData] of videoStreamData.videoTracks) {
                        if (!!trackData.source) {
                            streams.push({
                                identity: videoStreamData.identity,
                                trackSid: `livekit-video://${sid}/${videoSid}`,
                                sourceType: trackData.source === Track.Source.Camera ? VideoTrackSourceType.VTST_CAMERA
                                    : trackData.source === Track.Source.ScreenShare ? VideoTrackSourceType.VTST_SCREEN_SHARE
                                        : VideoTrackSourceType.VTST_UNKNOWN
                            })
                        }
                    }
                }
            }

            return { streams }
        }
    })
    )
}