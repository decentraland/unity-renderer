import { expect } from 'chai'
import { ETHEREUM_NETWORK } from 'config'
import { testSaga } from 'redux-saga-test-plan'
import { deployProfile, profileSuccess, saveProfileDelta } from 'shared/profiles/actions'
import { getInformationForSaveAvatar, handleSaveLocalAvatar } from 'shared/profiles/sagas/handleSaveLocalAvatar'
import { fakeValidAvatar } from './fakeValidAvatar'
import { localProfilesRepo } from 'shared/profiles/sagas/local/localProfilesRepo'
import Sinon from 'sinon'

const userId = '0xe804a05092D13Bb1F626F2472b6539C88EDC5976'
const identity = { address: userId, hasConnectedWeb3: true }
const savedProfile = {
  userId,
  ethAddress: userId,
  hasClaimedName: false,
  hasConnectedWeb3: true,
  version: 1,
  description: '',
  tutorialStep: 0,
  name: 'Guest',
  avatar: fakeValidAvatar
}

describe('handleSaveLocalAvatar', () => {
  it('should save local avatar successfully', () => {
    const saveAvatar = saveProfileDelta({
      description: 'Test description',
      tutorialStep: 5
    })

    const updatedProfile = {
      ...savedProfile,
      ...saveAvatar.payload.profile,
      version: 2
    }
    const local = Sinon.mock(localProfilesRepo)
    testSaga(handleSaveLocalAvatar, saveAvatar)
      .next()
      .select(getInformationForSaveAvatar)
      .next({
        userId,
        savedProfile,
        identity,
        network: ETHEREUM_NETWORK.MAINNET
      })
      .next()
      .put(profileSuccess(updatedProfile))
      .next()
      .put(deployProfile(updatedProfile))
      .next()
      .isDone()
    expect(local.expects('persist').calledOnce)
    local.restore()
  })

  it('should not re-save if avatar did not change', () => {

    const saveAvatar = saveProfileDelta({
      description: savedProfile.description,
      tutorialStep: savedProfile.tutorialStep,
    })

    const local = Sinon.mock(localProfilesRepo)
    testSaga(handleSaveLocalAvatar, saveAvatar)
      .next()
      .select(getInformationForSaveAvatar)
      .next({
        userId,
        savedProfile,
        identity,
        network: ETHEREUM_NETWORK.MAINNET
      })
      .next()
      .isDone()
    expect(local.expects('persist').notCalled)
    local.restore()
  })
})
