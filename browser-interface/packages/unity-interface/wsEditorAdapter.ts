import type { UnityGame } from 'unity-interface/loader'
import { CommonRendererOptions } from './loader'
import { webSocketTransportAdapter } from 'renderer-protocol/transports/webSocketTransportAdapter'
import { Transport } from '@dcl/rpc'

/** This connects the local game to a native client via WebSocket */
export async function initializeUnityEditor(
  wsUrl: string,
  container: HTMLElement,
  options: CommonRendererOptions
): Promise<{ renderer: UnityGame; transport: Transport }> {
  container.innerHTML = `<h3>Connecting...</h3>`

  const transport = webSocketTransportAdapter(wsUrl, options)

  transport.on('connect', () => {
    container.classList.remove('dcl-loading')
    container.innerHTML = `<h3 style='color:green'>Connected</h3>`
  })

  transport.on('close', () => {
    container.innerHTML = `<h3 style='color:red'>Disconnected</h3>`
  })

  const renderer: UnityGame = {
    Module: {},
    SendMessage(_obj, type, payload) {
      transport.sendMessage({ type, payload } as any)
    },
    SetFullscreen() {
      // stub
    },
    async Quit() {
      // stub
    }
  }

  return { renderer, transport }
}
