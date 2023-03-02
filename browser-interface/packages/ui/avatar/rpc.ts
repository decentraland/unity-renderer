import { executeTask } from '@dcl/legacy-ecs'

declare let dcl: DecentralandInterface

export async function execute(controller: string, method: string, args: Array<any>) {
  return executeTask(async () => {
    return dcl.callRpc(controller, method, args)
  })
}
