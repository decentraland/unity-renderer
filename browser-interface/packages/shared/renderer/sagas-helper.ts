import { select, take } from 'redux-saga/effects'

import { isRendererInitialized } from './selectors'
import { RENDERER_INITIALIZED_CORRECTLY } from './types'

export function* waitForRendererInstance() {
  while (!(yield select(isRendererInitialized))) {
    yield take(RENDERER_INITIALIZED_CORRECTLY)
  }
}
