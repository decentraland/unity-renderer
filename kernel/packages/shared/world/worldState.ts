import { Observable } from '../../decentraland-ecs/src/ecs/Observable'

let worldRunning: boolean = false

export const worldRunningObservable = new Observable<Readonly<boolean>>()

worldRunningObservable.add(state => {
  worldRunning = state
})

export function isWorldRunning(): boolean {
  return worldRunning
}
