import { IEntity, IEngine } from "./IEntity"

/**
 * Entities can be attached to each other by using the `setParent` method. However, there are cases where we might want to attach entities
 * to other objects that are not entities created by the same scene (for example, the player's avatar). For those cases, we have this class.
 */
export abstract class Attachable {

  /** Used to attach entities to the avatar. Entities will follow the avatar when it moves */
  static readonly AVATAR: Attachable = { getEntityRepresentation: (engine: IEngine) => engine.avatarEntity }
  /** Used to attach entities to the camera. When in first person mode, the attached entities will also rotate with the camera */
  static readonly FIRST_PERSON_CAMERA: Attachable = { getEntityRepresentation: (engine: IEngine) => engine.firstPersonCameraEntity }

  // @internal
  /** Entities must be attached to entities, so in this case, each attachable object must return the entity used to present it */
  abstract getEntityRepresentation(engine: IEngine): IEntity
}
