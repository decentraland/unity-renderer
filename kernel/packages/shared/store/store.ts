import { applyMiddleware, compose, createStore, Store } from 'redux'
import createSagaMiddleware from 'redux-saga'
import { reducers } from './rootReducer'
import { createRootSaga } from './rootSaga'
import { RootState } from './rootTypes'
import { DEBUG_REDUX } from '../../config'
import { ReportFatalError } from '../loading/ReportFatalError'
import defaultLogger from '../logger'

export let store: Store<RootState>

export const buildStore = () => {
  const sagaMiddleware = createSagaMiddleware({
    onError: (error: Error, { sagaStack }: { sagaStack: string }) => {
      defaultLogger.log('SAGA-ERROR: ', error)
      ReportFatalError(error.message as any, {
        type: 'saga',
        message: error.message,
        stack: error.stack,
        sagaStack
      })
    }
  })
  const composeEnhancers = (DEBUG_REDUX && (window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__) || compose
  store = createStore(reducers, composeEnhancers(applyMiddleware(sagaMiddleware)))
  const startSagas = () => sagaMiddleware.run(createRootSaga())
  return { store, startSagas }
}
