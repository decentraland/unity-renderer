import { Observable } from '../../decentraland-ecs/src/ecs/Observable'
import future, { IFuture } from 'fp-future'

let worldRunning: boolean = false

export const worldRunningObservable = new Observable<Readonly<boolean>>()

worldRunningObservable.add((state) => {
  worldRunning = state
})

export function isWorldRunning(): boolean {
  return worldRunning
}

export async function ensureWorldRunning() {
  const result: IFuture<void> = future()

  if (isWorldRunning()) {
    result.resolve()
    return result
  }

  onNextWorldRunning(() => result.resolve())

  return result
}

export function onNextWorldRunning(callback: Function) {
  const observer = worldRunningObservable.add((isRunning) => {
    if (isRunning) {
      worldRunningObservable.remove(observer)
      callback()
    }
  })
}
