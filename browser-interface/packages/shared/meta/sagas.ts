import { FeatureFlagsResult, fetchFlags } from '@dcl/feature-flags'
import { ETHEREUM_NETWORK, getAssetBundlesBaseUrl, getServerConfigurations, PREVIEW, rootURLPreviewMode } from 'config'
import defaultLogger from 'lib/logger'
import { waitFor } from 'lib/redux'
import { all, call, put, select, take } from 'redux-saga/effects'
import { SELECT_NETWORK } from 'shared/dao/actions'
import { getSelectedNetwork } from 'shared/dao/selectors'
import { RootState } from 'shared/store/rootTypes'
import { metaConfigurationInitialized, META_CONFIGURATION_INITIALIZED } from './actions'
import { isMetaConfigurationInitialized } from './selectors'
import { FeatureFlagsName, MetaConfiguration } from './types'

export const waitForMetaConfigurationInitialization = waitFor(
  isMetaConfigurationInitialized,
  META_CONFIGURATION_INITIALIZED
)

export function* waitForNetworkSelected() {
  while (!(yield select((state: RootState) => !!state.dao.network))) {
    yield take(SELECT_NETWORK)
  }
  const net: ETHEREUM_NETWORK = yield select(getSelectedNetwork)
  return net
}

export function* metaSaga(): any {
  const net: ETHEREUM_NETWORK = yield call(waitForNetworkSelected)

  const [config, flagsAndVariants]: [Partial<MetaConfiguration>, FeatureFlagsResult] = yield all([
    call(fetchMetaConfiguration, net),
    call(fetchFeatureFlagsAndVariants, net)
  ])

  const merge: Partial<MetaConfiguration> = {
    ...config,
    featureFlagsV2: flagsAndVariants
  }

  yield put(metaConfigurationInitialized(merge))
}

async function fetchFeatureFlagsAndVariants(network: ETHEREUM_NETWORK): Promise<FeatureFlagsResult> {
  const tld = network === ETHEREUM_NETWORK.MAINNET ? 'org' : 'zone'

  const explorerFeatureFlags = PREVIEW
    ? `${rootURLPreviewMode()}/feature-flags/`
    : `https://feature-flags.decentraland.${tld}`

  const flagsAndVariants = await fetchFlags({ applicationName: 'explorer', featureFlagsUrl: explorerFeatureFlags })

  for (const key in flagsAndVariants.flags) {
    const value = flagsAndVariants.flags[key]
    delete flagsAndVariants.flags[key]
    flagsAndVariants.flags[key.replace(/^explorer-/, '')] = value
  }

  for (const key in flagsAndVariants.variants) {
    const value = flagsAndVariants.variants[key]
    delete flagsAndVariants.variants[key]
    flagsAndVariants.variants[key.replace(/^explorer-/, '')] = value
  }

  if (location.search.length !== 0) {
    const flags = new URLSearchParams(location.search)
    flags.forEach((_, key) => {
      if (key.startsWith(`DISABLE_`)) {
        const featureName = key.replace('DISABLE_', '').toLowerCase() as FeatureFlagsName
        if (featureName in flagsAndVariants.variants) {
          flagsAndVariants.flags[featureName] = true
          flagsAndVariants.variants[featureName].enabled = false
        } else {
          flagsAndVariants.flags[featureName] = false
        }
      } else if (key.startsWith(`ENABLE_`)) {
        const featureName = key.replace('ENABLE_', '').toLowerCase() as FeatureFlagsName
        flagsAndVariants.flags[featureName] = true
        if (featureName in flagsAndVariants.variants) {
          flagsAndVariants.variants[featureName].enabled = true
        } else {
        }
      }
    })
  }

  return flagsAndVariants
}

async function fetchMetaConfiguration(network: ETHEREUM_NETWORK): Promise<Partial<MetaConfiguration>> {
  const serverConfiguration = getServerConfigurations(network)
  const explorerConfigurationEndpoint = serverConfiguration.explorerConfiguration

  try {
    const response = await fetch(explorerConfigurationEndpoint)
    if (response.ok) {
      return response.json()
    }
    throw new Error('Meta Response Not Ok')
  } catch (e) {
    defaultLogger.warn(
      `Error while fetching meta configuration from '${explorerConfigurationEndpoint}' using default config`
    )
    return {
      explorer: {
        minBuildNumber: 0,
        assetBundlesFetchUrl: getAssetBundlesBaseUrl(network)
      },
      servers: {
        added: [],
        denied: [],
        contentWhitelist: []
      },
      bannedUsers: {},
      synapseUrl:
        network === ETHEREUM_NETWORK.MAINNET ? 'https://synapse.decentraland.org' : 'https://synapse.decentraland.zone',
      socialServerUrl:
        network === ETHEREUM_NETWORK.MAINNET ? 'https://social.decentraland.org' : 'https://social.decentraland.zone',
      world: {
        pois: []
      }
    }
  }
}
