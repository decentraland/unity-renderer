import { Timer } from './Timer'

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

const timers = new Set<Timer>()

function handleVisibilityChange() {
  const _timers = [...timers]
  if (isForeground()) {
    _timers.forEach($ => $.resume())
  } else {
    _timers.forEach($ => $.pause())
  }
}

if (hidden && visibilityChange) {
  document.addEventListener(visibilityChange, handleVisibilityChange, false)
}

function register(timer: Timer) {
  timers.add(timer)
}

function unregister(timer: Timer) {
  timers.delete(timer)
}

function isForeground() {
  return !(document as any)[hidden]
}

export function setForegroundTimeout(callback: Function, delay: number) {
  const timer = new Timer((self: Timer) => {
    callback()
    unregister(self)
  }, delay)
  register(timer)
  if (isForeground()) {
    timer.resume()
  }
  return timer
}

export function clearForegroundTimeout(timer: Timer) {
  timer.cancel()
  unregister(timer)
}
