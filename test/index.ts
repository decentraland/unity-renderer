global['isRunningTests'] = true

/**
 * Hello, this file is the tests entry point.
 * You should import all your tests here.
 *
 * And as usual, `import './testHelpers'` must be the first line in the file.
 */

import './testHelpers'

/* HELPERS */
import './atomicHelpers/parcelScenePositions.test'
import './atomicHelpers/landHelpers.test'
import './atomicHelpers/vectorHelpers.test'

/* UNIT */
import './unit/entities.test'
import './unit/passing.test'
import './unit/telemetry.test'
import './unit/positions.test'
import './unit/ethereum.test'
import './unit/schemaValidator.test'
import './unit/objectComparison.test'
import './unit/deepDispose.test'
import './unit/comms.test'
import './unit/communications.test'
import './unit/query-scene-limits'
import './unit/ecs.test'
import './unit/ecsIntegration.test'
import './unityIntegration/ecs/math/quaternion.test'
import './unityIntegration/ecs/math/vector3.test'

/* INTERACTIONS */
import './interactions/click.test'

/* PARCEL SCENES */
import './parcelScenes/unmount.test'
import './parcelScenes/gltfLimits.test'
import './parcelScenes/triangleLimits.test'

/* VISUAL */
import './visualValidation/gamekit.test'
import './visualValidation/visualUnit.test'
import './visualValidation/atlas.test'
import './visualValidation/aframePrimitives.test'
import './visualValidation/avatar.test'
import './visualValidation/skyAndLights.test'
import './visualValidation/crocs.test'
import './visualValidation/parcel-shape.test'
import './visualValidation/changeMaterial.test'
import './visualValidation/text.test'
import './visualValidation/inputText.test'
import './visualValidation/materialMutation.test'
import './visualValidation/inclusiveLimits.test'
import './visualValidation/uiElements.test'
import './visualValidation/lookAt.test'
import './visualValidation/referenceCubes.test'

import './visualValidation/ccPaymentUI.test'

import * as engine from 'engine'
import * as renderer from 'engine/renderer'
global['engine'] = engine
global['renderer'] = renderer

mocha.run()
