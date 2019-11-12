import { executeTask } from 'decentraland-ecs/src'

declare var dcl: any

export async function execute(controller: string, method: string, args: Array<any>) {
  return executeTask(async () => {
    return dcl.callRpc(controller, method, args)
  })
}
