import {select, take} from "redux-saga/effects";
import {RootProfileState} from "../types";
import {getProfileHash} from "../selectors";
import {PROFILE_HASH_SUCCESS} from "../actions";

export function* waitForProfileHash(userId: string) {
  while (!(yield select((state: RootProfileState) => getProfileHash(state, userId)))) {
    yield take(PROFILE_HASH_SUCCESS)
  }
  return yield select(getProfileHash, userId)
}
