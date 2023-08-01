import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'
import { VideoTracksActiveStreamsRequest, VideoTracksActiveStreamsData, CommsApiServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/comms_api.gen'
import { isWorldLoaderActive } from 'shared/realm/selectors'
import { store } from 'shared/store/isolatedStore'
import { getLivekitActiveVideoStreams } from 'shared/comms/selectors'
import { ensureRealmAdapter } from 'shared/realm/ensureRealmAdapter'
import { ActiveVideoStreams } from 'shared/comms/adapters/types'

export function registerCommsApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
    codegen.registerService(port, CommsApiServiceDefinition, async () => ({
        async getActiveVideoStreams(req: VideoTracksActiveStreamsRequest, ctx: PortContext) {

            const realmAdapter = await ensureRealmAdapter()
            const isWorld = isWorldLoaderActive(realmAdapter)
            if (!isWorld){
                ctx.logger.error('API only available for Worlds')
                return { streams: [] }
            }

            // TODO: scene permissions?
            const activeVideoStreams: Map<string, ActiveVideoStreams> | undefined = getLivekitActiveVideoStreams(store.getState())

            if (!activeVideoStreams)
                return { streams: [] }

            let streams: VideoTracksActiveStreamsData[] = []

            for (let [sid, videoStreamData] of activeVideoStreams) {
                if (videoStreamData.videoTracks.size > 0) {
                    for (let [videoSid, _] of videoStreamData.videoTracks) {
                        streams.push({
                            identity: videoStreamData.identity,
                            trackSid: `livekit-video://${sid}/${videoSid}`
                        })
                    }
                }
            }

            return { streams }
        }
    })
    )
}