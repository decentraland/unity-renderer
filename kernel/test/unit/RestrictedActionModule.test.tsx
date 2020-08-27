import * as sinon from 'sinon'
import { Vector3 } from 'decentraland-ecs/src'
import { unityInterface } from '../../packages/unity-interface/UnityInterface'
import defaultLogger from '../../packages/shared/logger'
import { Permission, RestrictedActionModule } from '../../packages/shared/apis/RestrictedActionModule'
import { lastPlayerPosition } from '../../packages/shared/world/positionThings'

describe('RestrictedActionModule tests', () => {
  describe('MovePlayerTo tests', () => {
    afterEach(() => sinon.restore())

    const options = {
      apiName: '',
      system: null,
      expose: sinon.stub(),
      notify: sinon.stub(),
      on: sinon.stub(),
      getAPIInstance(name): any {}
    }

    const mockLastPlayerPosition = (inside: boolean = true) => {
      const position = inside
        ? { x: 7.554769515991211, y: 1.7549998760223389, z: 1622.2711181640625 } // in
        : { x: -1.0775706768035889, y: 1.774094820022583, z: 1621.8487548828125 } // out
      sinon.stub(lastPlayerPosition, 'x').value(position.x)
      sinon.stub(lastPlayerPosition, 'y').value(position.y)
      sinon.stub(lastPlayerPosition, 'z').value(position.z)
    }

    const buildParcelIdentity = (permissions: Permission[] = []) => {
      return {
        land: {
          sceneJsonData: {
            display: { title: 'interactive-text', favicon: 'favicon_asset' },
            contact: { name: 'Ezequiel', email: 'ezequiel@decentraland.org' },
            owner: 'decentraland',
            scene: { parcels: ['0,101'], base: '0,101' },
            communications: { type: 'webrtc', signalling: 'https://signalling-01.decentraland.org' },
            policy: { contentRating: 'E', fly: true, voiceEnabled: true, blacklist: [] },
            main: 'game.js',
            tags: [],
            requiredPermissions: permissions,
            spawnPoints: [
              { name: 'spawn1', default: true, position: { x: 0, y: 0, z: 0 }, cameraTarget: { x: 8, y: 1, z: 8 } }
            ]
          }
        }
      }
    }

    it('should move the player', async () => {
      mockLastPlayerPosition()
      sinon
        .mock(options)
        .expects('getAPIInstance')
        .withArgs()
        .once()
        .returns(buildParcelIdentity([Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE]))

      sinon
        .mock(unityInterface)
        .expects('Teleport')
        .once()
        .withExactArgs({ position: { x: 8, y: 0, z: 1624 }, cameraTarget: undefined })

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(8, 0, 8))
      sinon.verify()
    })

    it('should fail when position is outside scene', async () => {
      mockLastPlayerPosition()
      sinon
        .mock(options)
        .expects('getAPIInstance')
        .withArgs()
        .once()
        .returns(buildParcelIdentity([Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE]))
      sinon
        .mock(defaultLogger)
        .expects('error')
        .once()
        .withExactArgs('Error: Position is out of scene', { x: 21, y: 0, z: 1648 })

      sinon.mock(unityInterface).expects('Teleport').never()

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(21, 0, 32))
      sinon.verify()
    })

    it('should fail when scene does not have permissions', async () => {
      mockLastPlayerPosition()
      sinon.mock(options).expects('getAPIInstance').withArgs().once().returns(buildParcelIdentity([]))
      sinon.mock(unityInterface).expects('Teleport').never()
      sinon
        .mock(defaultLogger)
        .expects('error')
        .once()
        .withExactArgs('Permission "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE" is required')

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(8, 0, 8))
      sinon.verify()
    })

    it('should fail when player is out of scene and try to move', async () => {
      mockLastPlayerPosition(false)
      sinon
        .mock(options)
        .expects('getAPIInstance')
        .withArgs()
        .once()
        .returns(buildParcelIdentity([Permission.ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE]))

      sinon.mock(unityInterface).expects('Teleport').never()

      sinon
        .mock(defaultLogger)
        .expects('error')
        .once()
        .withExactArgs('Error: Player is not inside of scene', lastPlayerPosition)

      const module = new RestrictedActionModule(options)

      await module.movePlayerTo(new Vector3(8, 0, 8))
      sinon.verify()
    })
  })
})
