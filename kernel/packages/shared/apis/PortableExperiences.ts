import { registerAPI, exposeMethod } from 'decentraland-rpc/lib/host'
import {
  spawnPortableExperienceScene,
  getPortableExperience,
  PortableExperienceHandle,
  killPortableExperienceScene
} from 'unity-interface/portableExperiencesUtils'
import { ExposableAPI } from './ExposableAPI'
import { ParcelIdentity } from './ParcelIdentity'

type PortableExperienceUrn = string

@registerAPI('PortableExperiences')
export class PortableExperiences extends ExposableAPI {
  /**
   * Starts a portable experience.
   * @param  {SpawnPortableExperienceParameters} [pid] - Information to identify the PE
   *
   * Returns the handle of the portable experience.
   */
  @exposeMethod
  async spawn(pid: PortableExperienceUrn): Promise<PortableExperienceHandle> {
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)
    return await spawnPortableExperienceScene(pid, parcelIdentity.cid)
  }

  /**
   * Stops a portable experience. Only the executor that spawned the portable experience has permission to kill it.
   * @param  {string} [pid] - The portable experience process id
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  @exposeMethod
  async kill(pid: PortableExperienceUrn): Promise<boolean> {
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)
    const portableExperience: PortableExperienceHandle | undefined = await getPortableExperience(pid)

    if (!!portableExperience && portableExperience.parentCid == parcelIdentity.cid) {
      return await killPortableExperienceScene(pid)
    }
    return false
  }

  /**
   * Stops a portable experience from the current running portable scene.
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  @exposeMethod
  async exit(): Promise<boolean> {
    const parcelIdentity: ParcelIdentity = this.options.getAPIInstance(ParcelIdentity)

    return await killPortableExperienceScene(parcelIdentity.cid)
  }
}
