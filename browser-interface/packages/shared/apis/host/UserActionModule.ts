import { UserActionModuleServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/user_action_module.gen'
import type { RpcServerPort } from '@dcl/rpc'
import * as codegen from '@dcl/rpc/dist/codegen'
import type { Scene } from '@dcl/schemas'
import { getOwnerNameFromJsonData } from 'lib/decentraland/sceneJson/getOwnerNameFromJsonData'
import { getSceneNameFromJsonData } from 'lib/decentraland/sceneJson/getSceneNameFromJsonData'
import { getThumbnailUrlFromJsonDataAndContent } from 'lib/decentraland/sceneJson/getThumbnailUrlFromJsonDataAndContent'
import { postProcessSceneName } from 'shared/atlas/selectors'
import { fetchScenesByLocation } from 'shared/scene-loader/sagas'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import type { PortContext } from './context'

export function registerUserActionModuleServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, UserActionModuleServiceDefinition, async () => ({
    async requestTeleport(req, ctx) {
      const { destination } = req
      if (destination === 'magic' || destination === 'crowd') {
        getUnityInstance().RequestTeleport({ destination })
        return {}
      } else if (!/^\-?\d+\,\-?\d+$/.test(destination)) {
        ctx.logger.error(`teleportTo: invalid destination ${destination}`)
        return {}
      }

      let sceneEvent = {}
      const sceneData = {
        name: 'Unnamed',
        owner: 'Unknown',
        previewImageUrl: ''
      }

      const mapSceneData = (await fetchScenesByLocation([destination]))[0]

      if (mapSceneData) {
        const metadata: Scene | undefined = mapSceneData?.entity.metadata

        sceneData.name = postProcessSceneName(getSceneNameFromJsonData(metadata))
        sceneData.owner = getOwnerNameFromJsonData(metadata)
        sceneData.previewImageUrl =
          getThumbnailUrlFromJsonDataAndContent(
            mapSceneData.entity.metadata,
            mapSceneData.entity.content,
            mapSceneData.baseUrl
          ) || sceneData.previewImageUrl
      } else {
        debugger
      }

      try {
        const response = await fetch(`https://events.decentraland.org/api/events/?position=${destination}`)
        const json = await response.json()
        if (json.data.length > 0) {
          sceneEvent = {
            name: json.data[0].name,
            total_attendees: json.data[0].total_attendees,
            start_at: json.data[0].start_at,
            finish_at: json.data[0].finish_at
          }
        }
      } catch (e: any) {
        ctx.logger.error(e)
      }

      getUnityInstance().RequestTeleport({
        destination,
        sceneEvent,
        sceneData
      })
      return {}
    }
  }))
}
