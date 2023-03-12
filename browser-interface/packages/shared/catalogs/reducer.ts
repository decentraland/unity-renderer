import { AnyAction } from 'redux'
import { CatalogState } from './types'

const INITIAL_CATALOG_STATE: CatalogState = {
  catalogs: {}
}

export function catalogsReducer(state?: CatalogState, action?: AnyAction): CatalogState {
  if (!state) {
    return INITIAL_CATALOG_STATE
  }
  if (!action) {
    return state
  }
  switch (action.type) {
    default:
      return state
  }
}
