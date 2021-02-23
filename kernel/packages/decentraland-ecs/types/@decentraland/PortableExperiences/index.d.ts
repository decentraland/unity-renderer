declare module '@decentraland/PortableExperiences' {
  type PortableExperienceUrn = string
  type PortableExperienceHandle = {
    pid: PortableExperienceUrn
    parentCid: string // Identifier of who triggered the PE to allow to kill it only to who created it
  }

  /**
   * Starts a portable experience.
   * @param  {SpawnPortableExperienceParameters} [pid] - Information to identify the PE
   *
   * Returns the handle of the portable experience.
   */
  export function spawn(pid: PortableExperienceUrn): Promise<PortableExperienceHandle>

  /**
   * Stops a portable experience. Only the executor that spawned the portable experience has permission to kill it.
   * @param  {string} [pid] - The portable experience process id
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  export function kill(pid: PortableExperienceUrn): Promise<boolean>

  /**
   * Stops a portable experience from the current running portable scene.
   *
   * Returns true if was able to kill the portable experience, false if not.
   */
  export function exit(): Promise<boolean>
}
