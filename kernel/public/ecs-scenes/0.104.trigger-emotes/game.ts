import { PredefinedEmote, triggerEmote } from '@decentraland/RestrictedActions'

// Kiss
const kiss = new Entity()
kiss.addComponent(new Transform({ position: new Vector3(5, 2, 8), rotation: new Quaternion(0, 0.5) }))
kiss.addComponent(new TextShape('Kiss'))
kiss.addComponent(new Billboard(true, true, true))
engine.addEntity(kiss)

const box1 = new Entity()
const shape1 = new BoxShape()
box1.addComponent(shape1)
box1.addComponent(new Transform({ position: new Vector3(5, 0.125, 8) }))
box1.addComponent(new OnPointerDown(() => triggerEmote({ predefined: PredefinedEmote.KISS })))
engine.addEntity(box1)

// Raise Hand
const raiseHand = new Entity()
raiseHand.addComponent(new Transform({ position: new Vector3(11, 2, 8) }))
raiseHand.addComponent(new TextShape('Raise Hand'))
raiseHand.addComponent(new Billboard(true, true, true))
engine.addEntity(raiseHand)

const box2 = new Entity()
const shape2 = new BoxShape()
box2.addComponent(shape2)
box2.addComponent(new Transform({ position: new Vector3(11, 0.125, 8) }))
box2.addComponent(new OnPointerDown(() => triggerEmote({ predefined: PredefinedEmote.RAISE_HAND })))
engine.addEntity(box2)

