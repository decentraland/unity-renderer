import { setRunningTest } from 'config'
setRunningTest(true)

/**
 * Hello, this file is the tests entry point.
 * You should import all your tests here.
 */

beforeEach(() => history.replaceState({}, '', `?`))

/* HELPERS */
import './lib/landHelpers.test'
import './lib/vectorHelpers.test'
import './lib/OrderedRingBuffer.test'
import './lib/SortedLimitedQueue.test'

/* UNIT */
import './unit/ethereum.test'
import './unit/objectComparison.test'
import './unit/jsonFetch.test'
import './unit/profiles.saga.test'
import './unit/getPerformanceInfo.test'
import './unit/positionThings.test'
import './unit/RestrictedActions.test'
import './unit/portableExperiences.test'
import './unit/logger.spec'
import './unit/comms-resolver.test'
import './unit/friends.saga.test'
import './unit/channels.saga.test'
import './unit/world.saga.test'
import './unit/meta.test'

/* SCENE EVENTS */
import './sceneEvents/visibleAvatars.spec'
import './sceneEvents/comms.spec'

import './dao'

declare let mocha: Mocha

mocha.run()
