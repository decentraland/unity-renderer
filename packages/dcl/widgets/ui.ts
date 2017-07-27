import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { WebGLScene } from 'dcl/WebGLScene'
import { SceneWorker } from 'shared/world/SceneWorker'
import { ChatController } from 'dcl/api/ChatController'
import { SocialController } from 'dcl/api/SocialController'

const chatWorkerRaw = require('raw-loader!../../../static/systems/chat.scene.js')
const chatWorkerBLOB = new Blob([chatWorkerRaw])
const chatWorkerUrl = URL.createObjectURL(chatWorkerBLOB)

const hudWorkerRaw = require('raw-loader!../../../static/systems/avatarProfile.scene.js')
const hudWorkerBLOB = new Blob([hudWorkerRaw])
const hudWorkerUrl = URL.createObjectURL(hudWorkerBLOB)

export class WebGLUIScene extends WebGLScene<any> {
  constructor(id: string, main: string, context: SharedSceneContext) {
    super(
      {
        baseUrl: context.baseUrl,
        main,
        data: {},
        id,
        mappings: {}
      },
      context
    )

    this.context.rootEntity.name = this.context.rootEntity.id = id
    // tslint:disable-next-line:no-unused-expression
    new SceneWorker(this)
  }
}

export async function initHudSystem(): Promise<WebGLUIScene> {
  const context = new SharedSceneContext('/', 'ui-context-hud', false)
  context.isInternal = true
  const scene = new WebGLUIScene('hud', hudWorkerUrl, context)
  const system = await scene.worker.system

  // Create instances of non-public-controllers
  system.getAPIInstance(ChatController)
  system.getAPIInstance(SocialController)

  return scene
}

export async function initChatSystem(): Promise<WebGLUIScene> {
  const context = new SharedSceneContext('/', 'ui-context-chat', false)
  context.isInternal = true
  const scene = new WebGLUIScene('chat', chatWorkerUrl, context)
  const system = await scene.worker.system

  // Create instance of non-public-controllers
  system.getAPIInstance(ChatController)

  return scene
}
