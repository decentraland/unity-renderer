declare var dcl: any

let modulePromise: any

type PositionType = { x: number; y: number; z: number }

/**
 * move player to a position inside the scene
 *
 * @param position PositionType
 * @param cameraTarget PositionType
 */
export function movePlayerTo(position: PositionType, cameraTarget?: PositionType) {
  callModuleRpc('movePlayerTo', [position, cameraTarget])
}

function ensureModule(): boolean {
  if (typeof modulePromise === 'undefined' && typeof dcl !== 'undefined') {
    modulePromise = dcl.loadModule('@decentraland/RestrictedActionModule')
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
