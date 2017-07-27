/**
 * This file starts up all the required components to make the world viewer
 * work correctly.
 *
 * IMPORTANT: The following imports must follow the order they have. This
 * is done this way because of the interdependencies between components.
 *
 * Every entry point or test should include `import 'src'` as the first line
 * of the document.
 */

// preload configurations
import 'config'

// initialize the renderer and all of the elements
// we will need interact with like the camera
import './renderer'

// initialize the entity system
import './entities'

// register the components that will be used by the entity system
import './components'

export { domReadyFuture } from './renderer'
