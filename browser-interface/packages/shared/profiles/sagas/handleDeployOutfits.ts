import { Authenticator } from '@dcl/crypto'
import { EntityType, Outfits } from '@dcl/schemas'
import { DeploymentData, createContentClient } from 'dcl-catalyst-client'
import defaultLogger from 'lib/logger'
import { call, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { getProfilesContentServerFromRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { getCurrentIdentity, getCurrentUserId } from 'shared/session/selectors'
import { ExplorerIdentity } from 'shared/session/types'
import { DeployOutfits } from '../actions'
import { createFetchComponent } from '@well-known-components/fetch-component'
import { BuildEntityOptions } from 'dcl-catalyst-client/dist/client/types'
import { buildEntity } from 'dcl-catalyst-client/dist/client/utils/DeploymentBuilder'

export function* handleDeployOutfits(deployOutfitsAction: DeployOutfits) {
  const realmAdapter: IRealmAdapter = yield call(waitForRealm)
  const updateContentServiceUrl: string = yield call(getProfilesContentServerFromRealmAdapter, realmAdapter)

  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const userId: string = yield select(getCurrentUserId)
  const outfits: Outfits = deployOutfitsAction.payload.outfits
  try {
    yield call(deployOutfits, {
      url: updateContentServiceUrl,
      userId,
      identity,
      outfits
    })
  } catch (e: any) {
    trackEvent('error', {
      context: 'kernel#saga',
      message: 'error deploying outfits. ' + e.message,
      stack: e.stacktrace
    })
    defaultLogger.error('Error deploying outfits!', e)
  }
}

export async function deployOutfits(params: {
  url: string
  userId: string
  identity: ExplorerIdentity
  outfits: Outfits
  contentFiles?: Map<string, Uint8Array>
  contentHashes?: Map<string, string>
}) {
  const { url, identity, outfits } = params
  const contentFiles = params.contentFiles || new Map<string, Uint8Array>()

  // Build the client
  const fetcher = createFetchComponent()
  const catalyst = createContentClient({ url, fetcher })

  // The pointer for the outfits is: `<address>:outfits`
  const entity = {
    type: EntityType.OUTFITS,
    pointers: [identity.address + ':outfits'],
    files: contentFiles,
    metadata: outfits
  } as BuildEntityOptions

  // Build entity and group all files
  const preparationData = await buildEntity(entity)

  // Sign the entity id
  const authChain = Authenticator.signPayload(identity, preparationData.entityId)

  // Build the deploy data
  const deployData: DeploymentData = { ...preparationData, authChain }

  // Deploy the entity
  return catalyst.deploy(deployData)
}
