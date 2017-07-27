import * as blockies from 'ethereum-blockies'
import { BaseEntity } from 'engine/entities/BaseEntity'
import { validators } from 'engine/components/helpers/schemaValidator'
import { playerConfigurations } from 'config'
import { avatarTypes, loadAvatarModel, createGltfChild } from './avatarHelpers'
import { audioEngine } from 'engine/renderer/init'
import { profileObservable, ProfileEvent } from 'shared/comms/profile'
import { SharedSceneContext } from 'engine/entities/SharedSceneContext'
import { uuid } from 'atomicHelpers/math'
import { setEntityText } from 'engine/components/ephemeralComponents/TextShape'
import { Vector3Component, QuaternionComponent } from 'shared/types'
import { Color3 } from 'decentraland-ecs/src'

export type AvatarAttributes = {
  displayName: string
  publicKey: string
  avatarType?: string
  leftHandPosition: Vector3Component
  rightHandPosition: Vector3Component

  headRotation: Vector3Component
  leftHandRotation: QuaternionComponent
  rightHandRotation: QuaternionComponent

  muted: boolean

  blocked: boolean

  visible: boolean
}

const icon = blockies.create({
  seed: '0',
  size: 16,
  scale: 8
})

const defaultAttributes: AvatarAttributes = {
  displayName: 'Avatar Name',
  publicKey: '0x0000000000000000000000000000000000000000',
  headRotation: { x: 0, y: 0, z: 0 },
  leftHandPosition: { x: 0, y: 0, z: 0 },
  leftHandRotation: { x: 0, y: 0, z: 0, w: 1 },
  rightHandPosition: { x: 0, y: 0, z: 0 },
  rightHandRotation: { x: 0, y: 0, z: 0, w: 1 },
  muted: false,
  blocked: false,
  visible: false
}

export const avatarContext = new SharedSceneContext('/', 'avatar-context', false)

export class AvatarEntity extends BaseEntity {
  uuid: string
  head: ReturnType<typeof createGltfChild>
  body: ReturnType<typeof createGltfChild>
  leftHand: ReturnType<typeof createGltfChild>
  rightHand: ReturnType<typeof createGltfChild>
  removeEmoji: number
  sound: MediaStreamAudioSourceNode | null
  gainNode: GainNode | null
  displayName: BaseEntity
  attrs: AvatarAttributes = { ...defaultAttributes }
  profileObserver: BABYLON.Observer<any>
  private lastAvatar: string = ''

  private _stream: MediaStream

  public get stream(): MediaStream {
    return this._stream
  }

  public set stream(stream: MediaStream) {
    if (this._stream === stream) return

    // release allocated stream
    if (this._stream) {
      // firefox doesnt have the 'stop' method
      if (!this.stream.stop) {
        this._stream.getTracks().forEach(track => track.stop())
      } else {
        this.stream.stop()
      }
    }

    this._stream = stream

    // if we had a .sound, disconnect it
    if (this.sound) {
      this.sound.disconnect()
      this.gainNode.disconnect()
    }

    // if we have a new stream, create the .sound again
    if (stream) {
      this.sound = audioEngine.audioContext.createMediaStreamSource(stream)
      this.gainNode = audioEngine.audioContext.createGain()
      this.sound.connect(this.gainNode)
      this.gainNode.connect(audioEngine.audioContext.destination)
    } else {
      delete this.sound
    }
  }

  constructor(id: string) {
    super(id, avatarContext)

    this.displayName = new BaseEntity(uuid(), this.context)
    this.displayName.setParentEntity(this)
    this.displayName.addListener('onClick', $ => this.dispatchUUIDEvent('onClick', $))

    this.setAttributes(defaultAttributes)
    this.initInteraction()

    this.profileObserver = profileObservable.add((event: any) => {
      if (event.publicKey !== this.attrs.publicKey) {
        return
      }

      if (event.type === ProfileEvent.MUTE) {
        this.setAttributes({ muted: false })
      }

      if (event.type === ProfileEvent.UNMUTE) {
        this.setAttributes({ muted: true })
      }

      if (event.type === ProfileEvent.BLOCK) {
        this.setAttributes({ visible: false, muted: true, blocked: true })
      }

      if (event.type === ProfileEvent.UNBLOCK) {
        this.setAttributes({ visible: true, muted: false, blocked: false })
      }
    })
  }

  disposeModels() {
    this.lastAvatar = ''

    if (this.head) {
      this.head.disposeTree()
      this.head = null
    }

    if (this.body) {
      this.body.disposeTree()
      this.body = null
    }

    if (this.leftHand) {
      this.leftHand.disposeTree()
      this.leftHand = null
    }

    if (this.rightHand) {
      this.rightHand.disposeTree()
      this.rightHand = null
    }
  }

  dispose() {
    this.stream = null // this removes the .sound and also the .stream safely closing streams and channels
    this.disposeModels()
    this.displayName.disposeTree()
    this.displayName = null
    profileObservable.remove(this.profileObserver)
    super.dispose()
  }

  initInteraction() {
    this.addListener('onClick', () => {
      this.showProfile()
    })
  }

  regenerateAvatar(avatarType: string = defaultAttributes.avatarType) {
    if (this.lastAvatar === avatarType) {
      return
    }

    this.context.logger.log(this.id, 'regenerateAvatar', avatarType)

    this.disposeModels()

    this.lastAvatar = avatarType
    let avatarModel: ReturnType<typeof loadAvatarModel>

    // If avatar type provided isn't supported, generate square robot
    if (avatarTypes.has(avatarType)) {
      avatarModel = loadAvatarModel(avatarType, this.context)
    } else {
      avatarModel = loadAvatarModel('square-robot', this.context)
    }

    this.head = avatarModel.head
    this.head.position.y = playerConfigurations.height * -0.1
    this.head.setParentEntity(this)
    this.head.addListener('onClick', $ => this.dispatchUUIDEvent('onClick', $))

    this.body = avatarModel.body
    this.body.position.y = playerConfigurations.height * -0.4
    this.body.setParentEntity(this)
    this.body.addListener('onClick', $ => this.dispatchUUIDEvent('onClick', $))

    this.leftHand = avatarModel.leftHand
    this.leftHand.position.x -= playerConfigurations.handFromBodyDistance
    // Make body a parent to move/rotate hand model around the body
    this.leftHand.setParentEntity(this.body)
    this.leftHand.addListener('onClick', $ => this.dispatchUUIDEvent('onClick', $))

    this.rightHand = avatarModel.rightHand
    this.rightHand.position.x += playerConfigurations.handFromBodyDistance
    // Make body a parent to move/rotate hand model around the body
    this.rightHand.setParentEntity(this.body)
    this.rightHand.addListener('onClick', $ => this.dispatchUUIDEvent('onClick', $))

    this.setDisplayText()
  }

  showProfile() {
    blockies.render(
      {
        seed: this.attrs.publicKey,
        size: 16,
        scale: 8
      },
      icon
    )

    profileObservable.notifyObservers({
      type: ProfileEvent.SHOW_PROFILE,
      displayName: this.attrs.displayName,
      publicKey: this.attrs.publicKey,
      avatarType: this.attrs.avatarType,
      isMuted: this.attrs.muted,
      isBlocked: this.attrs.blocked,
      avatarUrl: icon.toDataURL()
    })

    document.exitPointerLock()
  }
  // tslint:disable-next-line:prefer-function-over-method
  hideProfile() {
    profileObservable.notifyObservers({ type: ProfileEvent.HIDE_PROFILE })
  }

  setDisplayText() {
    setEntityText(this.displayName, {
      value: this.attrs.displayName,
      color: new Color3(0, 0, 0),
      fontSize: 40,
      isPickable: true,
      billboard: true
    })

    if (this.head) {
      this.displayName.position.set(this.head.position.x, this.head.position.y + 0.6, this.head.position.z)
    } else {
      this.displayName.position.set(0, 1.6, 0)
    }
  }

  setAttributes(data: Partial<AvatarAttributes>) {
    this.attrs = { ...this.attrs, ...data }
    for (let i in data) {
      this.setAttribute(i as any, data[i])
    }
  }

  setVolume(volume: number) {
    this.gainNode.gain.setValueAtTime(volume, audioEngine.audioContext.currentTime)
  }

  setAttribute<K extends keyof AvatarAttributes>(key: K, value: any) {
    // TODO: get rid of this value: any
    if (key === 'headRotation') {
      this.head && this.head.rotation.copyFrom(value)
      return
    }

    if (key === 'leftHandRotation' && this.leftHand) {
      this.leftHand.rotation.copyFrom(value)
    }

    if (key === 'leftHandPosition' && this.leftHand) {
      this.leftHand.position.set(value.x - this.position.x, value.y - this.position.y, value.z - this.position.z)
      return
    }

    if (key === 'rightHandRotation' && this.rightHand) {
      this.rightHand.rotation.copyFrom(value)
    }

    if (key === 'rightHandPosition' && this.rightHand) {
      this.rightHand.position.set(value.x - this.position.x, value.y - this.position.y, value.z - this.position.z)
      return
    }

    if (key === 'muted' && this._stream) {
      const muted = validators.boolean(value, defaultAttributes.muted)
      if (this.sound) {
        this._stream.getAudioTracks().forEach($ => ($.enabled = !muted))
        muted ? this.setVolume(0) : this.setVolume(1)
      }
      return
    }

    if (key === 'avatarType') {
      this.regenerateAvatar(validators.string(value, defaultAttributes.avatarType))
      return
    }

    if (key === 'displayName') {
      this.setDisplayText()
      return
    }
  }
}
