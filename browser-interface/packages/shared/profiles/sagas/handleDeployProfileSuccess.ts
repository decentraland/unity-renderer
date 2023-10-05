import {DeployProfileSuccess} from "../actions";
import {fetchProfileHashes} from "./content/fetchProfileHashes";
import {call} from "redux-saga/effects";

export function* handleDeployProfileSuccess(deployProfileAction: DeployProfileSuccess) {
  // every time the profile is deployed the hashes changes so it needs to be updated
  yield call(fetchProfileHashes, deployProfileAction.payload.userId, deployProfileAction.payload.version)
}
