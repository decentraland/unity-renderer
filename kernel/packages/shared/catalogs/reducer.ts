import { AnyAction } from 'redux'
import { CatalogState } from './types'
import {
  ADD_CATALOG,
  AddCatalogAction,
  CATALOG_LOADED,
  CatalogLoadedAction
} from './actions'

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
    case ADD_CATALOG:
      let catalogAction = action as AddCatalogAction
      return {
        ...state,
        catalogs: {
          ...state.catalogs,
          [catalogAction.payload.name]: {
            status: 'loading',
            id: catalogAction.payload.name,
            data: catalogAction.payload.catalog
          }
        }
      }
    case CATALOG_LOADED:
      let loadCatalog = action as CatalogLoadedAction
      return {
        ...state,
        catalogs: {
          ...state.catalogs,
          [loadCatalog.payload.name]: {
            ...state.catalogs[loadCatalog.payload.name],
            status: 'ok'
          }
        }
      }
    default:
      return state
  }
}
