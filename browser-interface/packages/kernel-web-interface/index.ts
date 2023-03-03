// This interface is the anti corruption layer between kernel and website

import { AuthIdentity } from './dcl-crypto'
import { PersistentAsyncStorage } from './storage'

export { AuthIdentity, PersistentAsyncStorage }

/**
 * @public
 */
export type IEthereumProvider = { sendAsync: any } | { request: any }

/**
 * @public
 */
export interface KernelTrackingEvent {
  eventName: string
  eventData: Record<string, any>
}

/**
 * @public
 */
export interface KernelError {
  error: Error
  code?: string
  level?: 'critical' | 'fatal' | 'serious' | 'warning'
  extra?: Record<string, any>
}

/**
 * @public
 */
export interface KernelLoadingProgress {
  progress: number
  status?: number
}

/**
 * @public
 */
export enum LoginState {
  /**
   * Program not ready.
   */
  LOADING = 'LOADING',
  /**
   * Ready to authenticate
   */
  WAITING_PROVIDER = 'WAITING_PROVIDER',
  WAITING_RENDERER = 'WAITING_RENDERER',
  /**
   * Authenticating
   */
  AUTHENTICATING = 'AUTHENTICATING',
  SIGNATURE_PENDING = 'SIGNATURE_PENDING',
  SIGNATURE_FAILED = 'SIGNATURE_FAILED',
  /**
   * Creating avatar. Before signing ToS
   */
  SIGN_UP = 'SIGN_UP',
  WAITING_PROFILE = 'WAITING_PROFILE',
  COMPLETED = 'COMPLETED'
}

/**
 * @public
 */
export type DecentralandIdentity = AuthIdentity & {
  address: string // contains the lowercased address that will be used for the userId
  rawAddress: string // contains the real ethereum address of the current user
  provider?: any
  hasConnectedWeb3: boolean
}

/**
 * @public
 */
export interface KernelAccountState {
  loginStatus: LoginState
  network?: string
  identity?: DecentralandIdentity
  hasProvider: boolean
  isGuest?: boolean
}

/**
 * @public
 */
export interface KernelSignUpEvent {
  email: string
}

/**
 * @public
 */
export interface KernelOpenUrlEvent {
  url: string
}

/**
 * @public
 */
export interface KernelRendererVisibleEvent {
  visible: boolean
}

/**
 * @public
 */
export type KernelOptions = {
  kernelOptions: {
    baseUrl?: string
    previewMode?: boolean
    configurations?: Record<string, string>
    persistentStorage?: PersistentAsyncStorage
  }
  rendererOptions: {
    container: any
    baseUrl?: string
  }
}

/**
 * @public
 */
export type KernelLogoutEvent = any

/**
 * @public
 *
 * This event is triggered after the kernel shuts down for any reason
 */
export type KernelShutdownEvent = any

/**
 * @public
 */
export type NamedEvents = {
  signUp: KernelSignUpEvent
  accountState: KernelAccountState
  loadingProgress: KernelLoadingProgress
  error: KernelError
  trackingEvent: KernelTrackingEvent
  rendererVisible: KernelRendererVisibleEvent
  openUrl: KernelOpenUrlEvent
  logout: KernelLogoutEvent
  shutdown: KernelShutdownEvent
}

/**
 * @public
 */
export type KernelResult = {
  on<K extends keyof NamedEvents>(eventName: K, cb: (event: NamedEvents[K]) => void): void
  on(eventName: string, cb: (event: Record<string, any>) => void): void
  authenticate(provider: IEthereumProvider, isGuest: boolean): void
  version: string
  /**
   * This method is used to know if the kernel has a stored session for
   * a specific address it is mainly used to perform autologin.
   */
  hasStoredSession(address: string, networkId: number): Promise<{ result: boolean; profile?: any }>
}

/**
 * @public
 */
export interface IDecentralandKernel {
  initKernel(options: KernelOptions): Promise<KernelResult>
}
