import {call, select} from "redux-saga/effects";
import {fetchCatalystProfile} from "./fetchCatalystProfile";
import {getProfileHash} from "../../selectors";

export function* fetchProfileHashes(userId: string, version?: number) {
  yield call(fetchCatalystProfile, userId, version);
  return (yield select(getProfileHash, userId)) as ReturnType<typeof getProfileHash>;
}
