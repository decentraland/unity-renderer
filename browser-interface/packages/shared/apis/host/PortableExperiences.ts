import {
  spawnScenePortableExperienceSceneFromUrn,
  getPortableExperiencesLoaded,
  getRunningPortableExperience,
  spawnPortableExperienceFromEns,
  ensPxMapping
} from 'unity-interface/portableExperiencesUtils'
import { store } from 'shared/store/isolatedStore'
import { removeScenePortableExperience } from 'shared/portableExperiences/actions'

import type { RpcServerPort } from '@dcl/rpc'
import type { PortContext } from './context'
import * as codegen from '@dcl/rpc/dist/codegen'

import { PortableExperiencesServiceDefinition } from 'shared/protocol/decentraland/kernel/apis/portable_experiences.gen'
import { isDclEns } from '../../realm/resolver'

export function registerPortableExperiencesServiceServerImplementation(port: RpcServerPort<PortContext>) {
  codegen.registerService(port, PortableExperiencesServiceDefinition, async () => ({
    /**
     * Starts a portable experience.
     * Returns the handle of the portable experience.
     */
    async spawn(req, ctx) {
      if (!req.pid && !req.ens) throw new Error('Invalid Spawn params. Provide a URN or an ENS name.')

      // Load via URN.
      if (req.pid) {
        const px = await spawnScenePortableExperienceSceneFromUrn(req.pid, ctx.sceneData.id)
        return px
      }
      // Load via Worlds ENS url
      if (!isDclEns(req.ens)) throw new Error('Invalid ens name')
      const px = await spawnPortableExperienceFromEns(req.ens, ctx.sceneData.id)
      return { ...px, ens: req.ens }
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
        loaded: Array.from(loaded).map(($) => ({
          pid: $.loadableScene.id,
          parentCid: $.loadableScene.parentCid || '',
          ens: ensPxMapping.get($.loadableScene.id) ?? '',
          name: $.metadata.display?.title ?? ''
        }))
      }
    }
  }))
}
