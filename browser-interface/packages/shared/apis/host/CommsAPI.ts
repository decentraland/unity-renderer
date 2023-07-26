import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { PortContext } from './context'
import { VideoTracksActiveStreamsRequest, VideoTracksActiveStreamsData, CommsApiServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/comms_api.gen'
import { RemoteParticipant } from 'livekit-client'
import { getRealmAdapter } from 'shared/realm/selectors'
import { store } from 'shared/store/isolatedStore'
import { dclWorldUrl, isDclEns } from 'shared/realm/resolver'

export function registerCommsApiServiceServerImplementation(port: RpcServerPort<PortContext>) {
    codegen.registerService(port, CommsApiServiceDefinition, async () => ({
        async getActiveVideoStreams(req: VideoTracksActiveStreamsRequest, ctx: PortContext) {

            const realmAdapter = getRealmAdapter(store.getState())
            const realmName = realmAdapter?.about.configurations?.realmName
            const isWorld = isDclEns(realmName) && realmAdapter?.baseUrl === dclWorldUrl(realmName)
            if (!isWorld){
                ctx.logger.error('API only available for Worlds')
                return { streams: [] }
            }

            // TODO: scene permissions

            const participants: Map<string, RemoteParticipant> | undefined = (window as any).livekit_participants

            if (!participants)
                return { streams: [] }

            let streams: VideoTracksActiveStreamsData[] = []

            for (let [sid, participant] of participants) {
                if (participant.videoTracks.size > 0) {
                    for (let [videoSid, _] of participant.videoTracks) {
                        streams.push({
                            identity: participant.identity,
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