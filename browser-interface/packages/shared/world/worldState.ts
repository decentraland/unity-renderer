import { Observable } from 'mz-observable'

let hidden: 'hidden' | 'msHidden' | 'webkitHidden' = 'hidden'
let visibilityChange: 'visibilitychange' | 'msvisibilitychange' | 'webkitvisibilitychange' = 'visibilitychange'

if (typeof (document as any).hidden !== 'undefined') {
  // Opera 12.10 and Firefox 18 and later support
  hidden = 'hidden'
  visibilityChange = 'visibilitychange'
} else if (typeof (document as any).msHidden !== 'undefined') {
  hidden = 'msHidden'
  visibilityChange = 'msvisibilitychange'
} else if (typeof (document as any).webkitHidden !== 'undefined') {
  hidden = 'webkitHidden'
  visibilityChange = 'webkitvisibilitychange'
}

export const foregroundChangeObservable = new Observable<void>()

function handleVisibilityChange() {
  foregroundChangeObservable.notifyObservers()
}

if (hidden && visibilityChange) {
  document.addEventListener(visibilityChange, handleVisibilityChange, false)
}

export function isForeground(): boolean {
  return !(document as any)[hidden]
}
