import { PermissionItem, permissionItemToJSON } from 'shared/protocol/decentraland/kernel/apis/permissions.gen'
import { EntityType, Scene } from '@dcl/schemas'
import { expect } from 'chai'
import { PortContext } from 'shared/apis/host/context'
import { triggerEmote } from 'shared/apis/host/RestrictedActions'
import { buildStore } from 'shared/store/store'
import Sinon, * as sinon from 'sinon'
import { UnityInterface } from 'unity-interface/UnityInterface'
import defaultLogger from 'lib/logger'
import { lastPlayerPosition } from 'shared/world/positionThings'
import { getUnityInstance, setUnityInstance } from 'unity-interface/IUnityInterface'

describe('RestrictedActions tests', () => {
  beforeEach(() => {
    sinon.reset()
    sinon.restore()
    setUnityInstance({ Teleport: () => { }, TriggerSelfUserExpression: () => { } } as any)
    buildStore()
  })

  after(() => {
    setUnityInstance(new UnityInterface())
  })

  describe('TriggerEmote tests', () => {
    const emote = 'emote'

    it('should trigger emote', async () => {
      setLastPlayerPosition()
      const ctx = getContextWithPermissions(PermissionItem.PI_ALLOW_TO_TRIGGER_AVATAR_EMOTE)
      const stub = sinon.stub(getUnityInstance(), 'TriggerSelfUserExpression')

      triggerEmote({ predefinedEmote: emote }, ctx)
      Sinon.assert.calledWithExactly(stub, emote)
    })

    it('should fail when scene does not have permissions', async () => {
      setLastPlayerPosition()
      const ctx = getContextWithPermissions()
      const stub = sinon.stub(getUnityInstance(), 'TriggerSelfUserExpression')

      expect(() => triggerEmote({ predefinedEmote: 'emote' }, ctx)).to.throw(
        /This scene doesn't have some of the next permissions: PI_ALLOW_TO_TRIGGER_AVATAR_EMOTE/
      )

      Sinon.assert.callCount(stub, 0)
    })

    it('should fail when player is out of scene and try to move', async () => {
      setLastPlayerPosition(false)
      const ctx = getContextWithPermissions(PermissionItem.PI_ALLOW_TO_TRIGGER_AVATAR_EMOTE)

      const stub = sinon.stub(getUnityInstance(), 'TriggerSelfUserExpression')
      const errorSpy = sinon.spy(defaultLogger, 'error')

      triggerEmote({ predefinedEmote: 'emote' }, ctx)

      Sinon.assert.calledWithExactly(errorSpy, 'Error: Player is not inside of scene', lastPlayerPosition)
      Sinon.assert.callCount(stub, 0)
    })
  })

  const setLastPlayerPosition = (inside: boolean = true) => {
    const position = inside
      ? { x: 7.554769515991211, y: 1.7549998760223389, z: 1622.2711181640625 } // in
      : { x: -1.0775706768035889, y: 1.774094820022583, z: 1621.8487548828125 } // out
    lastPlayerPosition.x = position.x
    lastPlayerPosition.y = position.y
    lastPlayerPosition.z = position.z
  }

  function getContextWithPermissions(...permissions: PermissionItem[]): PortContext {
    const sceneData = buildSceneData(permissions)
    return {
      sdk7: false,
      sceneData,
      logger: defaultLogger,
      rpcSceneControllerService: null as any,
      scenePort: null as any,
      permissionGranted: new Set(permissions),
      subscribedEvents: new Set(),
      events: [],
      sendProtoSceneEvent() {
        throw new Error('not implemented')
      },
      sendSceneEvent() {
        throw new Error('not implemented')
      },
      sendBatch() {
        throw new Error('not implemented')
      },
      hasMainCrdt: false,
      initialEntitiesTick0: Uint8Array.of(),
      readFile(_path) {
        throw new Error('not implemented')
      },
    }
  }

  function buildSceneData(permissions: PermissionItem[] = []): PortContext['sceneData'] {
    const metadata: Scene = {
      display: { title: 'interactive-text', favicon: 'favicon_asset' },
      contact: { name: 'Ezequiel', email: 'ezequiel@decentraland.org' },
      owner: 'decentraland',
      scene: { parcels: ['0,101'], base: '0,101' },
      main: 'game.js',
      tags: [],
      requiredPermissions: permissions.map((item) => {
        const ret = permissionItemToJSON(item).replace('PI_', '')
        expect(ret).to.not.eq('UNRECOGNIZED')
        return ret
      }),
      spawnPoints: [
        { name: 'spawn1', default: true, position: { x: 0, y: 0, z: 0 }, cameraTarget: { x: 8, y: 1, z: 8 } }
      ]
    }

    return {
      id: 'test',
      isPortableExperience: false,
      useFPSThrottling: false,
      sceneNumber: 3,
      baseUrl: '',
      entity: {
        version: 'v3',
        content: [],
        pointers: [],
        timestamp: 0,
        type: EntityType.SCENE,
        metadata
      }
    }
  }
})
