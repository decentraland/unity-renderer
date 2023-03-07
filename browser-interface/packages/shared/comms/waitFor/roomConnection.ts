import { waitFor } from 'lib/redux'
import { SET_ROOM_CONNECTION } from '../actions'
import { getCommsRoom } from '../selectors'

export const waitForRoomConnection = waitFor(getCommsRoom, SET_ROOM_CONNECTION)
