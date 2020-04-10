import { call, put, select, take } from 'redux-saga/effects'
import { getServerConfigurations } from '../../config/index'
import { metaConfigurationInitialized, META_CONFIGURATION_INITIALIZED } from './actions'
import defaultLogger from '../logger'
import { buildNumber } from './env'
import { MetaConfiguration } from './types'
import { isMetaConfigurationInitiazed } from './selectors'

const DEFAULT_META_CONFIGURATION: MetaConfiguration = {
  explorer: {
    minBuildNumber: 0
  },
  servers: {
    added: [],
    denied: [],
    contentWhitelist: []
  },
  world: {
    pois: []
  }
}

export function* metaSaga(): any {
  const config: Partial<MetaConfiguration> = yield call(fetchMetaConfiguration)

  yield put(metaConfigurationInitialized(config))
  yield call(checkExplorerVersion, config)
}

function checkExplorerVersion(config: Partial<MetaConfiguration>) {
  const currentBuildNumber = buildNumber
  defaultLogger.info(`Current build number: `, currentBuildNumber)

  if (!config || !config.explorer || !config.explorer.minBuildNumber) {
    return
  }

  if (currentBuildNumber < config.explorer.minBuildNumber) {
    // force client to reload from server
    window.location.reload(true)
  }
}

async function fetchMetaConfiguration() {
  const explorerConfigurationEndpoint = getServerConfigurations().explorerConfiguration
  try {
    const response = await fetch(explorerConfigurationEndpoint)
    return response.ok ? response.json() : DEFAULT_META_CONFIGURATION
  } catch (e) {
    defaultLogger.warn(
      `Error while fetching meta configuration from '${explorerConfigurationEndpoint}' using default config`
    )
    return DEFAULT_META_CONFIGURATION
  }
}

export function* waitForMetaConfigurationInitialization() {
  if (!(yield select(isMetaConfigurationInitiazed))) {
    yield take(META_CONFIGURATION_INITIALIZED)
  }
}
