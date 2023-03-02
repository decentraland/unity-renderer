import { expect } from 'chai'
import { buildStore } from '../../packages/shared/store/store'
import { getDisabledCatalystConfig, getFeatureFlags } from 'shared/meta/selectors'

describe('Meta tests', () => {
  describe('Parse feature flags', () => {
    it.skip('enable feature flags', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      location.search = '&ENABLE_FEATURE1='

      const features = getFeatureFlags(store.getState())

      expect(features.flags['feature1']).to.equal(true)
    })

    it.skip('disable feature flags', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      location.search = '&DISABLE_FEATURE1='

      const features = getFeatureFlags(store.getState())

      expect(features.flags['feature1']).to.equal(false)
    })

    it.skip('parse multiple feature flags', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      location.search = '&DISABLE_ASSET_BUNDLES=&DISABLE_WEARABLES_ASSET_BUNDLES&ENABLE_FEATURE1=&ENABLE_FEATURE2'

      const features = getFeatureFlags(store.getState())

      expect(features.flags['asset_bundles']).to.equal(false)
      expect(features.flags['wearables_asset_bundles']).to.equal(false)
      expect(features.flags['feature1']).to.equal(true)
      expect(features.flags['feature2']).to.equal(true)
    })

    it.skip('override featureflag', () => {
      const { store } = buildStore()
      globalThis.globalStore = store

      location.search = '&ENABLE_FEATURE1=&DISABLE_FEATURE1'

      const features = getFeatureFlags(store.getState())

      expect(features.flags['feature1']).to.equal(false)
    })

    it('get catalyst denied featureFlag', () => {
      const { store } = buildStore()
      store.getState().meta.config.featureFlagsV2 = {
        flags: {},
        variants: {
          ['disabled-catalyst']: {
            name: 'disabledCatalyst',
            enabled: true,
            payload: {
              type: 'json',
              value: JSON.stringify(['invlaid', 'https://casla.boedo'])
            }
          }
        }
      }
      globalThis.globalStore = store

      const disabledCatalyst = getDisabledCatalystConfig(store.getState())

      expect(disabledCatalyst).to.eql(['https://casla.boedo'])
    })
  })
})
