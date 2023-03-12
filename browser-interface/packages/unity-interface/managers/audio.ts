import defaultLogger from 'lib/logger'
import { SetAudioDevicesPayload } from 'shared/types'
import { requestMediaDevice } from 'shared/voiceChat/sagas'
import { getUnityInstance } from 'unity-interface/IUnityInterface'

export async function handleRequestAudioDevices() {
  if (!navigator.mediaDevices?.enumerateDevices) {
    defaultLogger.error('enumerateDevices() not supported.')
  } else {
    try {
      await requestMediaDevice()

      // List cameras and microphones.
      const devices = await navigator.mediaDevices.enumerateDevices()

      const filterDevices = (kind: string) => {
        return devices
          .filter((device) => device.kind === kind)
          .map((device) => {
            return { deviceId: device.deviceId, label: device.label }
          })
      }

      const payload: SetAudioDevicesPayload = {
        inputDevices: filterDevices('audioinput'),
        outputDevices: filterDevices('audiooutput')
      }

      getUnityInstance().SetAudioDevices(payload)
    } catch (err: any) {
      defaultLogger.error(`${err.name}: ${err.message}`)
    }
  }
}
