import { engine, Entity, executeTask, Observable, Transform, EventManager } from 'decentraland-ecs/src'
import { AvatarShape } from 'decentraland-ecs/src/decentraland/AvatarShape'
import {
  AvatarMessage,
  AvatarMessageType,
  Pose,
  ReceiveUserDataMessage,
  ReceiveUserExpressionMessage,
  ReceiveUserPoseMessage,
  ReceiveUserTalkingMessage,
  ReceiveUserVisibleMessage,
  UserInformation,
  UserMessage,
  UserRemovedMessage,
  UUID
} from 'shared/comms/interface/types'
import { execute } from './rpc'

export const avatarMessageObservable = new Observable<AvatarMessage>()

const avatarMap = new Map<string, AvatarEntity>()

export class AvatarEntity extends Entity {
  blocked = false
  muted = false
  visible = true

  readonly transform: Transform
  avatarShape!: AvatarShape

  constructor(uuid?: string, avatarShape = new AvatarShape()) {
    super(uuid)
    this.avatarShape = avatarShape

    this.addComponentOrReplace(this.avatarShape)
    this.eventManager = new EventManager()
    this.eventManager.fireEvent

    // we need this component to filter the interpolator system
    this.transform = this.getComponentOrCreate(Transform)
  }

  loadProfile(user: Partial<UserInformation>) {
    const { profile } = user
    if (profile) {
      const { avatar } = profile

      const shape = this.avatarShape
      shape.id = profile.userId
      shape.name = profile.name

      shape.bodyShape = avatar.bodyShape
      shape.wearables = avatar.wearables
      shape.skinColor = avatar.skinColor
      shape.hairColor = avatar.hairColor
      shape.eyeColor = avatar.eyeColor
      shape.expressionTriggerId = user.expression ? user.expression.expressionType || 'Idle' : 'Idle'
      shape.expressionTriggerTimestamp = user.expression ? user.expression.expressionTimestamp || 0 : 0
    }
    this.setVisible(true)
  }

  setBlocked(blocked: boolean, muted: boolean): void {
    this.blocked = blocked
    this.muted = muted
  }

  setVisible(visible: boolean): void {
    this.visible = visible
    this.updateVisibility()
  }

  setTalking(talking: boolean): void {
    this.avatarShape.talking = talking
  }

  setUserData(userData: Partial<UserInformation>): void {
    if (userData.pose) {
      this.setPose(userData.pose)
    }

    if (userData.profile) {
      this.loadProfile(userData)
    }
  }

  setExpression(id: string, timestamp: number): void {
    const shape = this.avatarShape
    shape.expressionTriggerId = id
    shape.expressionTriggerTimestamp = timestamp
  }

  setPose(pose: Pose): void {
    const [x, y, z, Qx, Qy, Qz, Qw] = pose

    this.transform.position.set(x, y, z)
    this.transform.rotation.set(Qx, Qy, Qz, Qw)
  }

  public remove() {
    if (this.isAddedToEngine()) {
      engine.removeEntity(this)
      avatarMap.delete(this.uuid)
    }
  }

  private updateVisibility() {
    const visible = this.visible && !this.blocked
    if (!visible && this.isAddedToEngine()) {
      this.remove()
    } else if (visible && !this.isAddedToEngine()) {
      engine.addEntity(this)
    }
  }
}

/**
 * For every UUID, ensures synchronously that an avatar exists in the local state.
 * Returns the AvatarEntity instance
 * @param uuid
 */
function ensureAvatar(uuid: UUID): AvatarEntity {
  let avatar = avatarMap.get(uuid)

  if (avatar) {
    return avatar
  }

  avatar = new AvatarEntity(uuid)
  avatarMap.set(uuid, avatar)

  executeTask(hideBlockedUsers)

  return avatar
}

async function getBlockedUsers(): Promise<Array<string>> {
  return execute('SocialController', 'getBlockedUsers', [])
}

async function getMutedUsers(): Promise<Array<string>> {
  return execute('SocialController', 'getMutedUsers', [])
}

/**
 * Unblocks the users that are not in that list.
 */
async function hideBlockedUsers(): Promise<void> {
  const blockedUsers = await getBlockedUsers()
  const mutedUsers = await getMutedUsers()

  avatarMap.forEach((avatar, uuid) => {
    const blocked = blockedUsers.includes(uuid)
    const muted = blocked || mutedUsers.includes(uuid)
    avatar.setBlocked(blocked, muted)
  })
}

function handleUserData(message: ReceiveUserDataMessage): void {
  const avatar = ensureAvatar(message.uuid)

  if (avatar) {
    const userData = message.data

    avatar.setUserData(userData)
  }
}

function handleUserPose({ uuid, pose }: ReceiveUserPoseMessage): boolean {
  const avatar = ensureAvatar(uuid)

  if (!avatar) {
    return false
  }

  avatar.setPose(pose)

  return true
}

function handleUserExpression({ uuid, expressionId, timestamp }: ReceiveUserExpressionMessage): boolean {
  const avatar = ensureAvatar(uuid)

  if (!avatar) {
    return false
  }

  avatar.setExpression(expressionId, timestamp)

  return true
}

/**
 * In some cases, like minimizing the window, the user will be invisible to the rest of the world.
 * This function handles those visible changes.
 */
function handleUserVisible({ uuid, visible }: ReceiveUserVisibleMessage): void {
  const avatar = ensureAvatar(uuid)

  if (avatar) {
    avatar.setVisible(visible)
  }
}

function handleUserTalkingUpdate({ uuid, talking }: ReceiveUserTalkingMessage): void {
  const avatar = ensureAvatar(uuid)

  if (avatar) {
    avatar.setTalking(talking)
  }
}

function handleUserRemoved({ uuid }: UserRemovedMessage): void {
  const avatar = avatarMap.get(uuid)
  if (avatar) {
    avatar.remove()
  }
}

function handleShowWindow({ uuid }: UserMessage): void {
  // noop
}

function handleMutedBlockedMessages({ uuid }: UserMessage): void {
  executeTask(hideBlockedUsers)
}

avatarMessageObservable.add((evt) => {
  if (evt.type === AvatarMessageType.USER_DATA) {
    handleUserData(evt)
  } else if (evt.type === AvatarMessageType.USER_POSE) {
    handleUserPose(evt)
  } else if (evt.type === AvatarMessageType.USER_VISIBLE) {
    handleUserVisible(evt)
  } else if (evt.type === AvatarMessageType.USER_EXPRESSION) {
    handleUserExpression(evt)
  } else if (evt.type === AvatarMessageType.USER_REMOVED) {
    handleUserRemoved(evt)
  } else if (evt.type === AvatarMessageType.USER_MUTED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_BLOCKED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_UNMUTED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_UNBLOCKED) {
    handleMutedBlockedMessages(evt)
  } else if (evt.type === AvatarMessageType.USER_TALKING) {
    handleUserTalkingUpdate(evt as ReceiveUserTalkingMessage)
  } else if (evt.type === AvatarMessageType.SHOW_WINDOW) {
    handleShowWindow(evt)
  }
})
