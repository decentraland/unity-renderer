import {RemoteProfileWithHash} from "./types";
import {hashV1} from "@dcl/hashing";
import {Avatar} from "@dcl/schemas";
import {hash, recover} from "eth-crypto";

export async function isImpostor(profile: RemoteProfileWithHash, catalystPublicKey: string): Promise<boolean> {
  let profileChecksum = await getProfileChecksum(profile.profile.avatars[0]);
  let signature = recover(profile.signedHash, hash.keccak256(profile.hash));
  return profileChecksum != profile.hash || signature != catalystPublicKey
}

async function getProfileChecksum(avatar: Avatar) {
  const encoder = new TextEncoder()
  const payload = JSON.stringify([avatar.name, avatar.hasClaimedName, ...avatar.avatar.wearables])
  return await hashV1(encoder.encode(payload));
}
