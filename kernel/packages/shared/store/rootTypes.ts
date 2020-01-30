import { AtlasState } from '../atlas/types'
import { PassportState } from '../passports/types'

export type RootState = {
  atlas: AtlasState
  passports: PassportState
}
