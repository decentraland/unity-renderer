import {RemoteProfileWithHash} from "./types";
import {hashV1} from "@dcl/hashing";
import {Avatar} from "@dcl/schemas";
import {recoverAddressFromEthSignature} from "@dcl/crypto/dist/crypto";

export async function isImpostor(profile: RemoteProfileWithHash, signer: string | undefined): Promise<boolean> {
  let checksum = await getProfileChecksum(profile.profile.avatars[0]);
  return checksum != profile.hash || getSigner(profile) != signer
}

async function getProfileChecksum(avatar: Avatar) {
  const encoder = new TextEncoder()
  const payload = JSON.stringify([avatar.name, avatar.hasClaimedName, ...avatar.avatar.wearables])
  return await hashV1(encoder.encode(payload));
}

function getSigner(profile: RemoteProfileWithHash): string {
  return recoverAddressFromEthSignature(profile.signedHash, profile.hash)
}
