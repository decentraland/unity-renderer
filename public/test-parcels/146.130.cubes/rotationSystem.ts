import { ISystem, engine, Transform } from 'decentraland-ecs/src'
import { RotationComponent } from "./rotationComponent";

const rotEntities = engine.getComponentGroup(RotationComponent);

export class RotationSystem implements ISystem{
    update(dt : number){        
        for (let e of rotEntities.entities){
            let trans = e.get(Transform);
            let rotData = e.get(RotationComponent);
            trans.rotate(rotData.rotAxis.clone(), rotData.rotSpd * dt); //rotate resets vector
        }
    }
}