import { EntityAction, EnvironmentData } from 'shared/types'

export type ParcelSceneAPI = {
  data: EnvironmentData<any>
  sendBatch(ctions: EntityAction[]): void
  registerWorker(event: any): void
  dispose(): void
  on(event: string, cb: (event: any) => void): void
}
