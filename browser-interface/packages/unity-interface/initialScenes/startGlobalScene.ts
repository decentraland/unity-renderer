import { ContentMapping, EntityType, Scene } from '@dcl/schemas'
import { LoadableScene } from 'shared/types'
import { loadParcelSceneWorker } from 'shared/world/parcelSceneManager'

type GlobalSceneOptions = {
  ecs7?: boolean
  content?: ContentMapping[]
  baseUrl?: string
}

export async function startGlobalScene(
  cid: string,
  title: string,
  fileContentUrl: string,
  options: GlobalSceneOptions = {}
) {
  const metadataScene: Scene = {
    display: {
      title: title
    },
    main: 'scene.js',
    scene: {
      base: '0,0',
      parcels: ['0,0']
    }
  }

  const baseUrl = options.baseUrl || location.origin
  const extraContent = options.content || []
  const metadata: LoadableScene['entity']['metadata'] = { ...metadataScene }

  if (!!options.ecs7) {
    metadata.ecs7 = true
  }

  return await loadParcelSceneWorker({
    id: cid,
    baseUrl,
    entity: {
      content: [...extraContent, { file: 'scene.js', hash: fileContentUrl }],
      pointers: [cid],
      timestamp: 0,
      type: EntityType.SCENE,
      metadata,
      version: 'v3'
    },
    isGlobalScene: true,
    // global scenes have no FPS limit
    useFPSThrottling: false
  })
}
