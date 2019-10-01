import { AnyAction } from 'redux'
import { PassportState, INITIAL_PASSPORTS } from './types'
import {
  SET_PROFILE_SERVER,
  ADD_CATALOG,
  AddCatalogAction,
  CATALOG_LOADED,
  CatalogLoadedAction,
  INVENTORY_SUCCESS,
  InventorySuccess,
  INVENTORY_FAILURE,
  InventoryRequest,
  InventoryFailure,
  INVENTORY_REQUEST
} from './actions'
export function passportsReducer(state?: PassportState, action?: AnyAction): PassportState {
  if (!state) {
    return INITIAL_PASSPORTS
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
    case INVENTORY_REQUEST:
      const actionAsInventoryReq = action as InventoryRequest
      return {
        ...state,
        userInventory: {
          ...state.userInventory,
          [actionAsInventoryReq.payload.userId]: {
            status: 'loading'
          }
        }
      }
    case INVENTORY_FAILURE:
      const actionAsInventoryFailure = action as InventoryFailure
      return {
        ...state,
        userInventory: {
          ...state.userInventory,
          [actionAsInventoryFailure.payload.userId]: {
            status: 'error',
            data: actionAsInventoryFailure.payload.error
          }
        }
      }
    case INVENTORY_SUCCESS:
      const inventoryAction = action as InventorySuccess
      return {
        ...state,
        userInventory: {
          ...state.userInventory,
          [inventoryAction.payload.userId]: {
            status: 'ok',
            data: inventoryAction.payload.inventory
          }
        }
      }
    case SET_PROFILE_SERVER:
      return {
        ...state,
        profileServer: action.payload.url
      }
    case '[Request] Passport fetch':
      return {
        ...state,
        userInfo: { ...state.userInfo, [action.payload.userId]: { status: 'loading' } }
      }
    case '[Success] Passport fetch':
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId]: { data: action.payload.profile, status: 'ok' }
        }
      }
    case '[Failure] Passport fetch':
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId]: { status: 'error', data: action.payload.error }
        }
      }
    default:
      return state
  }
}
