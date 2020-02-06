import { AtlasState } from '../atlas/types'
import { PassportState } from '../passports/types'
import { DaoState } from '../dao/types'

export type RootState = {
  atlas: AtlasState
  passports: PassportState
  dao: DaoState
}
