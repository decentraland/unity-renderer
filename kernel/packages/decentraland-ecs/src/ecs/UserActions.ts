declare var dcl: any

let modulePromise: any

/**
 * teleport player to a destination
 * @param destination "coordX,coordY", "magic", "crowd"
 */
export function teleportTo(destination: string) {
  callModuleRpc('requestTeleport', [destination])
}

function ensureModule(): boolean {
  if (typeof modulePromise === 'undefined' && typeof dcl !== 'undefined') {
    modulePromise = dcl.loadModule('@decentraland/UserActionModule')
  }
  return typeof modulePromise !== 'undefined' && typeof dcl !== 'undefined'
}

function callModuleRpc(methodName: string, args: any[]): void {
  if (ensureModule()) {
    modulePromise.then(($: any) => {
      dcl.callRpc($.rpcHandle, methodName, args)
    })
  }
}
