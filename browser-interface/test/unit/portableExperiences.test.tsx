import { expectSaga } from 'redux-saga-test-plan'
import { delay, call, select } from 'redux-saga/effects'
import { portableExperienceSaga } from 'shared/portableExperiences/sagas'
import {
  addScenePortableExperience,
  denyPortableExperiences,
  updateEnginePortableExperiences,
  removeScenePortableExperience,
  reloadScenePortableExperience,
  killAllPortableExperiences,
  activateAllPortableExperiences
} from 'shared/portableExperiences/actions'
import { LoadableScene } from 'shared/types'
import { getDesiredPortableExperiences } from 'shared/portableExperiences/selectors'
import { RootPortableExperiencesState } from 'shared/portableExperiences/types'
import { reducers } from 'shared/store/rootReducer'
import { expect } from 'chai'
import { EntityType } from '@dcl/schemas'
import { PORTABLE_EXPERIENCES_DEBOUNCE_DELAY } from 'config'
import { declareWantedPortableExperiences } from 'unity-interface/portableExperiencesUtils'

describe('Portable experiences sagas test', () => {
  const createLoadablePX = (urn: string): LoadableScene => ({
    parentCid: 'main',
    id: urn,
    baseUrl: '',
    entity: {
      content: [],
      metadata: {
        menuBarIcon: 'icon'
      },
      pointers: [],
      timestamp: 0,
      type: EntityType.SCENE,
      version: 'v3'
    }
  })

  it('empty scenario', () => {
    const action = addScenePortableExperience(createLoadablePX('urn'))

    return expectSaga(portableExperienceSaga)
      .provide([
        [select(getDesiredPortableExperiences), []],
        [call(declareWantedPortableExperiences, []), []]
      ])
      .dispatch(action)
      .put(updateEnginePortableExperiences([]))
      .silentRun(0)
  })

  it('returning one PX in debug', () => {
    const px = createLoadablePX('urn')
    const action = addScenePortableExperience(px)

    return expectSaga(portableExperienceSaga)
      .provide([
        [select(getDesiredPortableExperiences), [px]],
        [call(declareWantedPortableExperiences, [px]), []]
      ])
      .dispatch(action)
      .put(updateEnginePortableExperiences([px]))
      .silentRun(0)
  })

  it('reload PX should call declare wanted once empty and again with the desired px', () => {
    const px = createLoadablePX('urn')
    const action = reloadScenePortableExperience(px)

    return expectSaga(portableExperienceSaga)
      .provide([
        [select(getDesiredPortableExperiences), [px]],
        [call(declareWantedPortableExperiences, []), []],
        [delay(250), []],
        [call(declareWantedPortableExperiences, [px]), []]
      ])
      .dispatch(action)
      .call(declareWantedPortableExperiences, [])
      .delay(250)
      .call(declareWantedPortableExperiences, [px])
      .silentRun(0)
  })

  it('updateEnginePortableExperiences triggers a change in the engine (debounced)', () => {
    const px = createLoadablePX('urn')

    return expectSaga(portableExperienceSaga)
      .provide([[call(declareWantedPortableExperiences, [px]), []]])
      .dispatch(updateEnginePortableExperiences([])) // this one should not be triggered
      .dispatch(updateEnginePortableExperiences([px]))
      .not.call(declareWantedPortableExperiences, [])
      .call(declareWantedPortableExperiences, [px])
      .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY())
  })

  it('returning a PX multiple times should dedup the px', () => {
    const px1 = createLoadablePX('urnA')
    const px2 = createLoadablePX('urnA')
    const px3 = createLoadablePX('urnB')

    const ret = getDesiredPortableExperiences({
      portableExperiences: {
        portableExperiencesCreatedByScenesList: {
          urnA: px1,
          urnB: px3
        },
        deniedPortableExperiencesFromRenderer: [],
        kernelPortableExperiences: {},
        globalPortalExperienceShutDown: false
      },
      wearablesPortableExperiences: {
        desiredWearablePortableExperiences: {
          urnA: px2
        }
      }
    })

    expect(ret).to.deep.eq([px1, px3])
  })

  function state(theState: RootPortableExperiencesState): RootPortableExperiencesState {
    return Object.assign((reducers as any)(), theState)
  }

  describe('with reducer', () => {
    it('removing a PX from denylist should start it', () => {
      const pxOld = createLoadablePX('urn-old')
      const pxDenied = createLoadablePX('urn-denied')

      return expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: ['urn-denied'],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [pxOld.id]: pxOld,
                [pxDenied.id]: pxDenied
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .provide([[call(declareWantedPortableExperiences, [pxOld, pxDenied]), []]])
        .dispatch(denyPortableExperiences([]))
        .call(declareWantedPortableExperiences, [pxOld, pxDenied])
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [pxOld.id]: pxOld,
                [pxDenied.id]: pxDenied
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .put(updateEnginePortableExperiences([pxOld, pxDenied]))
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY())
    })

    it('adding a denied PX should not trigger any action', () => {
      const pxOld = createLoadablePX('urn-old')
      const pxDenied = createLoadablePX('urn-denied')

      return expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: ['urn-denied'],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [pxOld.id]: pxOld
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .provide([[call(declareWantedPortableExperiences, [pxOld]), []]])
        .dispatch(addScenePortableExperience(pxDenied))
        .call(declareWantedPortableExperiences, [pxOld])
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: ['urn-denied'],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [pxOld.id]: pxOld,
                [pxDenied.id]: pxDenied
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .put(updateEnginePortableExperiences([pxOld]))
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY())
    })

    it('removing a scene-created PX should remove it from the final list', () => {
      const pxOld = createLoadablePX('urn-old')

      return expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [pxOld.id]: pxOld
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .provide([[call(declareWantedPortableExperiences, []), []]])
        .dispatch(removeScenePortableExperience(pxOld.id))
        .call(declareWantedPortableExperiences, [])
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {},
              globalPortalExperienceShutDown: false
            }
          })
        )
        .put(updateEnginePortableExperiences([]))
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY())
    })
  })

  describe('santi use case', async () => {
    const px = createLoadablePX('urn:decentraland:off-chain:static-portable-experiences:radio')

    // add debug px
    it('add the debug px, the desired PX should contain it', () =>
      expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {},
              globalPortalExperienceShutDown: false
            }
          })
        )
        .provide([[call(declareWantedPortableExperiences, [px]), []]])
        .dispatch(addScenePortableExperience(px))
        .call(declareWantedPortableExperiences, [px])
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .put(updateEnginePortableExperiences([px]))
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY()))

    // deny list it
    it('add it to the denylist, now the desired PX should be an empty list', () =>
      expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .provide([[call(declareWantedPortableExperiences, []), []]])
        .dispatch(denyPortableExperiences([px.id]))
        .call(declareWantedPortableExperiences, [])
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [px.id],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .put(updateEnginePortableExperiences([]))
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY()))

    // remove from deny list
    it('remove it from the denylist, the desired PX should include the allowed PX', () =>
      expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [px.id],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .provide([[call(declareWantedPortableExperiences, [px]), []]])
        .dispatch(denyPortableExperiences([]))
        .call(declareWantedPortableExperiences, [px])
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .put(updateEnginePortableExperiences([px]))
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY()))

    it('shutdown the PX, this should shutdown all the PX', () =>
      expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: false
            }
          })
        )
        .dispatch(killAllPortableExperiences())
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {
                [px.id]: px
              },
              globalPortalExperienceShutDown: true
            }
          })
        )
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY()))

    it('activate the PX, this should activate all the PX that has been shutdown', () =>
      expectSaga(portableExperienceSaga)
        .withReducer(reducers)
        .withState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {},
              globalPortalExperienceShutDown: true
            }
          })
        )
        .dispatch(activateAllPortableExperiences())
        .hasFinalState(
          state({
            portableExperiences: {
              deniedPortableExperiencesFromRenderer: [],
              kernelPortableExperiences: {},
              portableExperiencesCreatedByScenesList: {},
              globalPortalExperienceShutDown: false
            }
          })
        )
        .silentRun(PORTABLE_EXPERIENCES_DEBOUNCE_DELAY()))
  })
})
