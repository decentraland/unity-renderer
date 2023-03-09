import { expect } from 'chai'
import { ensureAvatarCompatibilityFormat } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import {
  addProfileToLastSentProfileVersionAndCatalog, ADD_PROFILE_TO_LAST_SENT_VERSION_AND_CATALOG, profileRequest
} from 'shared/profiles/actions'
import { fetchProfile, getInformationToFetchProfileFromStore } from 'shared/profiles/sagas/fetchProfile'
import type { ProfileUserInfo } from 'shared/profiles/types'
import type { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { getInformationToSubmitProfileFromStore, handleSubmitProfileToRenderer } from 'shared/renderer/sagas'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'
import { buildStore } from 'shared/store/store'
import sinon from 'sinon'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { PROFILE_SUCCESS } from '../../packages/shared/profiles/actions'

const mockAdapter = {
  services: {
    legacy: {}
  }
} as any as IRealmAdapter

describe('fetchProfile behavior', () => {
  it('avatar compatibility format', () => {
    ensureAvatarCompatibilityFormat({
      userId: '0x736df2ecb40e4bdc368e19e3067136802536550c',
      email: '',
      name: 'Ant#550c',
      hasClaimedName: false,
      description: 'Host at The Aquarium casino         * -140,126*                                         ',
      ethAddress: '0x736df2ecb40e4bdc368e19e3067136802536550c',
      version: 2,
      avatar: {
        bodyShape: 'urn:decentraland:off-chain:base-avatars:BaseMale',
        snapshots: {
          face256:
            'https://peer.decentral.io/content/contents/bafkreidxhkfakmifeccr3ypojv53oqnecufx647tkqupwipuelru3tkevm',
          body: 'https://peer.decentral.io/content/contents/bafkreie2pwupprfvg64mopsvwnmgzxckxw4q3i4gquaedelwdf6ax2soea'
        },
        eyes: { color: { r: 0.75, g: 0.62109375, b: 0.3515625 } },
        hair: { color: { r: 0.234375, g: 0.12890625, b: 0.04296875 } },
        skin: { color: { r: 0.60546875, g: 0.4609375, b: 0.35546875 } },
        wearables: [
          'urn:decentraland:off-chain:base-avatars:casual_hair_03',
          'urn:decentraland:off-chain:base-avatars:eyebrows_00',
          'urn:decentraland:off-chain:base-avatars:eyes_08',
          'urn:decentraland:off-chain:base-avatars:mouth_03',
          'urn:decentraland:matic:collections-v2:0x1286dad1da5233a63a5d55fcf9e834feb14e1d6d:0',
          'urn:decentraland:off-chain:base-avatars:thug_life',
          'urn:decentraland:off-chain:base-avatars:Thunder_earring',
          'urn:decentraland:off-chain:base-avatars:brown_pants',
          'urn:decentraland:off-chain:base-avatars:sneakers',
          'urn:decentraland:matic:collections-v2:0x1286dad1da5233a63a5d55fcf9e834feb14e1d6d:1'
        ]
      },
      tutorialStep: 256,
      interests: [],
      unclaimedName: ''
    } as any)
  })

  it('detects and fixes corrupted scaled snapshots', () => {
    const userId = 'user|1'
    const action = profileRequest(userId)

    return expectSaga(fetchProfile, action)
      .provide([
        [select(getInformationToFetchProfileFromStore, action), {
          roomConnection: undefined,
          loadingCurrentUser: false,
          hasRoomConnection: false,
          existingProfile: undefined,
          isGuestLogin: false,
          existingProfileWithCorrectVersion: false
        }]
      ])
      .run()
      .then((result) => {
        const putEffects = result.effects.put
        const lastPut = putEffects[putEffects.length - 1].payload.action
        expect(lastPut.type).to.eq(PROFILE_SUCCESS)

        const { face256 } = lastPut.payload.profile.avatar.snapshots
        expect(typeof face256).to.eq(typeof 'String')
      })
  })
})

describe('Handle submit profile to renderer', () => {
  sinon.mock()

  const unityInstance = getUnityInstance()
  const unityMock = sinon.mock(unityInstance)

  beforeEach(() => {
    sinon.reset()
    sinon.restore()
    buildStore()
  })

  afterEach(() => {
    sinon.restore()
    sinon.reset()
  })

  context('When the profile has already been sent, and it doesnt have any updates', () => {
    it('Does not send the AddUserProfileToCatalog message.', async () => {
      const userId = '0x11pizarnik00'

      const profile = getMockedProfileUserInfo(userId, '')

      unityMock.expects('AddUserProfilesToCatalog').never()

      await expectSaga(handleSubmitProfileToRenderer, { type: 'Send Profile to Renderer Requested', payload: { userId } })
        .provide([
          [call(waitForRendererInstance), true],
          [call(waitForRealm), mockAdapter],
          [select(getInformationToSubmitProfileFromStore, userId), {
            profile,
            identity: { userId: 'invalid' },
            isCurrentUser: false,
            lastSentProfileVersion: 1
          }],
        ])
        .run()
        .then((response) => {
          const putEffects = response.effects.put
          expect(putEffects).to.be.undefined
        })

      unityMock.verify()
    })
  })

  context('When the profile has already been sent, the version is 0 and it doesnt have any updates', () => {
    it('Does not send the AddUserProfileToCatalog message.', async () => {
      const userId = '0x11pizarnik00'

      const profile = getMockedProfileUserInfo(userId, '', 0)

      unityMock.expects('AddUserProfilesToCatalog').never()

      await expectSaga(handleSubmitProfileToRenderer, { type: 'Send Profile to Renderer Requested', payload: { userId } })
        .provide([
          [call(waitForRendererInstance), true],
          [call(waitForRealm), mockAdapter],
          [select(getInformationToSubmitProfileFromStore, userId), {
            profile,
            identity: { userId: 'invalid' },
            isCurrentUser: false,
            lastSentProfileVersion: 1
          }],
        ])
        .run()
        .then((response) => {
          const putEffects = response.effects.put
          expect(putEffects).to.be.undefined
        })

      unityMock.verify()
    })
  })

  context('When the profile has already been sent, and it has updates', () => {
    it('Sends the AddUserProfileToCatalog message.', async () => {
      const userId = '0x11pizarnik00'

      const profile = getMockedProfileUserInfo(userId, '', 3)

      unityMock.expects('AddUserProfileToCatalog').once()

      await expectSaga(handleSubmitProfileToRenderer, { type: 'Send Profile to Renderer Requested', payload: { userId } })
        .provide([
          [call(waitForRendererInstance), true],
          [call(waitForRealm), mockAdapter],
          [select(getInformationToSubmitProfileFromStore, userId), {
            profile,
            identity: { userId: 'invalid' },
            isCurrentUser: false,
            lastSentProfileVersion: 1
          }],
        ])
        .dispatch(addProfileToLastSentProfileVersionAndCatalog(userId, 3))
        .run()
        .then((response) => {
          const putEffects = response.effects.put
          const lastPut = putEffects[putEffects.length - 1].payload.action
          expect(lastPut.type).to.eq(ADD_PROFILE_TO_LAST_SENT_VERSION_AND_CATALOG)
        })

      unityMock.verify()
    })
  })
})

function getMockedProfileUserInfo(userId: string, name: string, version: number = 1): ProfileUserInfo {
  return {
    data: {
      avatar: {
        snapshots: {
          face256: '',
          body: ''
        },
        eyes: { color: '' },
        hair: { color: '' },
        skin: { color: '' }
      } as any,
      description: '',
      ethAddress: userId,
      hasClaimedName: false,
      name,
      tutorialStep: 1,
      userId,
      version
    },
    status: 'ok',
    addedToCatalog: false
  }
}
