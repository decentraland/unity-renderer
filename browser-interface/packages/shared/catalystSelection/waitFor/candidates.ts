import { waitFor } from 'lib/redux/waitFor'
import { getCatalystCandidates, getCatalystCandidatesReceived } from '../selectors'
import { SET_CATALYST_CANDIDATES } from '../actions'

export const waitForCatalystCandidates = waitFor(getCatalystCandidates, SET_CATALYST_CANDIDATES)
export const waitForCatalystCandidatesReceived = waitFor(getCatalystCandidatesReceived, SET_CATALYST_CANDIDATES)
