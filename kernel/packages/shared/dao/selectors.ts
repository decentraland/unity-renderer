import { RootDaoState } from './types'

export const getFetchProfileServer = (store: RootDaoState) => store.dao.profileServer
export const getUpdateProfileServer = (store: RootDaoState) => store.dao.updateContentServer

export const getFetchContentServer = (store: RootDaoState) => store.dao.fetchContentServer

export const getCommsServer = (store: RootDaoState) => store.dao.commsServer

export const getLayer = (store: RootDaoState) => store.dao.layer

export const isRealmInitialized = (store: RootDaoState) => store.dao.initialized
