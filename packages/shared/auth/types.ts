export type AuthState = {
  loading: string[]
  data: AuthData | null
  error: string | null
}

export type AuthData = {
  email: string
  sub: string
  idToken: string
  accessToken: string
  expiresAt: number
}
