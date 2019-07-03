import { engine, Component, ISystem, IEntity,  Vector3, Transform, Entity, log, Camera } from 'decentraland-ecs/src'

import { environments, Environment } from "./Environment";
import { Creature } from "./Creature";
import { grabbedObject, SetGrabbedObject } from './Params';

@Component('grabableObjectComponent')
export class GrabableObjectComponent {
  grabbed: boolean = false
  constructor(
    grabbed: boolean = false,
  ) {
    this.grabbed = grabbed
  }
}

@Component('objectGrabberComponent')
export class ObjectGrabberComponent {
}

let grabbedOffset = new Vector3(0.5, 1, 0)

// workaround since `setParent(null)` won't work
let dummyPosParent = new Entity()
engine.addEntity(dummyPosParent)

// object to get user position and rotation
const camera = Camera.instance

// start object grabber system
let objectGrabber = new Entity()
objectGrabber.addComponent(
  new Transform({
    position: camera.position.clone(),
    rotation: camera.rotation.clone()
  })
)
objectGrabber.addComponent(new ObjectGrabberComponent())
engine.addEntity(objectGrabber)

export const grabbableObjects = engine.getComponentGroup(
  GrabableObjectComponent
)

export class ObjectGrabberSystem implements ISystem {
  update(deltaTime: number) {
	  if (grabbedObject == null) {
		  //log("no children")
		  return
		}

	  let transform = objectGrabber.getComponent(Transform)
	  transform.position = camera.position.clone()
	  transform.rotation = camera.rotation.clone()

  }
}

export function grabObject(newGrabbedObject: IEntity) {

    if (!objectGrabber.children[0]) {
      log('grabbed object')

      newGrabbedObject.getComponent(GrabableObjectComponent).grabbed = true
      newGrabbedObject.setParent(objectGrabber)
      newGrabbedObject.getComponent(Transform).position = grabbedOffset.clone()
      newGrabbedObject.getComponent(Creature).SetEnvironment(null)

	    SetGrabbedObject(newGrabbedObject)
    } else {
      log('already holding')
    }
  }

export function dropObject(environment: Environment | null = null) {
    if(!grabbedObject) return

    environment = environment? environment : getClosestArea(Camera.instance.position)!.getComponent(Environment)

    if (environment) {
		// workaround ... parent should be null
		grabbedObject.setParent(dummyPosParent)

		grabbedObject.getComponent(Transform).position = environment.position
		grabbedObject.getComponent(GrabableObjectComponent).grabbed = false
    grabbedObject.getComponent(Creature).SetEnvironment(environment)
    grabbedObject.getComponent(Creature).TargetRandomPosition()

		SetGrabbedObject(null)

	} else {
      log('not possible to drop here')
    }
  }


export function getClosestArea(playerPos: Vector3){
	for (let environment of environments.entities) {
		let dist = Vector3.DistanceSquared(environment.getComponent(Transform).position, playerPos)
		if (dist < 25){
			return environment
		}
	}
	return null
  }
