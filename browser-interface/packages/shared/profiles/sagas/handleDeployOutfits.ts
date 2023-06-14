import { Authenticator } from '@dcl/crypto'
import { EntityType, Outfits } from '@dcl/schemas'
import { BuildEntityOptions, BuildEntityWithoutFilesOptions, ContentClient, DeploymentData } from 'dcl-catalyst-client'
import defaultLogger from 'lib/logger'
import { call, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics/trackEvent'
import { getProfilesContentServerFromRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { getCurrentIdentity, getCurrentUserId } from 'shared/session/selectors'
import { ExplorerIdentity } from 'shared/session/types'
import { DeployOutfits } from '../actions'

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
  const contentHashes = params.contentHashes || new Map<string, string>()

  // Build the client
  const catalyst = new ContentClient({ contentUrl: url })

  const entityWithoutNewFilesPayload = {
    type: EntityType.OUTFITS,
    pointers: [identity.address],
    hashesByKey: contentHashes,
    metadata: outfits
  } as unknown as BuildEntityWithoutFilesOptions // TODO Juli: update crypto lib to support outfits entity type

  const entity = {
    type: EntityType.OUTFITS,
    pointers: [identity.address],
    files: contentFiles,
    metadata: outfits
  } as unknown as BuildEntityOptions // TODO Juli: update crypto lib to support outfits entity type

  // Build entity and group all files
  const preparationData = await (contentFiles.size
    ? catalyst.buildEntity(entity)
    : catalyst.buildEntityWithoutNewFiles(entityWithoutNewFilesPayload))

  // Sign the entity id
  const authChain = Authenticator.signPayload(identity, preparationData.entityId)

  // Build the deploy data
  const deployData: DeploymentData = { ...preparationData, authChain }

  // Deploy the actual entity
  return catalyst.deploy(deployData)
}
