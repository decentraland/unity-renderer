import { AuthState } from '../auth/types'
import { PassportState } from '../passports/types'

export type RootState = {
  auth: AuthState
  passports: PassportState
}
