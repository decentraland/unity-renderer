import { ObservableComponent, Component } from '../ecs/Component'
import { CLASS_ID } from './Components'
import { ReadOnlyColor4 } from './math'

/**
 * @public
 */
export class Wearable {
  constructor(
    public category: string,
    public contentName: string,
    public contents: { file: string; hash: string }[] = []
  ) {}
}

/**
 * @public
 */
export class Skin {
  constructor(public color: ReadOnlyColor4) {}
}

/**
 * @public
 */
export class Hair {
  constructor(public color: ReadOnlyColor4) {}
}

/**
 * @public
 */
export class Face {
  constructor(public texture: string) {}
}

/**
 * @public
 */
export class Eyes {
  constructor(public texture: string, public mask?: string, public color?: ReadOnlyColor4) {}
}

/**
 * @public
 */
@Component('engine.avatarShape', CLASS_ID.AVATAR_SHAPE)
export class AvatarShape extends ObservableComponent {
  @ObservableComponent.field
  id!: string

  @ObservableComponent.field
  baseUrl!: string

  @ObservableComponent.field
  name!: string

  @ObservableComponent.field
  bodyShape!: Wearable

  @ObservableComponent.field
  wearables!: Wearable[]

  @ObservableComponent.field
  skin!: Skin

  @ObservableComponent.field
  hair!: Hair

  @ObservableComponent.field
  eyes!: Eyes

  @ObservableComponent.field
  eyebrows!: Face

  @ObservableComponent.field
  mouth!: Face

  @ObservableComponent.field
  useDummyModel: boolean = false

  public static Dummy(): AvatarShape {
    const avatarShape = new AvatarShape()
    avatarShape.useDummyModel = true
    return avatarShape
  }
}
