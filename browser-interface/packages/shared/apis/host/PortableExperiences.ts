import {
  spawnScenePortableExperienceSceneFromUrn,
  getPortableExperiencesLoaded,
  getRunningPortableExperience
} from 'unity-interface/portableExperiencesUtils'
import { store } from 'shared/store/isolatedStore'
import { removeScenePortableExperience } from 'shared/portableExperiences/actions'

import type { RpcServerPort } from '@dcl/rpc'
import type { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import { PortableExperiencesServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/portable_experiences.gen'

export function registerPortableExperiencesServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, PortableExperiencesServiceDefinition, async () => ({
    /**
     * Starts a portable experience.
     * @param  {SpawnPortableExperienceParameters} [pid] - Information to identify the PE
     *
     * Returns the handle of the portable experience.
     */
    async spawn(req, ctx) {
      return await spawnScenePortableExperienceSceneFromUrn(req.pid, ctx.sceneData.id)
    },

    /**
     * Stops a portable experience. Only the executor that spawned the portable experience has permission to kill it.
     *
     * Returns true if was able to kill the portable experience, false if not.
     */
    async kill(req, ctx) {
      const portableExperience = getRunningPortableExperience(req.pid)

      if (!!portableExperience && portableExperience.loadableScene.parentCid === ctx.sceneData.id) {
        store.dispatch(removeScenePortableExperience(req.pid))
        return { status: true }
      }
      return { status: false }
    },

    /**
     * Stops a portable experience from the current running portable scene.
     *
     * Returns true if was able to kill the portable experience, false if not.
     */
    async exit(_req, ctx) {
      store.dispatch(removeScenePortableExperience(ctx.sceneData.id))
      return { status: true }
    },

    /**
     *
     * Returns current portable experiences loaded with ids and parentCid
     */
    async getPortableExperiencesLoaded() {
      const loaded = getPortableExperiencesLoaded()
      return {
        loaded: Array.from(loaded).map(($) => ({ pid: $.loadableScene.id, parentCid: $.loadableScene.parentCid || '' }))
      }
    }
  }))
}
