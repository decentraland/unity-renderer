import { expectSaga } from 'redux-saga-test-plan'
import { call, select } from 'redux-saga/effects'
import {
  handleItemsRequestFailure,
  handleItemRequest,
  handleItemsRequestSuccess,
  informRequestFailure,
  sendWearablesCatalog,
  WRONG_FILTERS_ERROR,
  sendEmotesCatalog
} from 'shared/catalogs/sagas'
import {
  emotesFailure,
  emotesRequest,
  emotesSuccess,
  wearablesFailure,
  wearablesRequest,
  wearablesSuccess
} from 'shared/catalogs/actions'
import { baseCatalogsLoaded, getPlatformCatalog } from 'shared/catalogs/selectors'
import { waitForRendererInstance } from 'shared/renderer/sagas-helper'

const serverUrl = 'https://server.com'
const wearableId1 = 'WearableId1'
const wearable1 = {
  id: wearableId1,
  baseUrl: serverUrl + 'contents/',
  baseUrlBundles: 'https://content-assets-as-bundle.decentraland.zone/'
} as any

const emoteId1 = 'EmoteId1'
const emote1 = {
  id: emoteId1,
  baseUrl: serverUrl + 'contents/',
  baseUrlBundles: 'https://content-assets-as-bundle.decentraland.zone/'
} as any

const userId = 'userId'
const context = 'someContext'

describe('Items saga', () => {
  const runs = [
    {
      it: 'wearables',
      options: {
        items: [wearable1],
        requestAction: wearablesRequest,
        requestParams: {
          wearableIds: ['some-id']
        },
        successAction: wearablesSuccess,
        failureAction: wearablesFailure,
        catalogAction: sendWearablesCatalog
      }
    },
    {
      it: 'emotes',
      options: {
        items: [emote1],
        requestAction: emotesRequest,
        requestParams: {
          emoteIds: ['some-id']
        },
        successAction: emotesSuccess,
        failureAction: emotesFailure,
        catalogAction: sendEmotesCatalog
      }
    }
  ]

  runs.forEach((run) => {
    describe(`When fetching ${run.it}`, () => {
      const { items, catalogAction, requestAction, requestParams, successAction, failureAction } = run.options
      it(`When ${run.it} fetch is successful, then it is sent to the renderer with the same context`, () => {
        return expectSaga(handleItemsRequestSuccess, successAction(items, context))
          .call(catalogAction, items, context)
          .provide([
            [call(waitForRendererInstance), true],
            [call(catalogAction, items, context), null]
          ])
          .run()
      })

      it('When more than one filter is set, then the request fails', () => {
        return expectSaga(handleItemRequest, requestAction({ ...requestParams, ownedByUser: userId }, context))
          .put(failureAction(context, WRONG_FILTERS_ERROR))
          .provide([
            [select(baseCatalogsLoaded), true],
            [select(getPlatformCatalog), {}]
          ])
          .run()
      })

      it('When collection id is not base-avatars, then the request fails', () => {
        return expectSaga(handleItemRequest, requestAction({ collectionIds: ['some-other-collection'] }, context))
          .put(failureAction(context, WRONG_FILTERS_ERROR))
          .provide([[select(baseCatalogsLoaded), true]])
          .run()
      })

      it('When request fails, then the failure is informed', () => {
        const errorMessage = 'Something failed'
        return expectSaga(handleItemsRequestFailure, failureAction(context, errorMessage))
          .call(informRequestFailure, errorMessage, context)
          .provide([
            [call(waitForRendererInstance), true],
            [call(informRequestFailure, errorMessage, context), null]
          ])
          .run()
      })
    })
  })
})
