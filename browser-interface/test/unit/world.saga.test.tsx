import { expect } from 'chai'
import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import { signalSceneStart } from 'shared/loading/actions'
import { getParcelPositionAsString } from 'shared/scene-loader/selectors'
import { getCurrentUserId } from 'shared/session/selectors'
import { LoadableScene } from 'shared/types'
import { rendererSignalSceneReady } from 'shared/world/actions'
import {
  getLoadedParcelSceneByParcel,
  getSceneWorkerBySceneID,
  getSceneWorkerBySceneNumber
} from 'shared/world/parcelSceneManager'
import { anounceOnEnterOnSceneStart, anounceOnReadyOnSceneReady } from 'shared/world/sagas'
import { SceneWorker } from 'shared/world/SceneWorker'

describe('World', () => {
  it('anounceOnReadyOnSceneReady -> sceneNumber', async () => {
    const action = rendererSignalSceneReady('abc', 123)
    let called = false
    await expectSaga(anounceOnReadyOnSceneReady)
      .provide([
        [
          call(getSceneWorkerBySceneNumber, 123),
          {
            onReady() {
              called = true
            }
          }
        ]
      ])
      .dispatch(action)
      .silentRun(0)
    expect({ called }).to.deep.eq({ called: true })
  })

  it('anounceOnReadyOnSceneReady -> sceneId', async () => {
    const action = rendererSignalSceneReady('abc')
    let called = false
    await expectSaga(anounceOnReadyOnSceneReady)
      .provide([
        [
          call(getSceneWorkerBySceneID, 'abc'),
          {
            onReady() {
              called = true
            }
          }
        ]
      ])
      .dispatch(action)
      .silentRun(0)
    expect({ called }).to.deep.eq({ called: true })
  })

  it('anounceOnEnterOnSceneStart -> parcelScene', async () => {
    const SCENE_ID = 'SCENE_ID'
    const SCENE_POSITION = '10,-10'
    const action = signalSceneStart({ id: SCENE_ID } as LoadableScene)

    let userIdFromCall: any = null

    const SCENE_WORKER = {
      loadableScene: {
        id: SCENE_ID
      },
      onEnter(userId) {
        userIdFromCall = userId
      }
    } as any as SceneWorker

    await expectSaga(anounceOnEnterOnSceneStart)
      .provide([
        [select(getParcelPositionAsString), SCENE_POSITION],
        [call(getLoadedParcelSceneByParcel, SCENE_POSITION), SCENE_WORKER],
        [select(getCurrentUserId), 'userid1']
      ])
      .dispatch(action)
      .silentRun(0)

    expect({ userIdFromCall }).to.deep.eq({ userIdFromCall: 'userid1' })
  })

  it('anounceOnEnterOnSceneStart -> portableExperience', async () => {
    const SCENE_ID = 'SCENE_ID'
    const SCENE_POSITION = '0,0'
    const action = signalSceneStart({ id: SCENE_ID } as LoadableScene)

    let userIdFromCall: any = null

    const SCENE_WORKER = {
      loadableScene: {
        id: 'some-random-id'
      },
      rpcContext: { sceneData: { isPortableExperience: true } },
      onEnter(userId) {
        userIdFromCall = userId
      }
    } as any as SceneWorker

    await expectSaga(anounceOnEnterOnSceneStart)
      .provide([
        [select(getParcelPositionAsString), SCENE_POSITION],
        [call(getLoadedParcelSceneByParcel, SCENE_POSITION), SCENE_WORKER],
        [select(getCurrentUserId), 'userid1']
      ])
      .dispatch(action)
      .silentRun(0)

    expect({ userIdFromCall }).to.deep.eq({ userIdFromCall: 'userid1' })
  })
})
