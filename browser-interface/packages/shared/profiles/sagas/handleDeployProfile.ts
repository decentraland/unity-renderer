import { Authenticator } from '@dcl/crypto'
import { hashV1 } from '@dcl/hashing'
import { Avatar, EntityType, Profile, Snapshots } from '@dcl/schemas'
import { ContentClient } from 'dcl-catalyst-client/dist/ContentClient'
import type { DeploymentData } from 'dcl-catalyst-client/dist/utils/DeploymentBuilder'
import { base64ToBuffer } from 'lib/encoding/base64ToBlob'
import { call, put, select } from 'redux-saga/effects'
import { trackEvent } from 'shared/analytics'
import defaultLogger from 'lib/logger'
import { getProfilesContentServerFromRealmAdapter } from 'shared/realm/selectors'
import type { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { getCurrentIdentity, getCurrentUserId } from 'shared/session/selectors'
import type { ExplorerIdentity } from 'shared/session/types'
import type { DeployProfile } from '../actions'
import { deployProfileFailure, deployProfileSuccess } from '../actions'
import { buildServerMetadata } from 'lib/decentraland/profiles/transformations/profileToServerFormat'
import type { ContentFile } from '../types'

export function* handleDeployProfile(deployProfileAction: DeployProfile) {
  const realmAdapter: IRealmAdapter = yield call(waitForRealm)
  const profileServiceUrl: string = yield call(getProfilesContentServerFromRealmAdapter, realmAdapter)

  const identity: ExplorerIdentity = yield select(getCurrentIdentity)
  const userId: string = yield select(getCurrentUserId)
  const profile: Avatar = deployProfileAction.payload.profile
  try {
    yield call(deployAvatar, {
      url: profileServiceUrl,
      userId,
      identity,
      profile
    })
    yield put(deployProfileSuccess(userId, profile.version, profile))
  } catch (e: any) {
    trackEvent('error', {
      context: 'kernel#saga',
      message: 'error deploying profile. ' + e.message,
      stack: e.stacktrace
    })
    defaultLogger.error('Error deploying profile!', e)
    yield put(deployProfileFailure(userId, profile, e))
  }
}

async function buildSnapshotContent(selector: string, value: string) {
  let hash: string
  let contentFile: ContentFile | undefined

  const name = `${selector}.png`

  if (value.includes('://')) {
    // value is already a URL => use existing hash
    hash = value.split('/').pop()!
  } else {
    // value is coming in base 64 => convert to blob & upload content
    const buffer = base64ToBuffer(value)
    contentFile = await makeContentFile(name, buffer)
    hash = await hashV1(contentFile.content)
  }

  return { name, hash, contentFile }
}

export async function deployAvatar(params: {
  url: string
  userId: string
  identity: ExplorerIdentity
  profile: Avatar
}) {
  const { url, profile, identity } = params
  const { avatar } = profile

  const newAvatar = { ...avatar }

  const files = new Map<string, Buffer>()

  const snapshots = avatar.snapshots || (profile as any).snapshots
  const content = new Map()

  if (snapshots) {
    const newSnapshots: Record<string, string> = {}
    for (const [selector, value] of Object.entries(snapshots)) {
      const { name, hash, contentFile } = await buildSnapshotContent(selector, value as any)

      newSnapshots[selector] = hash
      content.set(name, hash)
      contentFile && files.set(contentFile.name, Buffer.from(contentFile.content))
    }
    newAvatar.snapshots = newSnapshots as Snapshots
  }

  const metadata = buildServerMetadata({ ...profile, avatar: newAvatar })

  return deploy(url, identity, metadata, files, content)
}

async function deploy(
  url: string,
  identity: ExplorerIdentity,
  metadata: Profile,
  contentFiles: Map<string, Buffer>,
  contentHashes: Map<string, string>
) {
  // Build the client
  const catalyst = new ContentClient({ contentUrl: url })

  const entityWithoutNewFilesPayload = {
    type: EntityType.PROFILE,
    pointers: [identity.address],
    hashesByKey: contentHashes,
    metadata
  }

  // Build entity and group all files
  const preparationData = await (contentFiles.size
    ? catalyst.buildEntity({ type: EntityType.PROFILE, pointers: [identity.address], files: contentFiles, metadata })
    : catalyst.buildEntityWithoutNewFiles(entityWithoutNewFilesPayload))
  // sign the entity id
  const authChain = Authenticator.signPayload(identity, preparationData.entityId)
  // Build the deploy data
  const deployData: DeploymentData = { ...preparationData, authChain }
  // Deploy the actual entity
  return catalyst.deployEntity(deployData)
}

async function makeContentFile(path: string, content: string | Blob | Buffer): Promise<ContentFile> {
  if (Buffer.isBuffer(content)) {
    return { name: path, content }
  } else if (typeof content === 'string') {
    const buffer = Buffer.from(content)
    return { name: path, content: buffer }
  } else {
    throw new Error('Unable to create ContentFile: content must be a string or a Blob')
  }
}
