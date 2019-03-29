import {
  Entity,
  Observable,
  engine,
  Transform,
  executeTask,
  GLTFShape,
  TextShape,
  Vector3,
  Quaternion,
  Component,
  Scalar,
  OnPointerDown
} from 'decentraland-ecs/src'
import {
  ReceiveUserDataMessage,
  UUID,
  ReceiveUserPoseMessage,
  ReceiveUserVisibleMessage,
  UserRemovedMessage,
  UserMessage,
  AvatarMessageType,
  AvatarMessage,
  Pose,
  UserInformation
} from 'shared/comms/types'
import { execute } from './rpc'
import { ComponentGroup } from 'decentraland-ecs/src/ecs/ComponentGroup'
import { showAvatarWindow, currentAvatarId, hideAvatarWindow } from './avatarWindow'

export const avatarMessageObservable = new Observable<AvatarMessage>()

const models: Map<string, GLTFShape> = new Map()
const avatarMap = new Map<string, AvatarEntity>()

function getModel(src: string) {
  let model = models.get(src)

  if (!model) {
    model = new GLTFShape(src)

    models.set(src, model)
  }

  return model
}

function getAvatarModel(avatarName: string) {
  if (avatarName.endsWith('.gltf') || avatarName.endsWith('.glb')) {
    return getModel(`models/avatar/${avatarName}`)
  }
  return getModel(`models/avatar/${avatarName}/head.glb`)
}

function cleanupUnusedModels() {
  /**
   * Let S be a set of GLTFModel
   *
   * For each avatar A in avatarMap
   *   Add every model used by this entities to S
   *
   * For each model M in models
   *   if M is not present in S
   *     remove M from the engine and modeld
   */
  let usedModels: Set<GLTFShape> = new Set()
  avatarMap.forEach($ => {
    const model = $.body.getComponentOrNull(GLTFShape)
    if (model) {
      usedModels.add(model)
    }
  })

  new Map(models).forEach(($, key) => {
    if (!usedModels.has($)) {
      models.delete(key)
    }
  })
}

@Component('animatedTransform')
export class TargetTransform {
  source: Transform = new Transform()
  target: Transform = new Transform()

  out: Transform = new Transform()

  endTime = 0
  currentTime = 0

  animate(currentState: Transform, duration: number) {
    this.out = currentState
    this.source.position.copyFrom(currentState.position)
    this.source.scale.copyFrom(currentState.scale)
    this.source.rotation.copyFrom(currentState.rotation)
    this.currentTime = 0
    this.endTime = duration / 1000
  }

  update(dt: number) {
    if (this.currentTime < this.endTime) {
      this.currentTime += dt
    }

    this.currentTime = Scalar.Clamp(this.currentTime, 0, this.endTime)

    const d = this.currentTime / this.endTime

    Vector3.LerpToRef(this.source.position, this.target.position, d, this.out.position)
    Vector3.LerpToRef(this.source.scale, this.target.scale, d, this.out.scale)
    Quaternion.SlerpToRef(this.source.rotation, this.target.rotation, d, this.out.rotation)
  }
}

export class InterpolatorSystem {
  group: ComponentGroup = engine.getComponentGroup(TargetTransform)

  update(dt: number) {
    for (let entity of this.group.entities) {
      const animator = entity.getComponent(TargetTransform)
      animator.update(dt)
    }
  }
}

engine.addSystem(new InterpolatorSystem())

export class AvatarEntity extends Entity {
  blocked = false
  muted = false
  visible = true

  displayName = 'Avatar'
  publicKey = '0x00000000000000000000000000000000'

  readonly label: TextShape = new TextShape(this.displayName)
  readonly body: Entity = new Entity()
  readonly labelEntity = new Entity()
  readonly transformAnimation: TargetTransform = this.getComponentOrCreate(TargetTransform)
  readonly transform: Transform = this.getComponentOrCreate(Transform)

  constructor(public name: string) {
    super(name)

    const clicked = new OnPointerDown(this.clicked)

    {
      this.labelEntity.setParent(this)
      this.labelEntity.getComponentOrCreate(Transform).position.y = 2
      this.label.billboard = true
      this.label.isPickable = true
      this.labelEntity.addComponent(this.label)
      this.labelEntity.addComponent(clicked)
    }

    this.body.setParent(this)
    this.body.addComponent(clicked)

    // we need this component to filter the interpolator system
    this.getComponentOrCreate(Transform)

    this.setVisible(true)
  }

  clicked = () => {
    showAvatarWindow({
      displayName: this.displayName,
      isBlocked: this.blocked,
      isMuted: this.muted,
      publicKey: this.publicKey,
      uuid: this.name
    })
  }

  setBlocked(blocked: boolean, muted: boolean): void {
    this.blocked = blocked
    this.muted = muted
  }

  setVisible(visible: boolean): void {
    this.visible = visible
    this.updateVisibility()
  }

  setUserData(userData: Partial<UserInformation>): void {
    if (userData.avatarType) {
      const model = getAvatarModel(userData.avatarType)

      this.body.addComponentOrReplace(model)

      cleanupUnusedModels()
    }

    if (userData.pose) {
      this.setPose(userData.pose)
    }

    if (userData.displayName) {
      this.setDisplayName(userData.displayName)
    }
  }

  setDisplayName(name: string) {
    this.label.value = name
    this.displayName = name
  }

  setPose(pose: Pose): void {
    const [x, y, z, Qx, Qy, Qz, Qw] = pose

    if (this.transform.position.equalsToFloats(0, 0, 0)) {
      this.transform.position.set(x, y, z)
      this.transform.rotation.set(Qx, Qy, Qz, Qw)
    }

    this.transformAnimation.target.position.set(x, y, z)
    this.transformAnimation.target.rotation.set(Qx, Qy, Qz, Qw)

    this.transformAnimation.animate(this.transform, 100 /* ms */)
  }

  private updateVisibility() {
    const visible = this.visible && !this.blocked

    if (!visible && this.isAddedToEngine()) {
      engine.removeEntity(this)
    } else if (visible && !this.isAddedToEngine()) {
      engine.addEntity(this)
    }
  }
}

/**
 * for every UUID, ensures that exist an avatar and a user in the local state.
 * Returns the AvatarEntity instance
 * @param uuid
 */
function ensureAvatar(uuid: UUID): AvatarEntity | null {
  let avatar = avatarMap.get(uuid)

  if (avatar) {
    return avatar
  }

  avatar = new AvatarEntity(uuid)

  avatar.setUserData({ avatarType: 'square-robot' })

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

function handleUserRemoved({ uuid }: UserRemovedMessage): void {
  const avatar = avatarMap.get(uuid)
  if (avatar) {
    avatarMap.delete(uuid)
    engine.removeEntity(avatar)
  }
  if (uuid === currentAvatarId) {
    hideAvatarWindow()
  }
  cleanupUnusedModels()
}

function handleShowWindow({ uuid }: UserMessage): void {
  const avatar = avatarMap.get(uuid)

  if (avatar) {
    avatar.clicked()
  }
}

function handleMutedBlockedMessages({ uuid }: UserMessage): void {
  executeTask(hideBlockedUsers)
}

avatarMessageObservable.add(evt => {
  if (evt.type === AvatarMessageType.USER_DATA) {
    handleUserData(evt)
  } else if (evt.type === AvatarMessageType.USER_POSE) {
    handleUserPose(evt)
  } else if (evt.type === AvatarMessageType.USER_VISIBLE) {
    handleUserVisible(evt)
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
  } else if (evt.type === AvatarMessageType.SHOW_WINDOW) {
    handleShowWindow(evt)
  }
})
