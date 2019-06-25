import { SceneWorker, hudWorkerUrl } from 'shared/world/SceneWorker'
import { ensureUiApis } from 'shared/world/uiSceneInitializer'

import { SharedSceneContext } from '../../entities/SharedSceneContext'
import { WebGLScene } from '../WebGLScene'

export class WebGLUIScene extends WebGLScene<any> {
  constructor(sceneId: string, main: string, context: SharedSceneContext) {
    super(
      {
        baseUrl: context.baseUrl,
        main,
        data: {},
        sceneId,
        mappings: []
      },
      context
    )

    this.context.rootEntity.name = this.context.rootEntity.id = sceneId
    // tslint:disable-next-line:no-unused-expression
    new SceneWorker(this)
  }
}

export let hud: WebGLUIScene | null = null

export async function initHudSystem(): Promise<WebGLUIScene> {
  if (!hud) {
    const context = new SharedSceneContext('/', 'ui-context-hud', false)

    context.useMappings = false
    context.baseUrl = document.location.origin
    hud = new WebGLUIScene('hud', hudWorkerUrl, context)

    await ensureUiApis(hud.worker!)
  }
  return hud
}
