import { Component, ObservableComponent } from '../ecs/Component'
import { IEvents } from './Types'
import { uuidEventSystem } from './Systems'
import { CLASS_ID, OnUUIDEvent, OnPointerUUIDEvent } from './Components'
import { ActionButton } from './Input'

/**
 * @public
 */
@Component('engine.onFocus', CLASS_ID.UUID_CALLBACK)
export class OnFocus extends OnUUIDEvent<'onFocus'> {
  @ObservableComponent.readonly
  readonly type: string = 'onFocus'
  constructor(callback: (event: IEvents['onFocus']) => void) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this
  }
}

/**
 * @public
 */
@Component('engine.onTextSubmit', CLASS_ID.UUID_CALLBACK)
export class OnTextSubmit extends OnUUIDEvent<'onTextSubmit'> {
  @ObservableComponent.readonly
  readonly type: string = 'onTextSubmit'
  constructor(callback: (event: IEvents['onTextSubmit']) => void) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this
  }
}

/**
 * @public
 */
@Component('engine.onBlur', CLASS_ID.UUID_CALLBACK)
export class OnBlur extends OnUUIDEvent<'onBlur'> {
  @ObservableComponent.readonly
  readonly type: string = 'onBlur'
  constructor(callback: (event: IEvents['onBlur']) => void) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this
  }
}

/**
 * @public
 */
@Component('engine.onEnter', CLASS_ID.UUID_CALLBACK)
export class OnEnter extends OnUUIDEvent<'onEnter'> {
  @ObservableComponent.readonly
  readonly type: string = 'onEnter'
  constructor(callback: (event: IEvents['onEnter']) => void) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this
  }
}

/**
 * @public
 */
@Component('engine.onChange', CLASS_ID.UUID_CALLBACK)
export class OnChanged extends OnUUIDEvent<'onChange'> {
  @ObservableComponent.readonly
  readonly type: string = 'onChange'
  constructor(callback: (event: IEvents['onChange']) => void) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this
  }
}

/**
 * @public
 */
export type OnPointerUUIDEventOptions = {
  button?: ActionButton
  hoverText?: string
  showFeedback?: boolean
  distance?: number
}

/**
 * @public
 */
@Component('engine.onClick', CLASS_ID.UUID_CALLBACK)
export class OnClick extends OnPointerUUIDEvent<'onClick'> {
  @ObservableComponent.readonly
  readonly type: string = 'onClick'

  constructor(callback: (event: IEvents['onClick']) => void)
  constructor(callback: (event: IEvents['onClick']) => void, options: OnPointerUUIDEventOptions)
  constructor(callback: (event: IEvents['onClick']) => void, options?: any) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this

    if (options) {
      this.showFeedback = !(options.showFeedback === false)

      if (options.button) {
        this.button = options.button
      }

      if (options.hoverText) {
        this.hoverText = options.hoverText
      }

      if (options.distance) {
        this.distance = options.distance
      }
    }
  }
}

/**
 * @public
 */
@Component('engine.pointerDown', CLASS_ID.UUID_CALLBACK)
export class OnPointerDown extends OnPointerUUIDEvent<'pointerDown'> {
  @ObservableComponent.readonly
  readonly type: string = 'pointerDown'

  constructor(callback: (event: IEvents['pointerDown']) => void)
  constructor(callback: (event: IEvents['pointerDown']) => void, options: OnPointerUUIDEventOptions)
  constructor(callback: (event: IEvents['pointerDown']) => void, options?: any) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this

    if (options) {
      this.showFeedback = !(options.showFeedback === false)

      if (options.button) {
        this.button = options.button
      }

      if (options.hoverText) {
        this.hoverText = options.hoverText
      }

      if (options.distance) {
        this.distance = options.distance
      }
    }
  }
}

/**
 * @public
 */
@Component('engine.pointerUp', CLASS_ID.UUID_CALLBACK)
export class OnPointerUp extends OnPointerUUIDEvent<'pointerUp'> {
  @ObservableComponent.readonly
  readonly type: string = 'pointerUp'

  constructor(callback: (event: IEvents['pointerUp']) => void)
  constructor(callback: (event: IEvents['pointerUp']) => void, options: OnPointerUUIDEventOptions)
  constructor(callback: (event: IEvents['pointerUp']) => void, options?: any) {
    super(callback)
    // This injection is necessary ONLY in events that are ALWAYS turned on and are
    // not assignable to entities. Like events for the UI elements

    // TODO(Brian): This will be removed when UI gets back to the entity parenting.
    uuidEventSystem.handlerMap[this.uuid] = this

    if (options) {
      this.showFeedback = !(options.showFeedback === false)

      if (options.button) {
        this.button = options.button
      }

      if (options.hoverText) {
        this.hoverText = options.hoverText
      }

      if (options.distance) {
        this.distance = options.distance
      }
    }
  }
}
