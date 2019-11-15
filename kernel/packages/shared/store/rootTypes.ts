import { AtlasState } from '../atlas/types'
import { AuthState } from '../auth/types'
import { PassportState } from '../passports/types'

export type RootState = {
  atlas: AtlasState
  auth: AuthState
  passports: PassportState
}
