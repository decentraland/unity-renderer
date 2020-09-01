import * as sinon from 'sinon'
import * as peers from '../../packages/shared/comms/peers'
import { browserInterface } from '../../packages/unity-interface/BrowserInterface'
import { StoreContainer } from '../../packages/shared/store/rootTypes'
import * as actions from '../../packages/shared/profiles/actions'
import { buildStore } from '../../packages/shared/store/store'
import { Profile } from '../../packages/shared/types'

declare const globalThis: StoreContainer

describe('BrowserInterface tests', () => {
  describe('User profile tests', () => {
    afterEach(() => sinon.restore())

    it('should save user interests', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      const interests = ['sport', 'music', 'sport', 'music']
      const profile: Partial<Profile> = {
        userId: '123',
        description: '',
        hasClaimedName: false,
        interests: []
      }
      const action = {
        meta: undefined,
        error: undefined,
        type: '[Request] Save Profile',
        payload: { profile: { interests: ['sport', 'music'] }, userId: '123' }
      }

      sinon.mock(peers).expects('getUserProfile').once().returns({ profile })
      sinon
        .mock(actions)
        .expects('saveProfileRequest')
        .once()
        .withExactArgs({ ...profile, interests: ['sport', 'music'] })
        .returns(action)
      sinon.mock(globalThis.globalStore).expects('dispatch').once().withExactArgs(action)

      browserInterface.SaveUserInterests(interests)
    })

    it('should update user interests', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      const interests = ['running', 'travel', 'shopping', 'shopping']
      const profile: Partial<Profile> = {
        userId: '123',
        description: '',
        hasClaimedName: false,
        interests: ['sport', 'music']
      }
      const action = {
        meta: undefined,
        error: undefined,
        type: '[Request] Save Profile',
        payload: { profile: { interests: ['running', 'travel', 'shopping'] }, userId: '123' }
      }

      sinon.mock(peers).expects('getUserProfile').once().returns({ profile })
      sinon
        .mock(actions)
        .expects('saveProfileRequest')
        .once()
        .withExactArgs({ ...profile, interests: ['running', 'travel', 'shopping'] })
        .returns(action)
      sinon.mock(globalThis.globalStore).expects('dispatch').once().withExactArgs(action)

      browserInterface.SaveUserInterests(interests)
    })

    it('should clean user interests', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      const interests: string[] = []
      const profile: Partial<Profile> = {
        userId: '123',
        description: '',
        hasClaimedName: false,
        interests: ['running', 'travel', 'shopping']
      }
      const action = {
        meta: undefined,
        error: undefined,
        type: '[Request] Save Profile',
        payload: { profile: { interests: [] }, userId: '123' }
      }

      sinon.mock(peers).expects('getUserProfile').once().returns({ profile })
      sinon
        .mock(actions)
        .expects('saveProfileRequest')
        .once()
        .withExactArgs({ ...profile, interests: [] })
        .returns(action)
      sinon.mock(globalThis.globalStore).expects('dispatch').once().withExactArgs(action)

      browserInterface.SaveUserInterests(interests)
    })
  })
})
