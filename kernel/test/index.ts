global['isRunningTests'] = true

/**
 * Hello, this file is the tests entry point.
 * You should import all your tests here.
 */

/* HELPERS */
import './atomicHelpers/parcelScenePositions.test'
import './atomicHelpers/landHelpers.test'
import './atomicHelpers/vectorHelpers.test'
import './atomicHelpers/OrderedRingBuffer.test'
import './atomicHelpers/SortedLimitedQueue.test'

/* UNIT */
import './unit/ethereum.test'
import './unit/objectComparison.test'
import './unit/jsonFetch.test'
import './unit/profiles.saga.test'
import './unit/BrowserInterface.test'
import './unit/positionThings.test'
import './unit/RestrictedActions.test'
import './unit/engine.test'
import './unit/catalog.saga.test'
import './unityIntegration/ecs/math/quaternion.test'
import './unityIntegration/ecs/math/vector3.test'

declare var mocha: any
declare var global: any

mocha.run()
