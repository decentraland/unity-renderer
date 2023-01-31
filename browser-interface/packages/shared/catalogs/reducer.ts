import { AnyAction } from 'redux'
import { CatalogState, PartialWearableV2, WearableId } from './types'
import { CATALOG_LOADED, CatalogLoadedAction } from './actions'

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
    case CATALOG_LOADED:
      const { payload } = action as CatalogLoadedAction
      const { name, catalog } = payload
      const catalogObject: Record<WearableId, PartialWearableV2> = {}
      catalog.forEach((wearable) => (catalogObject[wearable.id] = wearable))
      return {
        ...state,
        catalogs: {
          ...state.catalogs,
          [name]: {
            id: name,
            data: catalogObject,
            status: 'ok'
          }
        }
      }
    default:
      return state
  }
}
