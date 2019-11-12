import { call, fork, put } from 'redux-saga/effects'
import { signalRendererInitialized } from './actions'

export function* rendererSaga() {
  yield fork(awaitRendererInitialSignal)
}

export function* awaitRendererInitialSignal(): any {
  yield call(waitForRenderer)
  yield put(signalRendererInitialized())
}

export async function waitForRenderer() {
  return new Promise(resolve => {
    const interval = setInterval(() => {
      if ((window as any)['unityInterface'] && (window as any)['unityInterface'].SendMessage) {
        clearInterval(interval)
        resolve()
      }
    }, 1000)
  })
}
