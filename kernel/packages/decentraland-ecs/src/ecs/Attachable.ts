import { IEntity, IEngine } from "./IEntity";

/**
 * Entities can be attached to each other by using the `setParent` method. However, there are cases where we might want to attach entities
 * to other objects that are not entities created by the same scene (for example, the player's avatar). For those cases, we have this class.
 */
export abstract class Attachable {

  /** Used to attach entities to the avatar */
  static readonly AVATAR: Attachable = { getEntity: (engine: IEngine) => engine.avatarEntity }
  /** Used to attach entities to the avatar, but when the camera is in first person mode, the attached entities rotate with the camera */
  static readonly PLAYER: Attachable = { getEntity: (engine: IEngine) => engine.playerEntity }

  // @internal
  abstract getEntity(engine: IEngine): IEntity;
}

