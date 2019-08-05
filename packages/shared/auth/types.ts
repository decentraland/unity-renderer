export enum Event {
  USER_TOKEN = 'DECENTRALAND_USER_TOKEN',
  LOGOUT = 'DECENTRALAND_LOGOUT',
  ERROR = 'DECENTRALAND_ERROR'
}

export enum LoginType {
  IFRAME = 'IFRAME',
  POPUP = 'POPUP'
}

export type LogoutMessage = {
  type: Event.LOGOUT
  from: LoginType
}

export type UserTokenMessage = {
  type: Event.USER_TOKEN
  from: LoginType
  token: string
}

export type ErrorMessage = {
  type: Event.ERROR
  from: LoginType
  error: {
    error: string
    errorDescription: string
    state: string
  }
}

export type Message = LogoutMessage | UserTokenMessage | ErrorMessage
