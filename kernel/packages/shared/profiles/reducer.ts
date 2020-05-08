import { AnyAction } from 'redux'
import { ProfileState, INITIAL_PROFILES } from './types'
import {
  ADDED_PROFILE_TO_CATALOG,
  ADD_CATALOG,
  AddCatalogAction,
  CATALOG_LOADED,
  CatalogLoadedAction,
  INVENTORY_SUCCESS,
  InventorySuccess,
  INVENTORY_FAILURE,
  InventoryRequest,
  InventoryFailure,
  INVENTORY_REQUEST,
  PROFILE_SUCCESS,
  PROFILE_FAILURE,
  PROFILE_REQUEST
} from './actions'

export function profileReducer(state?: ProfileState, action?: AnyAction): ProfileState {
  if (!state) {
    return INITIAL_PROFILES
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
    case PROFILE_REQUEST:
      return {
        ...state,
        userInfo: { ...state.userInfo, [action.payload.userId]: { status: 'loading' } }
      }
    case PROFILE_SUCCESS:
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId]: {
            data: action.payload.profile,
            status: 'ok',
            hasConnectedWeb3: action.payload.hasConnectedWeb3
          }
        }
      }
    case PROFILE_FAILURE:
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId]: { status: 'error', data: action.payload.error }
        }
      }
    case ADDED_PROFILE_TO_CATALOG:
      return {
        ...state,
        userInfo: {
          ...state.userInfo,
          [action.payload.userId]: {
            ...state.userInfo[action.payload.userId],
            addedToCatalog: true
          }
        }
      }
    default:
      return state
  }
}
