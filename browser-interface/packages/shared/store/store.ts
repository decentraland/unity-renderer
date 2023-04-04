import { AnyAction, applyMiddleware, compose, createStore, Middleware, StoreEnhancer } from 'redux'
// eslint-disable-next-line @typescript-eslint/no-var-requires
const createSagaMiddleware = require('@redux-saga/core').default
import { createLogger } from 'redux-logger'
import { reducers } from './rootReducer'
import { createRootSaga } from './rootSaga'
import { DEBUG_REDUX } from 'config'
import { ErrorContext, BringDownClientAndReportFatalError } from 'shared/loading/ReportFatalError'
import defaultLogger from 'lib/logger'
import { setStore } from './isolatedStore'
import { composeWithDevTools } from '@redux-devtools/extension'
import { logTrace } from 'unity-interface/trace'

export const sagaMiddleware = createSagaMiddleware({
  sagaMonitor: undefined,
  onError: (error: Error, { sagaStack }: { sagaStack: string }) => {
    defaultLogger.log('SAGA-ERROR: ', error)
    BringDownClientAndReportFatalError(error, ErrorContext.KERNEL_SAGA, { sagaStack })
  }
})

export const buildStore = (enhancer?: StoreEnhancer<any>) => {
  const middlewares: Middleware[] = [sagaMiddleware]

  middlewares.push((_store) => (next) => (action: AnyAction) => {
    logTrace(action.type, action.payload, 'KK')
    return next(action)
  })

  const composeEnhancers =
    (DEBUG_REDUX &&
      ((window as any).__REDUX_DEVTOOLS_EXTENSION_COMPOSE__ || composeWithDevTools({ trace: true, traceLimit: 25 }))) ||
    compose

  const enhancers: StoreEnhancer<any>[] = [applyMiddleware(...middlewares)]

  if (DEBUG_REDUX) {
    enhancers.unshift(
      applyMiddleware(
        createLogger({
          collapsed: true,
          stateTransformer: () => null
        })
      )
    )
  }

  if (enhancer) {
    enhancers.push(enhancer)
  }

  const store = createStore(reducers, composeEnhancers(...enhancers))
  const startSagas = () => sagaMiddleware.run(createRootSaga())
  setStore(store)
  return { store, startSagas }
}
