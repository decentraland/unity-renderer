import { action } from 'typesafe-actions'
import { Profile, Wearable, Catalog, WearableId } from './types'
import { ProfileForRenderer } from '../../decentraland-ecs/src/decentraland/Types'

// Profile fetching

export const PROFILE_REQUEST = '[Request] Profile fetch'
export const PROFILE_SUCCESS = '[Success] Profile fetch'
export const PROFILE_FAILURE = '[Failure] Profile fetch'
export const PROFILE_RANDOM = '[?] Profile randomized'

export const profileRequest = (userId: string) => action(PROFILE_REQUEST, { userId })
export const profileSuccess = (userId: string, profile: Profile, hasConnectedWeb3: boolean = false) =>
  action(PROFILE_SUCCESS, { userId, profile, hasConnectedWeb3 })
export const profileFailure = (userId: string, error: any) => action(PROFILE_FAILURE, { userId, error })
export const profileRandom = (userId: string, profile: Profile) => action(PROFILE_RANDOM, { userId, profile })

export type ProfileRequestAction = ReturnType<typeof profileRequest>
export type ProfileSuccessAction = ReturnType<typeof profileSuccess>
export type ProfileFailureAction = ReturnType<typeof profileFailure>
export type ProfileRandomAction = ReturnType<typeof profileRandom>

// Profile update

export const SAVE_PROFILE_REQUEST = '[Request] Save Profile'
export const SAVE_PROFILE_SUCCESS = '[Success] Save Profile'
export const SAVE_PROFILE_FAILURE = '[Failure] Save Profile'

export const saveProfileRequest = (profile: Partial<Profile>, userId?: string) =>
  action(SAVE_PROFILE_REQUEST, { userId, profile })
export const saveProfileSuccess = (userId: string, version: number, profile: Profile) =>
  action(SAVE_PROFILE_SUCCESS, { userId, version, profile })
export const saveProfileFailure = (userId: string, error: any) => action(SAVE_PROFILE_FAILURE, { userId, error })

export type SaveProfileRequest = ReturnType<typeof saveProfileRequest>
export type SaveProfileSuccess = ReturnType<typeof saveProfileSuccess>
export type SaveProfileFailure = ReturnType<typeof saveProfileFailure>

// Wearables catalog

export const SET_BASE_WEARABLES_CATALOG = 'Set base wearables catalog'
export const setBaseWearablesCatalog = (wearables: Wearable[]) => action(SET_BASE_WEARABLES_CATALOG, wearables)

export const SET_BASE_EXCLUSIVES_CATALOG = 'Set base exclusives catalog'
export const setBaseExclusivesCatalog = (wearables: Wearable[]) => action(SET_BASE_EXCLUSIVES_CATALOG, wearables)

export const ADD_CATALOG = 'Add Catalog'
export const addCatalog = (name: string, catalog: Catalog) => action(ADD_CATALOG, { name, catalog })
export type AddCatalogAction = ReturnType<typeof addCatalog>
export const CATALOG_LOADED = 'Catalog Loaded'
export const catalogLoaded = (name: string) => action(CATALOG_LOADED, { name })
export type CatalogLoadedAction = ReturnType<typeof catalogLoaded>

// Inventory

export const INVENTORY_REQUEST = '[Request] Inventory fetch'
export const inventoryRequest = (userId: string, ethAddress: string) =>
  action(INVENTORY_REQUEST, { userId, ethAddress })
export type InventoryRequest = ReturnType<typeof inventoryRequest>

export const INVENTORY_SUCCESS = '[Success] Inventory fetch'
export const inventorySuccess = (userId: string, inventory: WearableId[]) =>
  action(INVENTORY_SUCCESS, { userId, inventory })
export type InventorySuccess = ReturnType<typeof inventorySuccess>

export const INVENTORY_FAILURE = '[Failure] Inventory fetch'
export const inventoryFailure = (userId: string, error: any) => action(INVENTORY_FAILURE, { userId, error })
export type InventoryFailure = ReturnType<typeof inventoryFailure>

export const NOTIFY_NEW_INVENTORY_ITEM = '[Inventory] New inventory item'
export const notifyNewInventoryItem = () => action(NOTIFY_NEW_INVENTORY_ITEM)
export type NotifyNewInventoryItem = ReturnType<typeof notifyNewInventoryItem>

export const ADDED_PROFILE_TO_CATALOG = '[Success] Added profile to catalog'
export const addedProfileToCatalog = (userId: string, profile: ProfileForRenderer) =>
  action(ADDED_PROFILE_TO_CATALOG, { userId, profile })
export type AddedProfileToCatalog = ReturnType<typeof addedProfileToCatalog>
