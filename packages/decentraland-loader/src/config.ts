import { ParcelScene } from './parcelScene'

export type InitializationOptions = {
  content: string
  rpcUrl: string
  landApi: string
  contractAddress: string
  radius: number
  contentServer: string
}

export let sendParcelScenes: (parcelScenes: ParcelScene[]) => Promise<any> = () =>
  Promise.reject(new Error('sendParcelScenes not set'))

export let options: InitializationOptions | null = null

export function configure(
  _options: InitializationOptions,
  _sendParcelScenes: (parcelScenes: ParcelScene[]) => Promise<any>
) {
  options = _options
  sendParcelScenes = _sendParcelScenes
}
